using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.XR.WSA.Input;
using UnityEngine.UI;
using Vuforia;
using System;

public class VuforiaCameraCapture : MonoBehaviour
{
    // Singleton 
    public static VuforiaCameraCapture S = null;

    //Vuforia Variables
    private Vuforia.Image.PIXEL_FORMAT mPixelFormat = Vuforia.Image.PIXEL_FORMAT.UNKNOWN_FORMAT;

    //private bool mAccessCameraImage = true;
    private bool mFormatRegistered = false;
    private bool mAccessCameraImage = true;
	private bool timedCaptureEnabled = true;
    //test variable
    public float lastCaptureTime = 0f;

    //old version variables

    // GameObjects where images and text are displayed 
    [Header("Image Display Objects")]
    public RawImage m_RawImageSmall;
    public RawImage m_RawImageBig;
    public Text m_sendTextSmall;
    public Text m_sendTextBig;
    public string outText;


    // Photo Capture objects 
    GameObject m_Canvas = null;
    Renderer m_CanvasRenderer = null;
    PhotoCapture m_PhotoCaptureObj;
    CameraParameters m_CameraParameters;
    bool m_CapturingPhoto = false;
    Texture2D m_Texture = null;


    // Start is called before the first frame update
    void Start()
    {
        if(S!=null)
        {
            Debug.LogError("Vuforia Camera Capture Singleton attempted to make duplicate (Static reference not null)");
            return;
        }
        else
        {
            S = this;
            mPixelFormat = Vuforia.Image.PIXEL_FORMAT.RGB888;
            lastCaptureTime = Time.realtimeSinceStartup;
            Vuforia.VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
            Vuforia.VuforiaARController.Instance.RegisterOnPauseCallback(OnPause);
        }
    }

    public void ToggleImage()
    {
        if (!m_RawImageBig.gameObject.activeInHierarchy)
        {
            m_RawImageBig.gameObject.SetActive(true);
            m_RawImageSmall.gameObject.SetActive(false);
            m_sendTextBig.gameObject.SetActive(true);
            m_sendTextSmall.gameObject.SetActive(false);

        }
        else
        {
            m_RawImageBig.gameObject.SetActive(false);
            m_RawImageSmall.gameObject.SetActive(true);
            m_sendTextBig.gameObject.SetActive(false);
            m_sendTextSmall.gameObject.SetActive(true);
        }
    }

    // Displays the image onscreen 
    public void SetImage(Texture2D text)
    {
        Debug.Log("Setting image"); 
        int height = text.height;
        int width = text.width;
        float ratio = (float)height / (float)width;

        //int smallScale = 100;
        //int largeScale = 350; 

        m_RawImageBig.texture = text;
        m_RawImageSmall.texture = text;

        m_RawImageBig.GetComponent<Transform>().localScale = new Vector3(1, ratio, 1);
        m_RawImageSmall.GetComponent<Transform>().localScale = new Vector3(1, ratio, 1);
    }

    // Displays a text message from ground control onscreen 
    public void SetText(string text)
    {
        m_sendTextSmall.text = text;
        m_sendTextBig.text = text;
    }

    public string GetQRStringData()
    {
        return outText;
    }

    public void BeginScanQRCode()
    {
        TrackerManager.Instance.GetTracker<ObjectTracker>().Stop();
        CameraDevice.Instance.Stop();
        lastCaptureTime = Time.realtimeSinceStartup;
        RegisterFormat();
        ScanQRCode();
        TrackerManager.Instance.GetTracker<ObjectTracker>().Start();
        CameraDevice.Instance.Start();
#if !UNITY_EDITOR
            NetworkMeshSource.getSingleton().sendImage(m_Texture,Camera.main.transform.position, Camera.main.transform.rotation);
#endif
        try
        {
            QRCodeChecker qr = QRCodeChecker.getSingleton();
            string o = qr.findQRCodeInImage(m_Texture);
            Debug.Log(o);
            if (o.Length > 0)
            {
                outText = o;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            outText = "Exception Thrown";
            return;
        }
        //m_RawImageBig.texture = m_Texture;
        //m_RawImageBig.mainTexture = m_Texture;
        //m_RawImageBig.SetNativeSize(); holy giant plane batman.
        //m_RawImageBig.material.SetTexture(m_Texture);
        //m_RawImageBig.material.mainTexture = m_Texture;
    }

    private void ScanQRCode()
    {
        if (mFormatRegistered)
        {
            if (mAccessCameraImage)
            {
                Vuforia.Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat); //DC this is where problem is
                if (image != null && image.IsValid())
                {
                    string imageInfo = mPixelFormat + " image: \n";
                    imageInfo += " size: " + image.Width + " x " + image.Height + "\n";
                    imageInfo += " bufferSize: " + image.BufferWidth + " x " + image.BufferHeight + "\n";
                    imageInfo += " stride: " + image.Stride;
                    Debug.Log(imageInfo);
                    byte[] pixels = image.Pixels;

                    if (pixels != null && pixels.Length > 0)
                    {
                        Debug.Log("Image pixels: " + pixels[0] + "," + pixels[1] + "," + pixels[2] + ",...");
                        Texture2D tex = new Texture2D(image.BufferWidth, image.BufferHeight, TextureFormat.RGB24, false); // RGB24
                        tex.LoadRawTextureData(pixels);
                        tex.Apply();
                        m_Texture = tex;
                        m_RawImageBig.texture = tex;
                        m_RawImageBig.material.mainTexture = tex;
                        QRCodeChecker qr = QRCodeChecker.getSingleton();
                        Debug.Log(qr.findQRCodeInImage(m_Texture));
                    }
                }
            }
        }
    }

    public void TakePhoto(bool display)
    {
        if (mFormatRegistered)
        {
            if (mAccessCameraImage)
            {
                Vuforia.Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);
                if (image != null && image.IsValid())
                {
                    /*
                    string imageInfo = mPixelFormat + " image: \n";
                    imageInfo += " size: " + image.Width + " x " + image.Height + "\n";
                    imageInfo += " bufferSize: " + image.BufferWidth + " x " + image.BufferHeight + "\n";
                    imageInfo += " stride: " + image.Stride;
                    Debug.Log(imageInfo);*/
                    byte[] pixels = image.Pixels;

                    if (pixels != null && pixels.Length > 0)
                    {
                        if (display)
                        {
                            //Debug.Log("Image pixels: " + pixels[0] + "," + pixels[1] + "," + pixels[2] + ",...");
                            Texture2D tex = new Texture2D(image.BufferWidth, image.BufferHeight, TextureFormat.RGB24, false); // RGB24
                            tex.LoadRawTextureData(pixels);
                            tex.Apply();
                            m_Texture = tex;
                            //since I noticed this was set to null in the main scene, I decided to save my network code if someone derps.
                            if(m_RawImageBig)
                            {
                                m_RawImageBig.texture = tex;
                                m_RawImageBig.material.mainTexture = tex;
                            }
                            else
                            {
                                Debug.LogError("You didn't assign m_RawImageBig");
                            }
                            if(m_RawImageSmall)
                            { 
                                m_RawImageSmall.texture = tex;                         
                                m_RawImageSmall.material.mainTexture = tex;
                            }
                            else
                            {
                                Debug.LogError("You didn't assign m_RawImageSmall");
                            }
                            ServerConnect.S.sendPicture(m_Texture);                           
                        }
                    }
                }
            }
        }
    }

	public void enableTimedCapture()
	{
		timedCaptureEnabled=true;
	}
	public void disableTimedCapture()
	{
		timedCaptureEnabled=false;
	}
	
    void FixedUpdate()
    {
        /*
        if (lastCaptureTime + 30.0f < Time.realtimeSinceStartup && timedCaptureEnabled)
        {
#if !UNITY_EDITOR
            TrackerManager.Instance.GetTracker<ObjectTracker>().Stop();
#endif
            CameraDevice.Instance.Stop();
            lastCaptureTime = Time.realtimeSinceStartup;
            RegisterFormat();
            this.TakePhoto(false);
#if !UNITY_EDITOR
            TrackerManager.Instance.GetTracker<ObjectTracker>().Start();
#endif
            CameraDevice.Instance.Start();
            //Debug.Log(QRCodeChecker.getSingleton().findQRCodeInImage(m_Texture));
#if !UNITY_EDITOR           
            NetworkMeshSource.getSingleton().sendImage(m_Texture,Camera.main.transform.position, Camera.main.transform.rotation);
#endif
            //check for qr code

            //m_RawImageBig.texture = m_Texture;
            //m_RawImageBig.mainTexture = m_Texture;
            //m_RawImageBig.SetNativeSize(); holy giant plane batman.
            //m_RawImageBig.material.SetTexture(m_Texture);
            //m_RawImageBig.material.mainTexture = m_Texture;
        }
        */

        //if (lastCaptureTime + 10.0f < Time.realtimeSinceStartup)
        //{
        //    lastCaptureTime = Time.realtimeSinceStartup;
        //    BeginScanQRCode();
        //}

    }

    public static VuforiaCameraCapture getSingleton()
    {
        return S;
    }
 
    private void UnregisterFormat()
    {
        Debug.Log("Unregistering camera pixel format " + mPixelFormat.ToString());
        CameraDevice.Instance.SetFrameFormat(mPixelFormat, false);
        mFormatRegistered = false;
    }
    /// <summary>
    /// Register the camera pixel format
    /// </summary>
    private void RegisterFormat()
    {
#if !UNITY_EDITOR
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError("Failed to register camera pixel format " + mPixelFormat.ToString());
            mFormatRegistered = false;
        }
#endif
    }

    private void OnVuforiaStarted()
    {
        // Try register camera image format
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            Debug.Log("Successfully registered pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
        }
        else
        {
            Debug.LogError("Failed to register pixel format " + mPixelFormat.ToString() +
                "\n the format may be unsupported by your device;" +
                "\n consider using a different pixel format.");
            mFormatRegistered = false;
        }
    }

    private void OnPause(bool paused)
    {
        if (paused)
        {
            Debug.Log("App was paused");
            UnregisterFormat();
        }
        else
        {
            Debug.Log("App was resumed");
            RegisterFormat();
        }
    }
}
