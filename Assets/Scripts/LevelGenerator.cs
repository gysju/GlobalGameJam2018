using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    // Use this for initialization


    public static LevelGenerator _Instance = null;

    [Header("Game Construction info")]
    [Range(1, 100)]
    public float _Length = 5;

    [Range(1, 100)]
    public int _SegmentPointCount = 5;
    public int _Segments = 5;
    public float _Height = 5;
    public float _MinLinkLenght = 0.2f;
    public float _MaxLinkLenght = 2;
    public float _Speed = 0.001f;
    public int _CounterSignalNmb = 0;
    public List<Point> _Points = new List<Point>();
    public List<Link> _Links = new List<Link>();

    [Header("Points type info")]
    [Range(0.0f, 0.25f)]
    public float _KillPointsSpawnPercentage = 0.1f;
    [Range(0.0f, 0.25f)]
    public float _FriedPointsSpawnPercentage = 0.1f;
    [Range(0.0f, 0.25f)]
    public float _BackPointsSpawnPercentage = 0.1f;

    [Range(0.0f, 1.0f)]
    public float _NormalPointsSpawnPercentage;

    [Header("DeathZone")]
    public GameObject _DeathZone;
    public float _DeathSpeed = 1.0f;
    public float _DeathSpawnBias = 1.0f;

    public GameObject _Player;
    public Coroutine _deathZoneCoroutine;

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

    void Start()
    {
        _NormalPointsSpawnPercentage = 1.0f - _KillPointsSpawnPercentage - _FriedPointsSpawnPercentage - _BackPointsSpawnPercentage;
        CameraMovement._Instance.transform.position = new Vector3(_Length, _Height/2, CameraMovement._Instance.transform.position.z);   

        //_deathZoneCoroutine = StartCoroutine(DeathZone());
    }

    public IEnumerator GenerateLevel(int i = 1)
    {
        i = 2;
        _Length = 5 * i;
        _Segments = 5 * i;
        _SegmentPointCount = 5 * i;
        _CounterSignalNmb = i;

        yield return StartCoroutine(SpawnPoints());
        yield return StartCoroutine(BuildPath());
        CheckPathType();
    }

    public void GeneratePlayer()
    {
        if (!_Player)
        {
            Instantiate(_Player);
            Player._Instance._Start = _Points[0];
            Player._Instance.transform.position = _Points[0].transform.position;
            Player._Instance._Target = _Points[1];
            Player._Instance._Immobile = false;
        }
        else
        {
            Player._Instance._Immobile = false;
        }

        if (Camera.main)
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, _Height / 2, Camera.main.transform.position.z);
        }

        _deathZoneCoroutine = StartCoroutine(DeathZone());
    }

    IEnumerator SpawnPoints()
    {

        GameObject go = new GameObject("Point_Start", typeof(Point));
        go.transform.position = new Vector3(-2, _Height / 2, 0);
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

            for (int i = 1; i < _SegmentPointCount; i++)
            {
                go = new GameObject("Point_" + j + "-" + i, typeof(Point));
                go.transform.position = new Vector3(Random.Range(start + i * segmentLength / _SegmentPointCount, start + (i + 1) * segmentLength / _SegmentPointCount), Random.Range(0, _Height), 0);
                go.transform.SetParent(transform);
                _Points.Add(go.GetComponent<Point>());
            }
            yield return null;
        }

        go = new GameObject("Point_toEnd", typeof(Point));
        go.transform.position = new Vector3(_Length, _Height / 2, 0);
        go.transform.parent = transform;
        _Points.Add(go.GetComponent<Point>());

        go = new GameObject("Point_End", typeof(Point));
        go.transform.position = new Vector3(_Length+2, _Height / 2, 0);
        go.transform.parent = transform;
        _Points.Add(go.GetComponent<Point>());

        List<Point> pointsToDelete = new List<Point>();

        foreach (Point p in _Points)
        {
            foreach (Point p2 in _Points)
            {
                if (p != p2)
                    if ((p.transform.position - p2.transform.position).magnitude < _MinLinkLenght && (!pointsToDelete.Contains(p2) && !pointsToDelete.Contains(p)))
                    {
                        pointsToDelete.Add(p2);
                    }
            }
            yield return null;
        }

        for (int i = pointsToDelete.Count; i > 0; i--)
        {
            _Points.Remove(pointsToDelete[i - 1]);
            Destroy(pointsToDelete[i - 1].gameObject);
        }
        pointsToDelete.Clear();

    }
    
    IEnumerator BuildPath()
    {

        foreach (Point p in _Points)
        {
            foreach (Point p2 in _Points)
            {
                if (p == p2)
                    continue;
                Vector3 pToP2 = p2.transform.position - p.transform.position;
                if (pToP2.magnitude < _MaxLinkLenght)
                {
                    if (Vector3.Dot(Vector3.right, pToP2.normalized) > -0.2f)
                    {
                        if (!IsTooCloseFromOtherLine(p, pToP2))
                            if (!DoesIntersectLinks(p.transform.position, p2.transform.position))
                            {
                                GameObject go = new GameObject("link", typeof(Link));
                                _Links.Add(go.GetComponent<Link>().buildLink(p, p2));
                                go.transform.SetParent(transform);
                            }
                    }
                }
            }
            yield return null;
        }

        foreach (Point p in _Points)
        {
            p.GeneratedType();
        }
    }

    void CheckPathType()
    {
        _Points[0]._Links[0]._PointB.SetInitialType(Point.PointType.Normal);
        _Points[_Points.Count - 1]._Links[0]._PointA.SetInitialType(Point.PointType.Normal);
    }

    IEnumerator DeathZone()
    {
        _DeathZone.transform.position = Player._Instance.transform.position - (Vector3.right * _DeathSpawnBias);

        while (!Player._Instance._Immobile)
        {
            _DeathZone.transform.position += Vector3.right * (Time.deltaTime * _DeathSpeed);
            yield return null;
        }
    }

    public bool DoesIntersectLinks(Vector3 p1, Vector3 p2)
    {

        foreach (Link l2 in _Links)
        {

            if (l2.Intersect(p1, p2))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsTooCloseFromOtherLine(Point p, Vector3 dir)
    {

        foreach (Link l in p._Links)
        {

            if (Vector3.Dot((l.getOtherPoint(p).transform.position - p.transform.position).normalized, dir.normalized) > 0.80)
                return true;

        }

        return false;
    }

    public void ResetPlayer()
    {  
        if (Player._Instance)
        {
            Player._Instance._Start = _Points[0];
            Player._Instance._Target = _Points[1];
            Player._Instance.transform.position = _Points[0].transform.position;
            CameraMovement._Instance.Snap();
        }
        _DeathZone.transform.position = Vector3.right*-10;
        
        foreach (Point p in _Points)
            p.Reset();
    }

    private void OnDrawGizmos()
    {
        // draw death zone
        Gizmos.color = Color.red;
        if(_DeathZone != null)
            Gizmos.DrawLine(_DeathZone.transform.position + Vector3.up * 100, _DeathZone.transform.position - Vector3.up * 100);
    }
}
