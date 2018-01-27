using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode, ImageEffectAllowedInSceneView, RequireComponent(typeof(Camera))]
public class TerrainToField : MonoBehaviour
{

    public ComputeShader Compute;
    public Vector4 Thickness;
    public Vector4 NormalRemap;
    public LevelGenerator Generator;
    public float DrawRange;
    public Material TerrainBlitter;
    private RenderTexture temp = null;
    private RenderTexture tempB = null;
    private ComputeBuffer nodesBuffer;
    private ComputeBuffer linesBuffer;
    private int kernelT2F, kernelF2R;

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

        kernelT2F = Compute.FindKernel("TerrainToField");
        kernelF2R = Compute.FindKernel("FieldToRender");
    }

    void OnDestroy()
    {
        if (temp != null)
        {
            temp.Release();
            temp = null;
        }

        if (tempB != null)
        {
            tempB.Release();
            tempB = null;
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
        if (null == Compute || kernelT2F < 0 || null == src || Generator == null)
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

        if (null == tempB || src.width != tempB.width
            || src.height != tempB.height)
        {
            if (null != tempB)
                tempB.Release();
            tempB = new RenderTexture(src.width, src.height, src.depth);
            tempB.enableRandomWrite = true;
            tempB.Create();
        }

        //Init compute
        Compute.SetTexture(kernelT2F, "src", src);
        Compute.SetTexture(kernelT2F, "dst", temp);
        Compute.SetVector("_Utils", Thickness);
        Compute.SetVector("_NormalRemap", NormalRemap);
        

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
            Compute.SetBuffer(kernelT2F, "Nodes", nodesBuffer);
        }

        if (lines.Count > 0)
        {
            if (linesBuffer != null)
                linesBuffer.Release();
            linesBuffer = new ComputeBuffer(lines.Count, sizeof(float) * 6);
            linesBuffer.SetData(lines);
            Compute.SetBuffer(kernelT2F, "Lines", linesBuffer);
        }

        //Apply compute
        ShareCameraParameters();
        Vector3Int threadSize = new Vector3Int(Mathf.CeilToInt(Screen.width / 32.0f), Mathf.CeilToInt(Screen.height / 32.0f), 1);
        Compute.Dispatch(kernelT2F, threadSize.x, threadSize.y, threadSize.z);

        if (nodes.Count > 0)
            nodesBuffer.Release();

        if (lines.Count > 0)
            linesBuffer.Release();

        //FIELD TO RENDER________________________________
        Compute.SetTexture(kernelF2R, "source", temp);
        Compute.SetTexture(kernelF2R, "buffer", tempB);
        Compute.Dispatch(kernelF2R, threadSize.x, threadSize.y, threadSize.z);

        //Apply result to dst
        if (TerrainBlitter != null)
        {
            TerrainBlitter.SetTexture("_Color_Distance", temp);
            TerrainBlitter.SetTexture("_Normal_Alpha", tempB);
			Graphics.Blit(src, dst, TerrainBlitter);
			Debug.Log("Blit");
        }
        else
            Graphics.Blit(tempB, dst);
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
