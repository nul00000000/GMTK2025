using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyGameState : MonoBehaviour
{
    // Start is called before the first frame update
    public static Boolean gamePaused = false;
    private static Boolean prefsInitialized = false;
    private static float musicVolume = -1;
    private static float volume = -1;
    private static float sensitivity = -1;
    const float DEFAULT_VOLUME = .5f;
    
    const float DEFAULT_SENSITIVITY = 1;

    private static void initializePrefs() {
        volume = PlayerPrefs.GetFloat("Volume", DEFAULT_VOLUME);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", DEFAULT_VOLUME);
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", DEFAULT_SENSITIVITY);

        prefsInitialized = true;
    }
    
    
    public static float getPrefVolume() {
        if (!prefsInitialized) initializePrefs();
        return volume;
    }

    public static float getPrefMusicVolume() {
        if (!prefsInitialized) initializePrefs();
        return musicVolume;
    }

    public static float getPrefSensitivity() {
        if (!prefsInitialized) initializePrefs();
        return sensitivity;
    }

    public static void setPrefVolume(float val) {
        PlayerPrefs.SetFloat("Volume", val);
        volume = val;
    }

    public static void setPrefMusicVolume(float val) {
        PlayerPrefs.SetFloat("MusicVolume", val);
        musicVolume = val;
    }

    public static void setPrefSensitivity(float val) {
        sensitivity = val;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
