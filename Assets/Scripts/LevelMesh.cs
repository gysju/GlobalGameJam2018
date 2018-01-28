using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelMesh", menuName = "LevelMesh", order = 52)]
public class LevelMesh : ScriptableObject
{

    public Node[] Nodes;
    public LinkF[] Links;
    public Dictionary<Point, int> adress;

    [System.Serializable]
    public struct LinkF
    {
        public int id0;
        public int id1;
        public float focus;
    }

    [System.Serializable]
    public struct Node
    {
        public Vector2 center;
        public float radius;
        public Color color;
    }

    public void Redefine(List<Link> links)
    {
        List<Node> nodes = new List<Node>();
        List<LinkF> ls = new List<LinkF>();
        adress = new Dictionary<Point, int>();
        for (int i = 0; i < links.Count; i++)
        {
            Vector2Int link = Vector2Int.zero;
            if (adress.ContainsKey(links[i]._PointA))
                link.x = adress[links[i]._PointA];
            else
            { 
                Vector3 position = links[i]._PointA.transform.position;
                Color color = links[i]._PointA.GetNodeColor();
                float radius = 2;
                nodes.Add(new Node() { center = position, color = color, radius = radius });
                link.x = nodes.Count - 1;
                adress.Add(links[i]._PointA, nodes.Count - 1);
            }

            if (adress.ContainsKey(links[i]._PointB))
                link.y = adress[links[i]._PointB];
            else
            {
                Vector3 position = links[i]._PointB.transform.position;
                Color color = links[i]._PointB.GetNodeColor();
                float radius = 2;
                nodes.Add(new Node() { center = position, color = color, radius = radius });
                link.y = nodes.Count - 1;
                adress.Add(links[i]._PointB, nodes.Count - 1);
            }
            ls.Add(new LinkF() { id0 = link.x, id1 = link.y, focus = links[i]._CurrentDot});
        }

        Nodes = nodes.ToArray();
        Links = ls.ToArray();
    }

}
