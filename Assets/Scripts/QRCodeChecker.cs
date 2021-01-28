using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;

public class QRCodeChecker : MonoBehaviour
{
    //private BarcodeReader barCodeReader;
    private BarcodeReader barcodeReader = null;
    private static QRCodeChecker QRCodeCheckerSingleton = null;

    void Start()
    {
        if(QRCodeCheckerSingleton)
        {
            Debug.LogError("QRCodeChecker singleton already made. Error bastages!");
            return;
        }
        QRCodeCheckerSingleton = this;
        barcodeReader = new BarcodeReader();
    }

    public string findQRCodeInImage(Texture2D image)
    {
        if(barcodeReader==null)
            barcodeReader = new BarcodeReader();
        string o = "";
        
        var data = barcodeReader.Decode(image.GetRawTextureData(), image.width, image.height, RGBLuminanceSource.BitmapFormat.RGB24);

        if (data != null)
        {
            // QRCode detected.
            Debug.Log(data.Text);
            o = data.Text;
        }


        return o;
    }

    public static QRCodeChecker getSingleton()
    {
        return QRCodeCheckerSingleton;
    }
}
