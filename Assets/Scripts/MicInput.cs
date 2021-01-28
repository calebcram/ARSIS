using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MicInput : MonoBehaviour
{
    public static float MicLoudness;
    public AudioClip clipRecord;
    int sampleWindow = 128;
    bool isInitialized;

    float highestVolume = 0.1f;

    float volumeBuffer = 0.1f;
    float bufferDecrease = 0.1f;

    public Image image;

    //mic initialization
    void InitMic()
    {
        clipRecord = Microphone.Start(Microphone.devices[0], true, 999, 44100);
    }

    void StopMicrophone()
    {
        Microphone.End(Microphone.devices[0]);
    }

    //get data from microphone into audioclip
    float LevelMax()
    {
        float levelMax = 0;
        float[] waveData = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(null) - (sampleWindow + 1); // null means the first microphone
        if (micPosition < 0) return 0;
        clipRecord.GetData(waveData, micPosition);
        // Getting a peak on the last 128 samples
        for (int i = 0; i < sampleWindow; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        return levelMax;
    }

    void Update()
    {
        MicLoudness = LevelMax();

        if (MicLoudness > volumeBuffer)
        {
            volumeBuffer = MicLoudness;
            bufferDecrease = 0.005f;
        }
        if (MicLoudness < volumeBuffer)
        {
            volumeBuffer -= bufferDecrease;
            bufferDecrease *= 1.2f;
        }

        if (volumeBuffer > highestVolume)
        {
            highestVolume = volumeBuffer;
        }

        //Debug.Log(volumeBuffer);

        float scale = map(volumeBuffer, 0, highestVolume, 0.3f, 0.5f);

        image.transform.localScale = new Vector3(scale, scale, scale);
    }

    private static float map(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }


    void OnEnable()
    {
        InitMic();
        isInitialized = true;
    }

    void OnDisable()
    {
        StopMicrophone();
    }

    void OnDestroy()
    {
        StopMicrophone();
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {

            if (!isInitialized)
            {

                InitMic();
                isInitialized = true;
            }
        }
        if (!focus)
        {
            StopMicrophone();
            isInitialized = false;

        }
    }
}