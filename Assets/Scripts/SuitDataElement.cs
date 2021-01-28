using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuitDataElement : MonoBehaviour
{
    [Header("Attach this to a game object to have its color changed based on a value you pass in.")]
    [Header("Color settings. Blends these colors to make the final displayed color")]
    public Color warningColor = Color.red;
    public Color cautionColor = Color.yellow;
    public Color nominalColor = Color.blue;

    public string m_DataTitle;
    public float m_DataValue;
    public string m_DataUnitName;

    [Header("Place your color threshholds here. Each value must be smaller than the next.")]
    public float upperWarning; 
    public float upperCaution; 
    public float nominalValue; 
    public float lowerCaution; 
    public float lowerWarning; 

    private Text m_BiometricTitle;
    private Text m_BiometricValue;
    private Image m_BackgroundImage;
    // Start is called before the first frame update
    void Start()
    {
        m_BackgroundImage = transform.GetChild(0).gameObject.GetComponent<Image>();
        m_BiometricTitle = transform.GetChild(5).gameObject.GetComponent<Text>(); //title is 6th child element in biometric data
        m_BiometricValue = transform.GetChild(4).gameObject.GetComponent<Text>(); //value is 5th child element in biometric data
    }

    public void SetData(string dataTitle, float dataValue, float lower, float upper, string unit)
    {
        if (m_BiometricTitle == null || m_BiometricValue == null) return;

        m_BiometricTitle.text = dataTitle;
        m_BiometricValue.text = dataValue.ToString();
        lowerWarning = lower;
        nominalValue = (lower + upper) / 2.0f;
        upperWarning = upper;
        upperCaution = upperWarning - ((upperWarning - nominalValue) * .15f); //get 15% of distance between nominal and warning
        lowerCaution = lowerWarning - ((nominalValue - lowerWarning) * .15f); //get 15% of distance between nominal and warning

        m_DataValue = dataValue; 
        m_DataUnitName = unit; 
    }


    public void SetData(string dataTitle, string dataValue)
    {
        if (m_BiometricTitle == null || m_BiometricValue == null) return;

        m_BiometricTitle.text = dataTitle;
        m_BiometricValue.text = dataValue;

        lowerWarning = 1;
        lowerCaution = 1.001f; 
        nominalValue = 0;
        upperCaution = 1.999f; 
        upperWarning = 2; 
    }

    void Update()
    {
        //debug code
        //m_DataValue += 0.01f;
        //if (m_DataValue > 4.0f) 
        //    m_DataValue = 0;

        if (m_BackgroundImage != null)
        {
            m_BackgroundImage.color = GetCurrentColor();
        }
    }

    public Color GetCurrentColor()
    {
        /*
        if (m_DataValue > upperWarning)
        {
            // "SYSTEM CRITICAL";
            return warningColor;
        }
        else if (m_DataValue < upperWarning && m_DataValue > upperCaution)
        {
            //"System Warning";
            float Offset = m_DataValue - upperCaution;
            float dif = Mathf.Abs(upperWarning - upperCaution);
            float val = Offset / dif;
            return Color.Lerp(cautionColor, warningColor, val);
        }
        else if (m_DataValue < upperCaution && m_DataValue >= nominalValue)
        {
            // "Systems Nominal";
            float Offset = m_DataValue - nominalValue;
            float dif = Mathf.Abs(upperCaution - nominalValue);
            float val = Offset / dif;
            return Color.Lerp(nominalColor, cautionColor, val);
        } 
        else if (m_DataValue <= nominalValue && m_DataValue >= lowerCaution)
        {
            // "Systems Nominal";
            float Offset = m_DataValue - lowerCaution;
            float dif = Mathf.Abs(nominalValue - lowerCaution);
            float val = Offset / dif;
            return Color.Lerp(cautionColor, nominalColor, val);
        }
        else if (m_DataValue < lowerCaution && m_DataValue > lowerWarning)
        {
            // "System Warning";
            float Offset = m_DataValue - lowerWarning;
            float dif = Mathf.Abs(lowerCaution - lowerWarning);
            float val = Offset / dif;
            return Color.Lerp(warningColor, cautionColor, val);
        }
        // "SYSTEM CRITICAL";
        return warningColor;*/

        if (m_DataValue > upperWarning)
        {
            return warningColor;
        }
        else if (m_DataValue <= upperWarning && m_DataValue > upperCaution)
        {
            return cautionColor;
        }
        else if (m_DataValue <= upperCaution && m_DataValue > lowerCaution)
        {
            return nominalColor;
        }
        else if (m_DataValue > lowerWarning && m_DataValue <= lowerCaution)
        {
            return cautionColor;
        }
        else if (m_DataValue < lowerWarning)
        {
            return warningColor;
        }
        else
        {
            //Debug.Log("Returning bad telemetry value " + m_DataValue + " in " + name);
            return warningColor;
        }
    }
}
