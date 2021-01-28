using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningSingleton : MonoBehaviour
{
    public static WarningSingleton m_Singleton;

    [HideInInspector]
    public bool m_DataInWarning = false;

    private List<string> m_Warnings;
    // Start is called before the first frame update
    void Awake()
    {
        m_Singleton = this;
        m_Warnings = new List<string>();
    }

    public void BiometricInWarning(string s)
    {
        if (m_Warnings.Contains(s)) return;

        m_Warnings.Add(s);
        m_DataInWarning = true;
        FindObjectOfType<OutputErrorData>().OutputErrorText("Warning in Biometrics");

        Debug.Log("Adding error");
    }

    public void BiometricInNominal(string s)
    {
        if (m_Warnings.Contains(s))
        {
            m_Warnings.Remove(s);
        }

        if (m_Warnings.Count == 0)
        {
            m_DataInWarning = false;
        }

    }
}
