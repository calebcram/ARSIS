using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI; 

[RequireComponent(typeof(AudioSource))]
public class MicrophoneFeed : MonoBehaviour
{
    public bool useMicrophone = true;

    private AudioSource source;
    private string device;
    private bool prevUseMicrophone = false;
    private AudioClip prevClip = null;

    public Image image;

    public static float[] samples = new float[512];
    public static float[] freqBands = new float[8];
    public static float[] bandBuffer = new float[8];
    float[] bufferDecrease = new float[8];

    float[] freqBandHighest = new float[8];
    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];
    public static float amplitude, amplitudeBuffer;
    float amplitudeHighest; 

    private void Start()
    {
        source = GetComponent<AudioSource>(); 
    }

    void Update()
    {
        /* if (useMicrophone != prevUseMicrophone)
         {
             prevUseMicrophone = useMicrophone;
             if (useMicrophone)
             {
                 foreach (string m in Microphone.devices)
                 {
                     device = m;
                     break;
                 }

                 source = GetComponent<AudioSource>();
                 prevClip = source.clip;
                 source.Stop();
                 source.clip = Microphone.Start(null, true, 1, AudioSettings.outputSampleRate);
                 source.Play();

                 int dspBufferSize, dspNumBuffers;
                 AudioSettings.GetDSPBufferSize(out dspBufferSize, out dspNumBuffers);

                 source.timeSamples = (Microphone.GetPosition(device) + AudioSettings.outputSampleRate - 3 * dspBufferSize * dspNumBuffers) % AudioSettings.outputSampleRate;
             }
             else
             {
                 Microphone.End(device);
                 source.clip = prevClip;
                 source.Play();
             }
         }*/

        var audio = GetComponent<AudioSource>();
        audio.clip = Microphone.Start(null, true, 10, 44100);
        audio.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { };
        audio.Play(); 

        /* float[] samples = new float[source.clip.samples * source.clip.channels];
         source.clip.GetData(samples, 0);
         //Debug.Log(samples[samples.Length / 2]);
         float scaleFactor = (samples[samples.Length / 2]); // adding 1 to offset the negatives 
         if (scaleFactor == 0)
         {
             scaleFactor = 0.1f; 
         } else
         {
             scaleFactor = map(scaleFactor, -1, 1, 0.1f, 0.5f); 
         }

         Debug.Log(scaleFactor);
         oldScale = image.transform.localScale;
         Vector3 target = new Vector3(scaleFactor, scaleFactor, scaleFactor); 
         Vector3 newScale = Vector3.Lerp(oldScale, target, Time.deltaTime * 10f);

         image.transform.localScale = target; */
        GetSpectrumData();
        MakeFrequencyBands();
        BandBuffer();
        CreateAudioBands();
        GetAmplitude();

        image.transform.localScale = new Vector3((amplitude * 0.5f) + 0.1f, (amplitude * 0.5f) + 0.1f, (amplitude * 0.5f) + 0.1f); 
    }

    void GetSpectrumData()
    {
        source.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        Debug.Log(samples[0]);
    }

    void GetAmplitude()
    {
        float currentAmplitude = 0;
        float currentAmplitudeBuffer = 0; 
        for(int i = 0; i < 8; i++)
        {
            currentAmplitude += audioBand[i];
            currentAmplitudeBuffer += audioBandBuffer[i]; 
        }
        if (currentAmplitude > amplitudeHighest)
        {
            amplitudeHighest = currentAmplitude; 
        }
        amplitude = currentAmplitude / amplitudeHighest;
        amplitudeBuffer = currentAmplitudeBuffer / amplitudeHighest; 
    }

    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBands[i] > freqBandHighest[i])
            {
                freqBandHighest[i] = freqBands[i]; 
            }
            audioBand[i] = (freqBands[i] / freqBandHighest[i]);
            audioBandBuffer[i] = (bandBuffer[i] / freqBandHighest[i]); 
        }
    }

    void BandBuffer()
    {
        for (int g = 0; g < 8; g++)
        {
            if (freqBands[g] > bandBuffer[g])
            {
                bandBuffer[g] = freqBands[g];
                bufferDecrease[g] = 0.005f; 
            }
            if (freqBands[g] < bandBuffer[g])
            {
                bandBuffer[g] -= bufferDecrease[g];
                bufferDecrease[g] *= 1.2f; 
            }
        }
    }

    void MakeFrequencyBands()
    {
        int count = 0; 
        for (int i = 0; i < 8; i++)
        {
            float average = 0; 
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if (i == 7)
            {
                sampleCount += 2; 
            }

            for(int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++; 
            }

            average /= count;

            freqBands[i] = average * 10; 
        }
    }

    private static float map(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
    }
}
