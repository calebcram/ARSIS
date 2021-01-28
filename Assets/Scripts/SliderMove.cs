using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls movement of slider objects. 
/// </summary>
public class SliderMove : MonoBehaviour {
    // The slider to be moved 
    public GameObject slider;

    // Movement parameters 
    public float minPosX = 0f;
    public float maxPosX = 0.3f; 
    private float interval = 0.04f; 

    public void Increase()
    {
        Move(-interval);
    }

    public void Decrease()
    {
        Move(interval); 
    }

    private void Move(float distance)
    {
        slider.transform.Translate(new Vector3(distance, 0, 0)); 
    }
}
