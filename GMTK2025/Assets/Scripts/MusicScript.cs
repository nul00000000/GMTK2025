using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour {
    [SerializeField] AudioSource luteSource;
    [SerializeField] AudioSource electricSource;
    [SerializeField] AudioSource recorderSource;
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

    void Start() {
        
    }

    public void SeekStart() {
        luteSource.time = 0;
        electricSource.time = 0;
        recorderSource.time = 0;
        
        luteSource.Play();
        electricSource.Play();
        recorderSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (EasyGameState.gamePaused || EasyGameState.gameLost) {
            luteSource.Pause();
            electricSource.Pause();
            recorderSource.Pause();

            return;
        } else {
            luteSource.UnPause();
            electricSource.UnPause();
            recorderSource.UnPause();
        }

        Debug.Log(EasyGameState.getPrefMusicVolume());
        luteSource.volume = playState["Lute"] ? 1.0f * EasyGameState.getPrefMusicVolume() : 0.0f;
        electricSource.volume = playState["Electric"] ? 1.0f * EasyGameState.getPrefMusicVolume() : 0.0f;
        recorderSource.volume = playState["Recorder"] ? 1.0f * EasyGameState.getPrefMusicVolume() : 0.0f;
    }
}
