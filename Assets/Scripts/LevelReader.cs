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
    public Vector4 NormalRemap;
    public Texture2D Noise;
    public float NoiseFactor;
    public float NoiseScale;
    public Vector2 NoiseDirection;

    //Private
    private int worldToFieldKernel;
    private int FieldToNormalKernel;
    private RenderTexture bufferA;
    private RenderTexture bufferB;
    private ComputeBuffer nodesBuffer;
    private ComputeBuffer linksBuffer;
    private MeshRenderer rend;
    private Player player;
    private System.Random rnd = new System.Random();


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
        const float range = .5f;
        Matrix4x4 l2World = transform.localToWorldMatrix;
        return new Vector3[] {
            l2World.MultiplyPoint(new Vector3(-range, -range, 0)),
            l2World.MultiplyPoint(new Vector3(range, -range, 0)),
            l2World.MultiplyPoint(new Vector3(range, range, 0)),
            l2World.MultiplyPoint(new Vector3(-range, range, 0)),
        };
    }

    RenderTexture RebuildBuffer(RenderTexture buffer)
    {
        if (buffer == null || buffer.width != TextureSize || buffer.height != TextureSize)
        {
            if (buffer != null)
                buffer.Release();
            RenderTexture temp = new RenderTexture(TextureSize, TextureSize, 0,RenderTextureFormat.ARGBHalf);
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

    void OnEnable()
    {
        if (Noise != null)
        {
            LevelToField.SetTexture(worldToFieldKernel, "Noise", Noise);
        }
    }

    void Update()
    {
        CurrentLevel = Level;

        //Abort if needed
        if (Level == null || LevelToField == null)
            return;

        //Get basics
        worldToFieldKernel = LevelToField.FindKernel("LevelToField");
        FieldToNormalKernel = LevelToField.FindKernel("FieldToNormal");

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
        LevelToField.SetFloat("Time", Time.time);
        LevelToField.SetVector("NoiseParameters", new Vector4(NoiseFactor, NoiseScale, NoiseDirection.x, NoiseDirection.y));

        //Share player info
        if (player == null)
            player = Player._Instance;
        else
        {
            Vector4 playerPos = (Vector4)player.transform.position;
            playerPos.w = player._Range;
            LevelToField.SetVector("PlayerPosition", playerPos);
            LevelToField.SetVector("PlayerColor", player._Color);
        }

        //Compute level informations
        Rect rect = new Rect(corners[0].x, corners[0].y, corners[1].x - corners[0].x, corners[2].y - corners[1].y);
        Dictionary<int, NodeC> nodes = new Dictionary<int, NodeC>();
        for (int i = 0; i < Level.Nodes.Length; i++)
        {
            LevelMesh.Node node = Level.Nodes[i];
            if (CircleInRect(node.center, node.radius + 5, rect))
                nodes.Add(i, new NodeC() { center = node.center + node.offset, color = node.color, radius = node.radius});
        }

        //Add ennemies
        if (LevelGenerator._Instance != null && LevelGenerator._Instance._Enemies != null)
        {
            for (int i = 0; i < LevelGenerator._Instance._Enemies.Count; i++)
            {
                Enemy enemy = LevelGenerator._Instance._Enemies[i];
                NodeC node = new NodeC() { center = enemy.transform.position, color = Color.red * 0.5f, radius = enemy._Range };
                nodes.Add(GetRandomKey(nodes), node);
            }
        }

        //Add links
        List<LinkC> links = new List<LinkC>();
        for (int i = 0; i < Level.Links.Length; i++)
        {
            Vector2Int index = new Vector2Int(Level.Links[i].id0, Level.Links[i].id1);
            if (nodes.ContainsKey(index.x) || nodes.ContainsKey(index.y))
            {
                Vector2 p0 = Level.Nodes[index.x].center + Level.Nodes[index.x].offset; 
                Vector2 p1 = Level.Nodes[index.y].center + Level.Nodes[index.y].offset;
                links.Add(new LinkC() { p0 = p0, p1 = p1, focus = Level.Links[i].focus});
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
            linksBuffer = new ComputeBuffer(links.Count, sizeof(float) * 5);
            linksBuffer.SetData(links);
            LevelToField.SetBuffer(worldToFieldKernel, "Links", linksBuffer);
        }


        //Share parameters to compute
        LevelToField.SetFloat("BlendingFactor", BlendingFactor);
        LevelToField.SetFloat("LinkSize", LinkSize);
        LevelToField.SetFloat("CutThreshold", CutThreshold);
        LevelToField.SetVector("NormalRemap", NormalRemap);
        

        //Execute compute
        LevelToField.SetTexture(worldToFieldKernel, "Result", bufferA);
        Vector3Int threadSize = new Vector3Int(Mathf.CeilToInt(bufferA.width / 32.0f), Mathf.CeilToInt(bufferA.height / 32.0f), 1);
        LevelToField.Dispatch(worldToFieldKernel, threadSize.x, threadSize.y, threadSize.z);
        //Normal pass
        LevelToField.SetTexture(FieldToNormalKernel, "SourceField", bufferA);
        LevelToField.SetTexture(FieldToNormalKernel, "Normal", bufferB);
        LevelToField.Dispatch(FieldToNormalKernel, threadSize.x, threadSize.y, threadSize.z);

        //Release buffers
        if (nodes.Count > 0)
            nodesBuffer.Release();
        if (links.Count > 0)
            linksBuffer.Release();

        //Apply texture to material
        rend.sharedMaterial.SetTexture("_Color_Distance", bufferA);
        rend.sharedMaterial.SetTexture("_Normal_Alpha", bufferB);
    }

    int GetRandomKey(Dictionary<int, NodeC> dico)
    {
        int key = rnd.Next();
        while (dico.ContainsKey(key))
            key = rnd.Next();
        return key;
    }

    struct LinkC
    {
        public Vector2 p0;
        public Vector2 p1;
        public float focus;
    }

    struct NodeC
    {
        public Vector2 center;
        public float radius;
        public Color color;
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
