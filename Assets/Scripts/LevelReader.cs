using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode, RequireComponent(typeof(MeshRenderer))]
public class LevelReader : MonoBehaviour
{

    //Public
    public LevelMesh Level;
    public ComputeShader LevelToField;
    public int TextureSize;
    public static LevelReader Main;
    public static LevelMesh CurrentLevel;

    //Compute parameters
    public float BlendingFactor;
    public float LinkSize;
    public float CutThreshold;

    //Private
    private int worldToFieldKernel;
    private RenderTexture bufferA;
    private RenderTexture bufferB;
    private ComputeBuffer nodesBuffer;
    private ComputeBuffer linksBuffer;
    private MeshRenderer rend;
    private Player player;


    void OnDrawGizmos()
    {
        Matrix4x4 l2World = transform.localToWorldMatrix;
        Vector3[] corners = GetCorners();

        Gizmos.color = new Color(0.7f, 1f, 0.1f);
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);
    }

    Vector3[] GetCorners()
    {
        Matrix4x4 l2World = transform.localToWorldMatrix;
        return new Vector3[] {
            l2World.MultiplyPoint(new Vector3(-.5f, -.5f, 0)),
            l2World.MultiplyPoint(new Vector3(.5f, -.5f, 0)),
            l2World.MultiplyPoint(new Vector3(.5f, .5f, 0)),
            l2World.MultiplyPoint(new Vector3(-.5f, .5f, 0)),
        };
    }

    RenderTexture RebuildBuffer(RenderTexture buffer)
    {
        if (buffer == null || buffer.width != TextureSize || buffer.height != TextureSize)
        {
            if (buffer != null)
                buffer.Release();
            RenderTexture temp = new RenderTexture(TextureSize, TextureSize, 0);
            temp.enableRandomWrite = true;
            temp.Create();
            return temp;
        }
        return buffer;
    }

    bool CircleInRect(Vector2 center, float radius, Rect rect)
    {
        return !(center.x + radius < rect.xMin || center.x - radius > rect.xMax || center.y + radius < rect.yMin || center.y - radius > rect.yMax);
    }

    void Awake()
    {
        Main = this;
        CurrentLevel = Level;
    }

    void Update()
    {
        CurrentLevel = Level;


        //Abort if needed
        if (Level == null || LevelToField == null)
            return;

        //Rebuild renderTextures
        bufferA = RebuildBuffer(bufferA);
        bufferB = RebuildBuffer(bufferB);

        //Share mesh info
        rend = GetComponent<MeshRenderer>();
        Vector3[] corners = GetCorners();
        LevelToField.SetVector("CornerLD", corners[0]);
        LevelToField.SetVector("CornerRD", corners[1]);
        LevelToField.SetVector("CornerRU", corners[2]);
        LevelToField.SetVector("CornerLU", corners[3]);

        //Share player info
        if (player == null)
            player = FindObjectOfType<Player>();
        else
        {
            Vector4 playerPos = (Vector4)player.transform.position;
            playerPos.w = player._Range;
            LevelToField.SetVector("PlayerPosition", playerPos);
            LevelToField.SetVector("PlayerColor", player._Color);
        }

        //Compute level informations
        Rect rect = new Rect(transform.position, transform.localScale);
        Dictionary<int, LevelMesh.Node> nodes = new Dictionary<int, LevelMesh.Node>();
        for (int i = 0; i < Level.Nodes.Length; i++)
        {
            LevelMesh.Node node = Level.Nodes[i];
            //if (CircleInRect(node.center, node.radius, rect))
                nodes.Add(i, node);
        }
        List<Vector4> links = new List<Vector4>();
        for (int i = 0; i < Level.Links.Length; i++)
        {
            Vector2Int index = Level.Links[i];
            if (nodes.ContainsKey(index.x) || nodes.ContainsKey(index.y))
            {
                Vector2 p0 = Level.Nodes[index.x].center; 
                Vector2 p1 = Level.Nodes[index.y].center;
                links.Add(new Vector4(p0.x, p0.y, p1.x, p1.y));
            }
        }

        //Rebuild buffers
        if (nodes.Count > 0)
        {
            if (nodesBuffer != null)
                nodesBuffer.Release();
            nodesBuffer = new ComputeBuffer(nodes.Count, sizeof(float) * 7);
            nodesBuffer.SetData(nodes.Values.ToList());
            LevelToField.SetBuffer(worldToFieldKernel, "Nodes", nodesBuffer);
        }
        if (links.Count > 0)
        {
            if (linksBuffer != null)
                linksBuffer.Release();
            linksBuffer = new ComputeBuffer(links.Count, sizeof(float) * 4);
            linksBuffer.SetData(links);
            LevelToField.SetBuffer(worldToFieldKernel, "Links", linksBuffer);
        }


        //Share parameters to compute
        LevelToField.SetFloat("BlendingFactor", BlendingFactor);
        LevelToField.SetFloat("LinkSize", LinkSize);
        LevelToField.SetFloat("CutThreshold", CutThreshold);

        //Execute compute
        LevelToField.SetTexture(worldToFieldKernel, "Result", bufferA);
        Vector3Int threadSize = new Vector3Int(Mathf.CeilToInt(bufferA.width / 32.0f), Mathf.CeilToInt(bufferA.height / 32.0f), 1);
        LevelToField.Dispatch(worldToFieldKernel, threadSize.x, threadSize.y, threadSize.z);

        //Release buffers
        if (nodes.Count > 0)
            nodesBuffer.Release();
        if (links.Count > 0)
            linksBuffer.Release();

        //Apply texture to material
        rend.sharedMaterial.SetTexture("_Color_Distance", bufferA);

    }

    void OnDestroy()
    {
        if (bufferA != null)
        {
            bufferA.Release();
            bufferA = null;
        }

        if (bufferB != null)
        {
            bufferB.Release();
            bufferB = null;
        }

        if (nodesBuffer != null)
        {
            nodesBuffer.Release();
            nodesBuffer = null;
        }
        if (linksBuffer != null)
        {
            linksBuffer.Release();
            linksBuffer = null;
        }
    }
}
