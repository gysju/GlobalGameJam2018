using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UglyFadeScript : MonoBehaviour
{

    public Image Image;
    public Text Title;
    public Text Description;
    private Image Fade;

    private static UglyFadeScript main;

    void Awake()
    {
        main = this;
        Fade = GetComponent<Image>();
    }

    public static void LoadNewPanel(DifficultyLevel level)
    {
        main.Image.sprite = level.Image;
        main.Title.text = level.Name;
        main.Description.text = level.Description;
    }

	void Update ()
    {
        main.Image.color = new Color(1, 1, 1, Fade.color.a);
        main.Title.color = new Color(0.8f, 0.8f, 0.8f, Fade.color.a);
        main.Description.color = new Color(0.1f, 0.7f, 0.9f, Fade.color.a);
    }
}
