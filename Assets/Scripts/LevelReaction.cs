using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelReader))]
public class LevelReaction : MonoBehaviour
{

    public ComputeShader NodeFlocking;

    private LevelReader reader;
    private LevelMesh level;
    private Vector2Int counts;

    private ComputeBuffer buffer;
    private int kernel;

    public float Transmission = 0.1f;
    public float Velocity = 0.99f;

    public bool AddForce;
    public float maxDist;
    public int ReactNode;
    public Vector2 ReactForce;
    public static LevelReaction main;

    void Awake()
    {
        reader = GetComponent<LevelReader>();
        main = this;
    }

    public static void AddForceToNode(Point node, Vector2 force)
    {
        main.AddForce = true;
        main.ReactNode = main.level.adress[node];
        main.ReactForce = force;
    }

    void Update()
    {
        if (reader == null || reader.Level == null)
            return;

        bool needReload = reader.Level != level || reader.Level.dirty || reader.Level.Nodes.Length != counts.x || reader.Level.Links.Length != counts.y;
        if (needReload)
        {
            ReloadLevel();
            return;
        }

        if (Player._Instance == null || Player._Instance._Immobile)
            return;

        NodeFlocking.SetFloat("Transmission", Transmission);
        NodeFlocking.SetFloat("Velocity", Velocity);
        NodeFlocking.SetFloat("maxDist", maxDist);

        if (AddForce)
        {
            AddForce = false;
            NodeFlocking.SetInt("ReactNodeId", ReactNode);
            NodeFlocking.SetVector("ReactForce", ReactForce);
        }
        else
        {
            NodeFlocking.SetInt("ReactNodeId", -1);
        }

        NodeFlocking.Dispatch(kernel, Mathf.CeilToInt(level.Nodes.Length/ 8.0f), 1, 1);
        ApplyReactions();
    }

    void ApplyReactions()
    {
        NodeNode[] result = new NodeNode[level.Nodes.Length];
        buffer.GetData(result);
        for (int i = 0; i < result.Length; i++)
            level.Nodes[i].offset = result[i].offset;
    }

    void ReloadLevel()
    {
        level = reader.Level;
        counts.x = reader.Level.Nodes.Length;
        counts.y = reader.Level.Links.Length;
        level.dirty = false;

        if (buffer != null)
            buffer.Release();
        kernel = NodeFlocking.FindKernel("CSMain");
        buffer = new ComputeBuffer(level.Nodes.Length, sizeof(float) * 2 + sizeof(int) * 5);
        buffer.SetData(BufferFromMesh());
        NodeFlocking.SetBuffer(kernel, "Nodes", buffer);
    }

    void OnDestroy()
    {
        buffer.Release();
    }

    NodeNode[] BufferFromMesh()
    {
        NodeNode[] array = new NodeNode[level.Nodes.Length];
        int[] neighFill = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array[i].neigh0 = -1;
            array[i].neigh1 = -1;
            array[i].neigh2 = -1;
            array[i].neigh3 = -1;
            array[i].neigh4 = -1;
        }
        foreach (var link in level.Links)
        {
            switch (neighFill[link.id0])
            {
                case 0: array[link.id0].neigh0 = link.id1; break;
                case 1: array[link.id0].neigh1 = link.id1; break;
                case 2: array[link.id0].neigh2 = link.id1; break;
                case 3: array[link.id0].neigh3 = link.id1; break;
                case 4: array[link.id0].neigh4 = link.id1; break;
            }
            switch (neighFill[link.id1])
            {
                case 0: array[link.id1].neigh0 = link.id0; break;
                case 1: array[link.id1].neigh1 = link.id0; break;
                case 2: array[link.id1].neigh2 = link.id0; break;
                case 3: array[link.id1].neigh3 = link.id0; break;
                case 4: array[link.id1].neigh4 = link.id0; break;
            }
            neighFill[link.id0]++;
            neighFill[link.id1]++;
        }
        return array;
    }

    struct NodeNode
    {
        public Vector2 offset;
        public int neigh0;
        public int neigh1;
        public int neigh2;
        public int neigh3;
        public int neigh4;
    }

}
