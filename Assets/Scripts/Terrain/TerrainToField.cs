using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
public class TerrainToField : MonoBehaviour
{

    public ComputeShader Compute;
    private RenderTexture temp = null;
    private int kernel;

    void Start()
    {
        if (null == Compute)
        {
            enabled = false;
            return;
        }

        kernel = Compute.FindKernel("CSMain");
    }

    void OnDestroy()
    {
        if (null != temp)
        {
            temp.Release();
            temp = null;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (null == Compute || kernel < 0 || null == src)
        {
            Graphics.Blit(src, dst);
            return;
        }

        //Recreate temp
        if (null == temp || src.width != temp.width
           || src.height != temp.height)
        {
            if (null != temp)
                temp.Release();
            temp = new RenderTexture(src.width, src.height, src.depth);
            temp.enableRandomWrite = true;
            temp.Create();
        }

        //Apply compute
        Compute.SetTexture(kernel, "src", src);
        Compute.SetTexture(kernel, "dst", temp);
        ShareCameraParameters();
        Vector3Int threadSize = new Vector3Int(Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);
        Compute.Dispatch(kernel, threadSize.x, threadSize.y, threadSize.z);

        //Apply result to dst
        Graphics.Blit(temp, dst);
    }

    void ShareCameraParameters()
    {
        Camera cam = Camera.current;
        Vector2 screen = new Vector2(Mathf.CeilToInt(Screen.width), Mathf.CeilToInt(Screen.height));
        Compute.SetVector("_ViewLU", cam.ScreenPointToRay(new Vector3(0, screen.y, 0)).direction);
        Compute.SetVector("_ViewLD", cam.ScreenPointToRay(new Vector3(0, 0, 0)).direction);
        Compute.SetVector("_ViewRU", cam.ScreenPointToRay(new Vector3(screen.x, screen.y, 0)).direction);
        Compute.SetVector("_ViewRD", cam.ScreenPointToRay(new Vector3(screen.x, 0, 0)).direction);
        Compute.SetVector("_CameraPos", cam.transform.position);
    }
}
