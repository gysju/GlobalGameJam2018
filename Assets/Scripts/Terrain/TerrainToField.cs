using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
public class TerrainToField : MonoBehaviour
{

    public ComputeShader Compute;
    public Vector4 Utils;
    public LevelGenerator Generator;
    public float DrawRange;
    private RenderTexture temp = null;
    private ComputeBuffer nodesBuffer;
    private ComputeBuffer linesBuffer;
    private int kernel;

    void OnDrawGizmos()
    {
        Vector3 left = transform.position + Vector3.left * DrawRange;
        Vector3 right = transform.position + Vector3.right * DrawRange;
        Gizmos.color = new Color(1, 0.7f, 0, 1f);
        Gizmos.DrawLine(left + Vector3.down * 20, left + Vector3.up * 20);
        Gizmos.DrawLine(right + Vector3.down * 20, right + Vector3.up * 20);
    }

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
        float leftThreshold = transform.position.x - DrawRange;
        float rightThreshold = transform.position.x + DrawRange;

        List<Node> nodes = new List<Node>();
        for (int i = 0; i < Generator._Points.Count; i++)
        {
            Point current = Generator._Points[i];
            if (current.Center.x < leftThreshold || current.Center.x > rightThreshold)
                continue;
            nodes.Add(new Node() { Center = current.Center, Radius = 10, Color = current.GetNodeColor()});
        }

        List<Line> lines = new List<Line>();
        for (int i = 0; i < Generator._Links.Count; i++)
        {
            Link current = Generator._Links[i];
            if (current._PointA.Center.x < leftThreshold || current._PointA.Center.x > rightThreshold &&
                current._PointB.Center.x < leftThreshold || current._PointB.Center.x > rightThreshold)
                continue;
            lines.Add(new Line() { PointA = current._PointA.Center, PointB = current._PointB.Center });
        }

        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            Compute.SetVector("player", player.transform.position);
            Compute.SetVector("playerColor", player._Color);
        }


        if (nodes.Count > 0)
        {
            if (nodesBuffer != null)
                nodesBuffer.Release();
            nodesBuffer = new ComputeBuffer(nodes.Count, sizeof(float) * 8);
            nodesBuffer.SetData(nodes);
            Compute.SetBuffer(kernel, "Nodes", nodesBuffer);
        }

        if (lines.Count > 0)
        {
            if (linesBuffer != null)
                linesBuffer.Release();
            linesBuffer = new ComputeBuffer(lines.Count, sizeof(float) * 6);
            linesBuffer.SetData(lines);
            Compute.SetBuffer(kernel, "Lines", linesBuffer);
        }

        //Apply compute
        ShareCameraParameters();
        Vector3Int threadSize = new Vector3Int(Mathf.CeilToInt(Screen.width / 8.0f), Mathf.CeilToInt(Screen.height / 8.0f), 1);
        Compute.Dispatch(kernel, threadSize.x, threadSize.y, threadSize.z);

        if (nodes.Count > 0)
            nodesBuffer.Release();

        if (lines.Count > 0)
            linesBuffer.Release();

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
