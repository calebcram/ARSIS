using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Envelope Chameleon
 * A color tweak script by Daniel Lambert
 * Attach to game object you want the color to change on.
 * Set your caution, warning, and nominal colors, and set your evelope parameters. 
 * Call setCurrentValue(float val) to change the color.
 * Call getCurrentColor() t get a color object based on the current value.
*/

public class EnvelopeChameleon : MonoBehaviour
{
    [Header("Attach this to a game object to have its color changed based on a value you pass in.")]
    [Header ("Color settings. Blends these colors to make the final displayed color")]
    public Color warningColor = Color.red;
    public Color cautionColor = Color.yellow;
    public Color nominalColor = Color.blue;

    [Header("Place Biometric Info Here")]
    public string metricName;
    public string unit;

    [Header("Place your color threshholds here. Each value must be smaller than the next.")]
    public float upperWarning = 4.0f;
    public float upperCaution = 3.0f;    
    public float nominalValue = 2.0f;
    public float lowerCaution = 1.0f;
    public float lowerWarning = 0.0f;

    private float currentValue = 2.0f;
    private Material mat = null;
    private Renderer rend = null;
    private Text nameText; 
    private Text value;
    private Text status;

    void Awake()
    {
        rend = this.gameObject.GetComponent<Renderer>();
        //Debug.Log(rend); 
        if (rend != null)
            mat = new Material(rend.material);
        value = this.gameObject.GetComponent<Transform>().Find("Value").GetComponent<Text>();
        status = this.gameObject.GetComponent<Transform>().Find("Status").GetComponent<Text>();
        nameText = this.gameObject.GetComponent<Transform>().Find("Name").GetComponent<Text>();
        nameText.text = metricName; 
    }

    public void setCurrentValue(float val)
    {
        if (value == null)
        {
            value = this.gameObject.GetComponent<Transform>().Find("Value").GetComponent<Text>();
        }
        currentValue = val;
        value.text = currentValue.ToString() + " " + unit; 
    }

    public void setCurrentValue(string val)
    {
        if (value == null)
        {
            value = this.gameObject.GetComponent<Transform>().Find("Value").GetComponent<Text>();
        }
        // set current value?? 
        value.text = val + " " + unit;
    }

    void Update()
    {
        //debug code
        //currentValue += 0.01f;
        //if (currentValue > 4.0f) 
        //    currentValue = 0;

       if(mat!=null)
        {
            mat.color = getCurrentColor();
            rend.material = mat;
        }
       if(this.gameObject.GetComponent<Image>()!=null)
        {
            this.gameObject.GetComponent<Image>().color = getCurrentColor();
        }
    }

    public Color getCurrentColor()
    {
        if (currentValue > upperWarning)
        {
            status.text = "SYSTEM CRITICAL";
            return warningColor;
        }
        else if(currentValue < upperWarning&&currentValue > upperCaution)
        {
            status.text = "System Warning";
            float Offset = currentValue - upperCaution;
            float dif = Mathf.Abs(upperWarning - upperCaution);
            float val = Offset / dif;
            return Color.Lerp(cautionColor, warningColor, val);
        }
        else if (currentValue < upperCaution && currentValue > nominalValue)
        {
            status.text = "Systems Nominal";
            float Offset = currentValue - nominalValue;
            float dif = Mathf.Abs(upperCaution - nominalValue);
            float val = Offset / dif;
            return Color.Lerp(nominalColor, cautionColor,  val);
        }
        else if (currentValue < nominalValue && currentValue > lowerCaution)
        {
            status.text = "Systems Nominal";
            float Offset = currentValue - lowerCaution;
            float dif = Mathf.Abs(nominalValue - lowerCaution);
            float val = Offset / dif;
            return Color.Lerp(cautionColor, nominalColor,  val);
        }
        else if (currentValue < lowerCaution && currentValue > lowerWarning)
        {
            status.text = "System Warning";
            float Offset = currentValue - lowerWarning;
            float dif = Mathf.Abs(lowerCaution - lowerWarning);
            float val = Offset / dif;
            return Color.Lerp(warningColor, cautionColor,  val);
        }
        status.text = "SYSTEM CRITICAL";
        return warningColor;
    }
}
