using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {

    // Use this for initialization

    [Range( 1 , 100 )]
    public float _Length = 5;

    [Range(1, 100)]
    public int _SegmentPointCount = 5;
    public int _Segments = 5;
    public float _Height = 5;
    public float _MaxLinkLenght = 2;

    List<Point> _Points = new List<Point>();
    List<Link> _Links = new List<Link>();

    IEnumerator Start()
    {
        yield return StartCoroutine(SpawnPoints());
        yield return StartCoroutine(BuildPath());
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    IEnumerator SpawnPoints() {

        GameObject go = new GameObject("Point_" + 0 + "-" + 0, typeof(Point));
        go.transform.position = new Vector3(0, _Height / 2, 0);
        go.transform.parent = transform;
        _Points.Add(go.GetComponent<Point>());
        for (int j = 0; j < _Segments; j++)
        {
            float segmentLength = _Length / _Segments;
            float start = j * segmentLength;
            float end = start + segmentLength;

            for (int i = 1; i < _SegmentPointCount; i++)
            {
                go = new GameObject("Point_" + j + "-" + i, typeof(Point));
                go.transform.position = new Vector3(Random.Range(start+i*segmentLength/_SegmentPointCount, start+(i+1)*segmentLength/_SegmentPointCount), Random.Range(0, _Height), 0);
                go.transform.parent = transform;
                _Points.Add(go.GetComponent<Point>());
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator BuildPath() {

        foreach( Point  p in _Points){
            foreach (Point p2 in _Points) {
                Vector3 pToP2 = p2.transform.position - p.transform.position;
                if (pToP2.magnitude < _MaxLinkLenght) {
                    if (Vector3.Dot(Vector3.right, pToP2.normalized) > 0.2f ) {
                        if (!DoesIntersectLinks(p.transform.position, p2.transform.position))
                        {
                            GameObject go = new GameObject("link", typeof(Link));
                            _Links.Add(go.GetComponent<Link>().buildLink(p, p2));
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
            }
        }


        
    }

    public bool DoesIntersectLinks(Vector3 p1, Vector3 p2) {

        foreach (Link l2 in _Links) {

            if (l2.Intersect(p1, p2)) {
                return true;
            }
        }
        return false;
    }

}
