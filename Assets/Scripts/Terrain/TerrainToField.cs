using UnityEngine;
using System.Linq;

[ExecuteInEditMode, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
public class TerrainToField : MonoBehaviour
{

    public ComputeShader Compute;
    public Vector4 Utils;
    public LevelGenerator Generator;
    private RenderTexture temp = null;
    private ComputeBuffer sphereBuffer;
    private ComputeBuffer linesBuffer;
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
        if (temp != null)
        {
            temp.Release();
            temp = null;
        }
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
            sphereBuffer = null;
        }
        if (linesBuffer != null)
        {
            linesBuffer.Release();
            linesBuffer = null;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (null == Compute || kernel < 0 || null == src || Generator == null)
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
        Compute.SetVector("_Utils", Utils);

        Vector4[] points = Generator._Points.Select(x => new Vector4(x.transform.position.x, x.transform.position.y, x.transform.position.z, 10)).ToArray();
        Line[] Lines = Generator._Links.Select(x => new Line() { PointA = x._PointA.transform.position, PointB = x._PointB.transform.position }).ToArray();
        Player player = Object.FindObjectOfType<Player>();
        if (player != null)
            Compute.SetVector("player", player.transform.position);

        if (points.Length > 0)
        {
            if (sphereBuffer != null)
                sphereBuffer.Release();
            sphereBuffer = new ComputeBuffer(points.Length, sizeof(float) * 4);
            sphereBuffer.SetData(points);
            Compute.SetBuffer(kernel, "spheres", sphereBuffer);
        }

        if (Lines.Length > 0)
        {
            if (linesBuffer != null)
                linesBuffer.Release();
            linesBuffer = new ComputeBuffer(Lines.Length, sizeof(float) * 6);
            linesBuffer.SetData(Lines);
            Compute.SetBuffer(kernel, "lines", linesBuffer);
        }

        ShareCameraParameters();
        Vector3Int threadSize = new Vector3Int(Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);
        Compute.Dispatch(kernel, threadSize.x, threadSize.y, threadSize.z);

        if (points.Length > 0)
            sphereBuffer.Release();

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

    public struct Line
    {
        public Vector3 PointA;
        public Vector3 PointB;
    }


}
