using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;

public enum BakedState
{
    NeverBaked = 0,
    Baked = 1,
    UpdatePostBake = 2
}

// Data that is kept to prioritize surface baking.
class SurfaceEntry
{
    public GameObject m_Surface; // the GameObject corresponding to this surface
    public int m_Id; // ID for this surface
    public DateTime m_UpdateTime; // update time as reported by the system
    public BakedState m_BakedState;
    public const float c_Extents = 5.0f;
}

public class MeshDataGatherer : MonoBehaviour
{
    // This observer is the window into the spatial mapping world.  
    SurfaceObserver m_Observer;

    // This dictionary contains the set of known spatial mapping surfaces.
    // Surfaces are updated, added, and removed on a regular basis.
    Dictionary<int, SurfaceEntry> m_Surfaces;
    List<SurfaceEntry> SurfacesList = null;
    // This is the material with which the baked surfaces are drawn.  
    public Material m_drawMat;

    bool isRendering = true; 

    // This flag is used to postpone requests if a bake is in progress.  Baking mesh
    // data can take multiple frames.  This sample prioritizes baking request
    // order based on surface data surfaces and will only issue a new request
    // if there are no requests being processed.
    bool m_WaitingForBake;

    // This is the last time the SurfaceObserver was updated.  It is updated no 
    // more than every two seconds because doing so is potentially time-consuming.
    float m_lastUpdateTime;
    float lastMeshDownlinkTime = 0;

    public static MeshDataGatherer S; 

    void Start()
    {
        m_Observer = new SurfaceObserver();
        m_Observer.SetVolumeAsSphere(new Vector3(0.0f, 0.0f, 0.0f),200.0f);
        SurfacesList = new List<SurfaceEntry>();
        m_Surfaces = new Dictionary<int, SurfaceEntry>();
        m_WaitingForBake = false;
        m_lastUpdateTime = 0.0f;
        S = this; 
    }

    public void disableMeshDisplay()
    {
        isRendering = false; 
        for (int i = 0; i < SurfacesList.Count; i++)
        {
            SurfacesList[i].m_Surface.GetComponent<MeshRenderer>().enabled = isRendering; // SetActive(isRendering); 
        }
    }

    public void enableMeshDisplay()
    {
        isRendering = true;
        for (int i = 0; i < SurfacesList.Count; i++)
        {
            SurfacesList[i].m_Surface.GetComponent<MeshRenderer>().enabled = isRendering; 
        }
    }

    private void FixedUpdate()
    {
        if (lastMeshDownlinkTime + 10.0f < Time.realtimeSinceStartup)
        {
            // you can't block here and wait for the camera capture.
            // Send the old data and trigger a new capture.
            // NetworkMeshSource.getSingleton()           
            for (int index = 0; index < SurfacesList.Count; index++)
            {
                SurfaceEntry item = SurfacesList[index];
                if(item.m_BakedState== BakedState.Baked || item.m_BakedState == BakedState.UpdatePostBake)
                {
                    GameObject go = item.m_Surface;
                    if (go)
                    {
                        MeshFilter[] meshFilters = go.GetComponents<MeshFilter>();
                        for (int mfi = 0; mfi < meshFilters.Length; mfi++)
                        { 
                            MeshFilter MFer = meshFilters[mfi];

                            if (MFer)
                            {

                                Mesh meesh = MFer.mesh;
                                if (meesh&&meesh.triangles.Length>0)
                                {
    #if !UNITY_EDITOR
                                NetworkMeshSource.getSingleton().sendMesh(meesh,
                                        go.transform.position,
                                        go.transform.rotation);
    #endif
                                }
                            }
                        }
                    }
                }              
            }
            lastMeshDownlinkTime = Time.realtimeSinceStartup;
        }
    }

    void Update()
    {
        // Avoid calling Update on a SurfaceObserver too frequently.
        if (m_lastUpdateTime + 5.0f < Time.realtimeSinceStartup)
        {
            // This block makes the observation volume follow the camera.
            Vector3 extents;
            extents.x = SurfaceEntry.c_Extents * 8.0f;
            extents.y = SurfaceEntry.c_Extents * 8.0f;
            extents.z = SurfaceEntry.c_Extents * 8.0f;
            m_Observer.SetVolumeAsAxisAlignedBox(Camera.main.transform.position, extents);
            
            try
            {
                m_Observer.Update(SurfaceChangedHandler);
            }
            catch
            {
                // Update can throw an exception if the specified callback was bad.
                Debug.Log("Observer update failed unexpectedly!");
            }
            
            m_lastUpdateTime = Time.realtimeSinceStartup;
        }

        
        if (!m_WaitingForBake)
        {
            // Prioritize older adds over other adds over updates.
            SurfaceEntry bestSurface = null;
            foreach (KeyValuePair<int, SurfaceEntry> surface in m_Surfaces)
            {
                if (surface.Value.m_BakedState != BakedState.Baked)
                {
                    if (bestSurface == null)
                    {
                        bestSurface = surface.Value;
                    }
                    else
                    {
                        if (surface.Value.m_BakedState < bestSurface.m_BakedState)
                        {
                            bestSurface = surface.Value;
                        }
                        else if (surface.Value.m_UpdateTime < bestSurface.m_UpdateTime)
                        {
                            bestSurface = surface.Value;
                        }
                    }
                }
            }
            if (bestSurface != null)
            {
                // Fill out and dispatch the request.
                SurfaceData sd;
                sd.id.handle = bestSurface.m_Id;
                sd.outputMesh = bestSurface.m_Surface.GetComponent<MeshFilter>();
                sd.outputAnchor = bestSurface.m_Surface.GetComponent<WorldAnchor>();
                sd.outputCollider = bestSurface.m_Surface.GetComponent<MeshCollider>();
                sd.trianglesPerCubicMeter = 500.0f;
                sd.bakeCollider = true;
                try
                {
                    if (m_Observer.RequestMeshAsync(sd, SurfaceDataReadyHandler))
                    {
                        m_WaitingForBake = true;
                    }
                    else
                    {
                        // A return value of false when requesting meshes 
                        // typically indicates that the specified surface
                        // ID specified was invalid.
                        Debug.Log(System.String.Format("Bake request for {0} failed.  Is {0} a valid Surface ID?", bestSurface.m_Id));
                    }
                }
                catch
                {
                    // Requests can fail if the data struct is not filled out
                    // properly.  
                    Debug.Log(System.String.Format("Bake for id {0} failed unexpectedly!", bestSurface.m_Id));
                }
            }
        }
    }

    // This handler receives surface changed events and is propagated by the 
    // Update method on SurfaceObserver.  
    void SurfaceChangedHandler(SurfaceId id, SurfaceChange changeType, Bounds bounds, DateTime updateTime)
    {
        SurfaceEntry entry;
        switch (changeType)
        {
            case SurfaceChange.Added:
            case SurfaceChange.Updated:
                if (m_Surfaces.TryGetValue(id.handle, out entry))
                {
                    // If this surface has already been baked, mark it as needing bake
                    // in addition to the update time so the "next surface to bake" 
                    // logic will order it correctly.  
                    if (entry.m_BakedState == BakedState.Baked)
                    {
                        entry.m_BakedState = BakedState.UpdatePostBake;
                        entry.m_UpdateTime = updateTime;
                    }
                }
                else
                {
                    // This is a brand new surface so create an entry for it.
                    entry = new SurfaceEntry();
                    entry.m_BakedState = BakedState.NeverBaked;
                    entry.m_UpdateTime = updateTime;
                    entry.m_Id = id.handle;
                    entry.m_Surface = new GameObject(System.String.Format("Surface-{0}", id.handle));
                    //SetActive(isRendering); 
                    entry.m_Surface.AddComponent<MeshFilter>();
                    entry.m_Surface.AddComponent<MeshCollider>();
                    MeshRenderer mr = entry.m_Surface.AddComponent<MeshRenderer>();
                    mr.shadowCastingMode = ShadowCastingMode.Off;
                    mr.receiveShadows = false;
                    entry.m_Surface.AddComponent<WorldAnchor>();
                    entry.m_Surface.GetComponent<MeshRenderer>().sharedMaterial = m_drawMat;
                    entry.m_Surface.GetComponent<MeshRenderer>().enabled = isRendering;
                    m_Surfaces[id.handle] = entry;
                    if(!SurfacesList.Contains(entry))
                        SurfacesList.Add(entry);
                }
                break;

            case SurfaceChange.Removed:
                if (m_Surfaces.TryGetValue(id.handle, out entry))
                {
                    m_Surfaces.Remove(id.handle);
                    Mesh mesh = entry.m_Surface.GetComponent<MeshFilter>().mesh;
                    if (mesh)
                    {
                        Destroy(mesh);
                    }
                    Destroy(entry.m_Surface);
                }
                break;
        }
    }

    void SurfaceDataReadyHandler(SurfaceData sd, bool outputWritten, float elapsedBakeTimeSeconds)
    {
        m_WaitingForBake = false;
        SurfaceEntry entry;
        if (m_Surfaces.TryGetValue(sd.id.handle, out entry))
        {
            // These two asserts are checking that the returned filter and WorldAnchor
            // are the same ones that the data was requested with.  That should always
            // be true here unless code has been changed to replace or destroy them.
            Assert.IsTrue(sd.outputMesh == entry.m_Surface.GetComponent<MeshFilter>());
            Assert.IsTrue(sd.outputAnchor == entry.m_Surface.GetComponent<WorldAnchor>());
            entry.m_BakedState = BakedState.Baked;
        }
        else
        {
            Debug.Log(System.String.Format("Paranoia:  Couldn't find surface {0} after a bake!", sd.id.handle));
            Assert.IsTrue(false);
        }
    }
}