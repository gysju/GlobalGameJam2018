using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LookUpCreator : MonoBehaviour
{

    public Material Target;
    public string Name;
    public Gradient Gradient;
    public TextureWrapMode WarpMode;
    public bool Refresh;
    public bool KeepRefreshing;

    void Update ()
    {
        if (KeepRefreshing)
            Refresh = true;

        if (Refresh)
        {
            Refresh = false;
            Target.SetTexture(Name, GradientToLUT(Gradient));
        }
    }

    public Texture2D GradientToLUT(Gradient gradient)
    {
        Texture2D tex = new Texture2D(256, 1);
        for (int i = 0; i < 256; i++)
        {
            Color color = gradient.Evaluate(i / 256f);
            tex.SetPixel(i, 0, color);
        }
        tex.Apply();
        tex.wrapMode = WarpMode;
        return tex;
    }

}
