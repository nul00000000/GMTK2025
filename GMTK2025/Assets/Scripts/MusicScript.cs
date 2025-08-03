using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour {
    [SerializeField] AudioSource luteSource;
    [SerializeField] AudioSource electricSource;
    [SerializeField] AudioSource recorderSource;
    [SerializeField] AudioSource altoSource;

    private Dictionary<string, bool> playState = new Dictionary<string, bool>();

    // Start is called before the first frame update

    public void setLute(bool val) {
        playState["Lute"] = val;
    }

    public void setElectric(bool val) {
        playState["Electric"] = val;
    }

    public void setRecorder(bool val) {
        playState["Recorder"] = val;
    }

    public void setAlto(bool val) {
        playState["Alto"] = val;
    }

    void Start() {
        luteSource.volume = 1.0f * EasyGameState.getPrefMusicVolume();
        electricSource.volume = 0;
        recorderSource.volume = 0;
        altoSource.volume = 0;
    }

    public void SeekStart() {
        luteSource.time = 0;
        electricSource.time = 0;
        recorderSource.time = 0;
        altoSource.time = 0;
        
        luteSource.Play();
        electricSource.Play();
        recorderSource.Play();
        altoSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (EasyGameState.gamePaused || EasyGameState.gameLost) {
            luteSource.Pause();
            electricSource.Pause();
            recorderSource.Pause();
            altoSource.Pause();

            return;
        } else {
            luteSource.UnPause();
            electricSource.UnPause();
            recorderSource.UnPause();
            altoSource.UnPause();

        }

        // Debug.Log(EasyGameState.getPrefMusicVolume());
        luteSource.volume = playState["Lute"] ? 1.0f * EasyGameState.getPrefMusicVolume() : 0.0f;
        electricSource.volume = playState["Electric"] ? 1.0f * EasyGameState.getPrefMusicVolume() : 0.0f;
        recorderSource.volume = playState["Recorder"] ? 1.0f * EasyGameState.getPrefMusicVolume() : 0.0f;
        altoSource.volume = playState["Alto"] ? 1.0f * EasyGameState.getPrefMusicVolume() : 0.0f;
    }
}
