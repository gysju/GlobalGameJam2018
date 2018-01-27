using UnityEngine;
using System.Linq;

[ExecuteInEditMode, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
public class TerrainToField : MonoBehaviour
{

    public ComputeShader Compute;
    public Vector4 Utils;
    public LevelGenerator Generator;
    private RenderTexture temp = null;
    private ComputeBuffer nodesBuffer;
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
        if (nodesBuffer != null)
        {
            nodesBuffer.Release();
            nodesBuffer = null;
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

        //Recreate temp rendertarget
        if (null == temp || src.width != temp.width
           || src.height != temp.height)
        {
            if (null != temp)
                temp.Release();
            temp = new RenderTexture(src.width, src.height, src.depth);
            temp.enableRandomWrite = true;
            temp.Create();
        }

        //Init compute
        Compute.SetTexture(kernel, "src", src);
        Compute.SetTexture(kernel, "dst", temp);
        Compute.SetVector("_Utils", Utils);

        //Share terrain
        Node[] nodes = Generator._Points.Select(x => new Node() { Center = x.Center, Radius = 10, Color = x.GetNodeColor()}).ToArray();
        Line[] lines = Generator._Links.Select(x => new Line() { PointA = x._PointA.transform.position, PointB = x._PointB.transform.position }).ToArray();
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            Compute.SetVector("player", player.transform.position);
            Compute.SetVector("playerColor", player._Color);
        }


        if (nodes.Length > 0)
        {
            if (nodesBuffer != null)
                nodesBuffer.Release();
            nodesBuffer = new ComputeBuffer(nodes.Length, sizeof(float) * 8);
            nodesBuffer.SetData(nodes);
            Compute.SetBuffer(kernel, "Nodes", nodesBuffer);
        }

        if (lines.Length > 0)
        {
            if (linesBuffer != null)
                linesBuffer.Release();
            linesBuffer = new ComputeBuffer(lines.Length, sizeof(float) * 6);
            linesBuffer.SetData(lines);
            Compute.SetBuffer(kernel, "Lines", linesBuffer);
        }

        //Apply compute
        ShareCameraParameters();
        Vector3Int threadSize = new Vector3Int(Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);
        Compute.Dispatch(kernel, threadSize.x, threadSize.y, threadSize.z);

        if (nodes.Length > 0)
            nodesBuffer.Release();

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

    public struct Node
    {
        public Vector3 Center;
        public float Radius;
        public Vector4 Color;
    }


}
