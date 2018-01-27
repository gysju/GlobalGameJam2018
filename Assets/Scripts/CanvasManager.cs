using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour {

    public static CanvasManager _Instance;

    public Image _FadePlane;
    public float fadeTime = 2;

    public delegate void Fade();
    public static event Fade FadeInIsFinished = null;
    public static event Fade FadeOutIsFinished = null;

    public GameObject MainMenu;
    public GameObject FailMenu;
    public GameObject WinMenu;

    private GameObject _currentMenu;

    public void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void PlayGame( GameObject currentMenu )
    {
        bool restartLevel = false;
        if (currentMenu == FailMenu)
            restartLevel = true;

        _currentMenu = currentMenu;

        StartCoroutine(fadein());
        FadeInIsFinished = delegate
        {
            StartCoroutine(GenerateLevel(restartLevel));
            _currentMenu.SetActive(false);
        };
    }

    public void GoToDeathMenu()
    {
        _currentMenu = FailMenu;
        FadeInIsFinished += delegate {
            StartCoroutine(fadeout());
            _currentMenu.SetActive(true);
            LevelGenerator._Instance.ResetPlayer();
        };
        StartCoroutine(fadein());
    }

    public void GoToWinMenu()
    {
        _currentMenu = WinMenu;
        FadeInIsFinished += delegate {
            StartCoroutine(fadeout());
            _currentMenu.SetActive(true);
            LevelGenerator._Instance.CleanLevels();
            LevelGenerator._Instance._Difficulty++;
        };
        StartCoroutine(fadein());
    }

    public void GoToMainMenu(GameObject currentMenu)
    {
        FadeInIsFinished += delegate
        {
            LevelGenerator._Instance.CleanLevels();
            currentMenu.SetActive(false);
            _currentMenu = MainMenu;
            _currentMenu.SetActive(true);
            LevelGenerator._Instance._Difficulty = 1;
            StartCoroutine(fadeout());
        };
        StartCoroutine(fadein());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator GenerateLevel(bool restartLevel, int i = 1)
    {
        if(!restartLevel)
            yield return StartCoroutine(LevelGenerator._Instance.GenerateLevel());

        FadeOutIsFinished = delegate { LevelGenerator._Instance.GeneratePlayer(); };
        yield return StartCoroutine(fadeout());
    }

    public IEnumerator fadeout()
    {
        float t = fadeTime;
        while (t > 0)
        { 
            t -= Time.deltaTime;  
            _FadePlane.color = new Color(_FadePlane.color.r, _FadePlane.color.g, _FadePlane.color.b, t / fadeTime);
            yield return null;
        }

        if (FadeOutIsFinished != null)
            FadeOutIsFinished();
        FadeOutIsFinished = null;
    }

    public IEnumerator fadein()
    {
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            _FadePlane.color = new Color(_FadePlane.color.r, _FadePlane.color.g, _FadePlane.color.b, t / fadeTime);

            yield return null;
        }

        if (FadeInIsFinished != null)
            FadeInIsFinished();
        FadeInIsFinished = null;
    }
}
