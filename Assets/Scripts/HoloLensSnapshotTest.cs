using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.XR.WSA.Input;
using UnityEngine.UI;

/// <summary>
/// This class takes pictures from the HoloLens camera. 
/// NOTE: Using HoloLens live stream from the camera may interfere with 
///       this class. If you need to record or live stream from the 
///       HoloLens while using the capture functionality, make sure 
///       that you are only accessing holograms and not the camera. 
///       
/// Code modified from the following link: https://forum.unity.com/threads/holographic-photo-blending-with-photocapture.416023/
/// </summary>
public class HoloLensSnapshotTest : MonoBehaviour
{
    // Singleton 
    public static HoloLensSnapshotTest S;

    // GameObjects where images and text are displayed 
    [Header("Image Display Objects")]
    public RawImage m_RawImageSmall;
    public RawImage m_RawImageBig;
    public Text m_sendTextSmall;
    public Text m_sendTextBig; 

    // Photo Capture objects 
    GameObject m_Canvas = null;
    Renderer m_CanvasRenderer = null;
    PhotoCapture m_PhotoCaptureObj;
    CameraParameters m_CameraParameters;
    bool m_CapturingPhoto = false;
    Texture2D m_Texture = null;

    void Start()
    {
        S = this; 
    }

    public void TakePhoto()
    {
        InitializeCamera();
    }

    //Set up camera and take a photo 
    void InitializeCamera()
    {
        List<Resolution> resolutions = new List<Resolution>(PhotoCapture.SupportedResolutions);
        
        Resolution selectedResolution = resolutions[1];
        //for (int i = 0; i < resolutions.Count; i++)
        //{
        //    Debug.Log("Resolution " + i + " :" + resolutions[i]); 
        //}
        

        m_CameraParameters = new CameraParameters(WebCamMode.PhotoMode);
        m_CameraParameters.cameraResolutionWidth = selectedResolution.width;
        m_CameraParameters.cameraResolutionHeight = selectedResolution.height;
        m_CameraParameters.hologramOpacity = 0.0f;
        m_CameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

        m_Texture = new Texture2D(selectedResolution.width, selectedResolution.height, TextureFormat.BGRA32, false);

        PhotoCapture.CreateAsync(false, OnCreatedPhotoCaptureObject); 
    }

    void OnCreatedPhotoCaptureObject(PhotoCapture captureObject)
    {
        m_PhotoCaptureObj = captureObject;
        m_PhotoCaptureObj.StartPhotoModeAsync(m_CameraParameters, OnStartPhotoMode);
    }

    void OnStartPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Prevents us from taking two photos at the same time 
        if (m_CapturingPhoto)
        {
            return;
        }

        m_CapturingPhoto = true;
        m_PhotoCaptureObj.TakePhotoAsync(OnPhotoCaptured);
    }

    void OnPhotoCaptured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        photoCaptureFrame.UploadImageDataToTexture(m_Texture);
        m_Texture.wrapMode = TextureWrapMode.Clamp;

        // Display the image 
        SetImage(m_Texture); 

        // Send picture to the server 
        ServerConnect.S.sendPicture(m_Texture);
        
        m_CapturingPhoto = false;

        // Close the camera 
        m_PhotoCaptureObj.StopPhotoModeAsync(onStoppedPhotoMode); 
    }
    
    void onStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        m_PhotoCaptureObj.Dispose();
        m_PhotoCaptureObj = null; 
    }

    // Displays the image onscreen 
    public void SetImage(Texture2D text)
    {
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

    // Toggles the size of image and text on display 
    public void ToggleImage()
    {
        if (!m_RawImageBig.gameObject.activeInHierarchy)
        {
            m_RawImageBig.gameObject.SetActive(true);
            m_RawImageSmall.gameObject.SetActive(false);
            m_sendTextBig.gameObject.SetActive(true);
            m_sendTextSmall.gameObject.SetActive(false); 

        } else
        {
            m_RawImageBig.gameObject.SetActive(false);
            m_RawImageSmall.gameObject.SetActive(true);
            m_sendTextBig.gameObject.SetActive(false);
            m_sendTextSmall.gameObject.SetActive(true);
        }
    }
}
