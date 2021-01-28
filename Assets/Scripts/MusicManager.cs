using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all playing of music, which is the most important function of ARSIS. 
/// </summary>
public class MusicManager : MonoBehaviour {

    // Singleton
    public static MusicManager m_Instance;

    [Header("Song MP3s")]
    public AudioSource m_Source;
    public AudioClip m_AdeleSong;
    public AudioClip m_Africa;
    public AudioClip m_Skyfall;
    public AudioClip m_SpaceOddity;
    public AudioClip m_Thunderstruck;
    public AudioClip m_Eclipse;
    public AudioClip m_RocketMan;

	void Start () {
        m_Instance = this;
	}

    public void PlaySong(AudioClip clip)
    {
        if (!m_Source.isPlaying || m_Source.clip != clip)  // Audio is not already playing or is playing a different clip 
        {
            m_Source.clip = clip;
            m_Source.Play();
        } 
    }

    public void StopMusic()
    {
        m_Source.Stop();
    }
}
