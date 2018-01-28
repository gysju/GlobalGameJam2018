using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    // Use this for initialization
    public static LevelGenerator _Instance = null;
    public int _Difficulty = 0;
    public List<DifficultyLevel> DifficultyLevels = new List<DifficultyLevel>();
    public DifficultyLevel ForceALevel;

    public DifficultyLevel CurrentLevel
    {
        get { if (ForceALevel)
                return ForceALevel;
            else
                return DifficultyLevels[_Difficulty];
        }
    }
    public float _Length
    {get { return CurrentLevel.Lenght; }}

    public int _SegmentPointCount
    {get { return CurrentLevel.SegmentPointCount; }}
    public int _Segments
    {get { return CurrentLevel.Segments; }}
    public float _Height
    { get { return CurrentLevel.Height; } }
    public float _MinLinkLenght
    {get { return CurrentLevel.MinLinkLenght; }}
    public float _MaxLinkLenght
    { get { return CurrentLevel.MaxLinkLenght; } }
    public float _Speed = 0.001f;
    public int _CounterSignalNmb = 0;

    public float _KillPointsSpawnPercentage
    { get { return CurrentLevel.KillPoint; } }
    public float _FriedPointsSpawnPercentage
    { get { return CurrentLevel.FriedPoint; } }
    public float _BackPointsSpawnPercentage
    { get { return CurrentLevel.BackPoint; } }
    public float _NormalPointsSpawnPercentage;


    [HideInInspector] public List<Point> _Points = new List<Point>();
    [HideInInspector] public List<Link> _Links = new List<Link>();
    [HideInInspector] public List<Enemy> _Enemies = new List<Enemy>();

    [Header("DeathZone")]
    [HideInInspector]
    public GameObject _DeathZone;
    public float _DeathSpeed
    { get { return CurrentLevel.DeathWaveSpeed; } }
    public float _DeathSpawnBias = 50.0f;
    private Vector3 _DefaultPos;
    public GameObject _Player;
    public GameObject _CounterSignal; 
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
        _DeathZone = new GameObject("DeathZone");
        _NormalPointsSpawnPercentage = 1.0f - _KillPointsSpawnPercentage - _FriedPointsSpawnPercentage - _BackPointsSpawnPercentage;
        CameraMovement._Instance.transform.position = new Vector3(_Length, _Height/2, CameraMovement._Instance.transform.position.z);
    }

    public IEnumerator GenerateLevel()
    {
        _DefaultPos  = -(Vector3.right * 1000.0f);
        _DeathZone.transform.position = _DefaultPos;

        _CounterSignalNmb = _Difficulty;

        yield return StartCoroutine(SpawnPoints());
        yield return StartCoroutine(BuildPath());
        CheckPathType();

        _DefaultPos = _Points[0].transform.position - Vector3.right * _DeathSpawnBias;

        LevelReader.CurrentLevel.Redefine(_Links);
        Debug.Log("Level generated");
    }

    public void CleanLevels()
    {
        foreach (Point p in _Points)
        {
            Destroy(p.gameObject);
        }
        _Points.Clear();

        foreach (Link l in _Links)
        {
            Destroy(l.gameObject);
        }
        _Links.Clear();

        foreach (Enemy e in _Enemies)
        {
            Destroy(e.gameObject);
        }
        _Enemies.Clear();

        if(Player._Instance != null)
            Destroy(Player._Instance.gameObject);
    }

    public void GeneratePlayer()
    {
        if (_Player)
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

        if(_CounterSignal != null) 
            for (int i = 0; i < _CounterSignalNmb; i++) {
                    _Enemies.Add(Instantiate(_CounterSignal).GetComponent<Enemy>()); 
            } 

        _deathZoneCoroutine = StartCoroutine(DeathZone());
    }

    IEnumerator SpawnPoints()
    {

        GameObject go = new GameObject("Point_Start", typeof(Point));
        go.transform.position = new Vector3(-_MaxLinkLenght * 0.9f, _Height / 2, 0);
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
            //yield return null;
        }

        for (int i = pointsToDelete.Count; i > 0; i--)
        {
            _Points.Remove(pointsToDelete[i - 1]);
            Destroy(pointsToDelete[i - 1].gameObject);
        }
        pointsToDelete.Clear();

        go = new GameObject("Point_toEnd", typeof(Point));
        go.transform.position = new Vector3(_Length, _Height / 2, 0);
        go.transform.parent = transform;
        _Points.Add(go.GetComponent<Point>());

        go = new GameObject("Point_End", typeof(Point));
        go.transform.position = new Vector3(_Length + _MaxLinkLenght * 0.9f, _Height / 2, 0);
        go.transform.parent = transform;
        _Points.Add(go.GetComponent<Point>());
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
        _Points[1].SetInitialType(Point.PointType.Normal);
        _Points[0].SetInitialType(Point.PointType.Normal);

        _Points[_Points.Count - 1].SetInitialType(Point.PointType.Normal);
        _Points[_Points.Count - 2].SetInitialType(Point.PointType.Normal);
    }

    IEnumerator DeathZone()
    {
        _DeathZone.transform.position = _DefaultPos;

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
        _DeathZone.transform.position = _DefaultPos;

        foreach (Point p in _Points)
            p.Reset();

        foreach (Enemy e in _Enemies)
            if (e != null)
                Destroy(e.gameObject);

        //LevelReader.CurrentLevel.Redefine(_Links);

        _Enemies.Clear();
    }

    private void OnDrawGizmos()
    {
        // draw death zone
        Gizmos.color = Color.red;
        if(_DeathZone != null)
            Gizmos.DrawLine(_DeathZone.transform.position + Vector3.up * 100, _DeathZone.transform.position - Vector3.up * 100);
    }
}
