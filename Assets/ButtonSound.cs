using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class ButtonSound : MonoBehaviour, ISelectHandler
{
    public string[] Move = { "SndGuiMove", "SndGuiMove2" };

    private Button _Button;
    private AudioSource _AudioSource;

    void Start ()
    {
        _Button = GetComponent<Button>();
        _AudioSource = GetComponent<AudioSource>();
        _Button.onClick.AddListener(OnClick);
    }

    void OnClick ()
    {
        if (_AudioSource == null)
            _AudioSource = GetComponent<AudioSource>();

        SoundManager.Instance.PlaySoundOneShot("SndGuiSelect", _AudioSource);
	}

    public void OnSelect(BaseEventData eventData)
    {
        if (_AudioSource == null)
            _AudioSource = GetComponent<AudioSource>();

        SoundManager.Instance.PlaySoundOneShot(Move[Random.Range(0,2)], _AudioSource);
    }
}
