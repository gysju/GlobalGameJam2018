using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour {

    // Use this for initialization


    public static LevelGenerator _Instance = null;
    private void Awake()
    {
        if (!_Instance)
        {
            _Instance = this;
        }
        else
        {
            Destroy(this);
        }

    }


    [Range( 1 , 100 )]
    public float _Length = 5;

    [Range(1, 100)]
    public int _SegmentPointCount = 5;
    public int _Segments = 5;
    public float _Height = 5;
    public float _MinLinkLenght = 0.2f;
    public float _MaxLinkLenght = 2;
    public float _Speed = 0.01f;

    public List<Point> _Points = new List<Point>();
    public List<Link> _Links = new List<Link>();

    public GameObject _Player;

    IEnumerator Start()
    {
        yield return StartCoroutine(SpawnPoints());
        yield return StartCoroutine(BuildPath());


        if (_Player) {
            Instantiate(_Player);
            Player._Instance._Start = _Points[0];
            Player._Instance._Target = _Points[1];
            Player._Instance._Immobile = false;
        }

        if (Camera.main) {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, _Height / 2, Camera.main.transform.position.z);
        }

    }
	

    IEnumerator SpawnPoints() {

        GameObject go = new GameObject("Point_Start", typeof(Point));
        go.transform.position = new Vector3(-1, _Height / 2, 0);
        go.transform.parent = transform;
        _Points.Add(go.GetComponent<Point>());

        go = new GameObject("Point_" + 0 + "-" + 0, typeof(Point));
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
                go.transform.SetParent(transform);
                _Points.Add(go.GetComponent<Point>());
                yield return new WaitForSeconds(_Speed);
            }
        }
        List<Point> pointsToDelete = new List<Point>();

        foreach (Point p in _Points) {
            foreach (Point p2 in _Points) {
                if (p != p2)
                    if ((p.transform.position - p2.transform.position).magnitude < _MinLinkLenght && (!pointsToDelete.Contains(p2) && !pointsToDelete.Contains(p))) {
                        pointsToDelete.Add(p2);
                    }
            }
        }

        for(int i = pointsToDelete.Count; i > 0; i-- )
        {
            _Points.Remove(pointsToDelete[i-1]);
            Destroy(pointsToDelete[i-1].gameObject);
            yield return new WaitForSeconds(_Speed);
        }
        pointsToDelete.Clear();

    }



    IEnumerator BuildPath() {

        foreach( Point  p in _Points){
            foreach (Point p2 in _Points) {
                if (p == p2)
                    continue;
                Vector3 pToP2 = p2.transform.position - p.transform.position;
                if (pToP2.magnitude < _MaxLinkLenght) {
                    if (Vector3.Dot(Vector3.right, pToP2.normalized) > -0.2f ) {
                        if( !IsTooCloseFromOtherLine( p, pToP2))
                            if (!DoesIntersectLinks(p.transform.position, p2.transform.position))
                            {
                                GameObject go = new GameObject("link", typeof(Link));
                                _Links.Add(go.GetComponent<Link>().buildLink(p, p2));
                                go.transform.SetParent(transform);
                                yield return new WaitForSeconds(_Speed);
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

    public bool IsTooCloseFromOtherLine(Point p, Vector3 dir) {

        foreach (Link l in p._Links) {

            if (Vector3.Dot((l.getOtherPoint(p).transform.position - p.transform.position).normalized, dir.normalized) > 0.80)
                return true;

        }

        return false;
    }

    public void StartResetRoutine() {
        StartCoroutine(KillPlayer());

    }
    public IEnumerator KillPlayer() {

        foreach (Point p in _Points) {
            p.Reset();
        }

        

        float fadeTime = 1;
        float t = 0;
        Color c = CameraMovement._Instance._FadePlane.color;
        while (t < fadeTime) { //Fade out
            t += Time.deltaTime;

            CameraMovement._Instance._FadePlane.color = new Color(c.r, c.g, c.b, t / fadeTime);

            yield return null;
        }

        if (Player._Instance)
        {
            Player._Instance._Start = _Points[0];
            Player._Instance._Target = _Points[1];
            Player._Instance.transform.position = _Points[0].transform.position;
            CameraMovement._Instance.Snap();
        }

        while (t > 0)
        { //Fade out
            t -= Time.deltaTime;
            CameraMovement._Instance._FadePlane.color = new Color(c.r, c.g, c.b, t / fadeTime);
            yield return null;
        }


        Player._Instance._Immobile = false;
    }


}
