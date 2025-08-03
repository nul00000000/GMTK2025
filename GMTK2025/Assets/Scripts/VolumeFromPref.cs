using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeFromPref : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] AudioSource source;
    [SerializeField] bool isMusic = false;
    [SerializeField] float baseVolume = 1;

    void Start()
    {  
        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = GetComponentInChildren<AudioSource>();

        if (isMusic) {
            source.volume = baseVolume * EasyGameState.getPrefMusicVolume();
        } else {
            source.volume = baseVolume * EasyGameState.getPrefVolume();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isMusic) {
            source.volume = baseVolume * EasyGameState.getPrefMusicVolume();
        } else {
            source.volume = baseVolume * EasyGameState.getPrefVolume();
        }
    }
}
