using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUiScript : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] TMP_InputField sensitivityInput;

    public void onVolumeValueChanged(float val) {
        EasyGameState.setPrefVolume(volumeSlider.value);
    }

    public void onMusicVolumeValueChanged(float val) {
        EasyGameState.setPrefMusicVolume(musicSlider.value);
    }

    private void setSens(float val) {
        EasyGameState.setPrefSensitivity(val);
        sensitivityInput.text = val.ToString("F2");
    }
    public void onSensitivitySliderChanged(float val) {
        setSens(sensitivitySlider.value);
    }

    public void onSensitivityInputUnfocused(String text) {
        float ou;
        if (float.TryParse(sensitivityInput.text, out ou)) {
            EasyGameState.setPrefSensitivity(ou);
        } else {
            setSens(EasyGameState.getPrefSensitivity());
        }
    }


    void Start()
    {
        volumeSlider.value = EasyGameState.getPrefVolume();
        musicSlider.value = EasyGameState.getPrefMusicVolume();
        sensitivitySlider.value = Math.Clamp(EasyGameState.getPrefSensitivity(), 0, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
