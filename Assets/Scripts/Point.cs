using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Point: MonoBehaviour{

    private LevelGenerator levelGenerator;
    private LevelMesh _Level;

    public enum PointType {
        Normal,
        Dead,
        Fried,
        Back,
        Count
    }

    public PointType _Type = PointType.Normal;
    public PointType _InitialType = PointType.Normal;

    public List<Link> _Links = new List<Link>();

    public Point(Vector3 pos) {
        transform.position = pos;
    }

    public Vector3 Center
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private void Awake()
    {
        _Links = new List<Link>();
        levelGenerator = LevelGenerator._Instance;
        _Level = LevelReader.CurrentLevel;

        //GeneratedType();
    }

    private void Update()
    {
        if (_Type == PointType.Dead)
            return;

        if (transform.position.x < levelGenerator._DeathZone.transform.position.x)
        {
            _Type = PointType.Dead;
            _Level.Nodes[_Level.adress[this]].color = GetNodeColor();
        }
    }

    public void GeneratedType()
    {

        PointType type = PointType.Normal;
        float val = Random.Range(0.0f, 1.0f);

        if (val <= levelGenerator._NormalPointsSpawnPercentage)// || GetClearPathCount() <= 2)
            type = PointType.Normal;
        else if (val <= levelGenerator._NormalPointsSpawnPercentage 
                      + levelGenerator._FriedPointsSpawnPercentage)
            type = PointType.Fried;
        else if (val <= levelGenerator._NormalPointsSpawnPercentage 
                      + levelGenerator._BackPointsSpawnPercentage
                      + levelGenerator._FriedPointsSpawnPercentage)
            type = PointType.Back;
        else if( GetClearPathCount() > 2)
            type = PointType.Dead;

        //Debug.Log("Spawn: " + type.ToString() + " for value: " + val);
        SetInitialType(type);
    }

    public Point getMostAccurateDestinaton(Vector3 targetDirection)
    {
        Point bestPoint = null;
        float currentDot = -2;
        float bestPointDot = -2;
        foreach (Link l in _Links)
        {
            if (!l._Open)
                continue;
            Point potentialDest = l.getOtherPoint(this);
            currentDot = Vector3.Dot(targetDirection, (potentialDest.transform.position - transform.position).normalized);
            l._CurrentDot = currentDot;
            if (bestPointDot < currentDot)
            {
                bestPoint = potentialDest;
                bestPointDot = currentDot;
            }
        }
        return bestPoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GetNodeColor();
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

    public Color GetNodeColor()
    {
        return StateToColor(_Type);
    }

    private Color StateToColor(PointType type)
    {
        switch (_Type)
        {
            case PointType.Normal:
                return Color.cyan;
            case PointType.Dead:
                return Color.red;
            case PointType.Fried:
                return Color.blue;
            case PointType.Back:
                return new Color(0.7f, 0, 0.7f);
            default:
                return Color.white;
        }
    }

    public void SetInitialType(PointType type) {

        _InitialType = type;
        _Type = type;
    }

    public void Reset()
    {
        _Type = _InitialType;
        _Level.Nodes[_Level.adress[this]].color = GetNodeColor();
    }

    public void SetDefaultType()
    {
        _Type = PointType.Normal;
        _Level.Nodes[_Level.adress[this]].color = Color.cyan;
    }

    public Link GetConnectingLink(Point other) {

        foreach (Link l in _Links) {

            if (l.getOtherPoint(this) == other)
                return l;
        }
        return null;
    }

    public int GetClearPathCount() {
        int count = 0;
        foreach (Link l in _Links) {
            if (l.getOtherPoint(this)._Type == PointType.Normal){// && l.getOtherPoint(this)._Links.Count > 2) {
                ++count;
            }
        }
        return count;
    }

    public Point GetRandomForwardPath() {
        List<Point> forwardPoints = new List<Point>();
        List<Point> other = new List<Point>();
        foreach (Link l in _Links) {
            if (Vector3.Dot((l.getOtherPoint(this).transform.position - transform.position).normalized, Vector3.right) > 0)
            {
                forwardPoints.Add(l.getOtherPoint(this));
            }
            else {
                other.Add(l.getOtherPoint(this));
            }             
        }

        if (forwardPoints.Count > 0)
        {
            return forwardPoints[Random.Range(0, forwardPoints.Count)];
        }
        else if (other.Count > 0)
        {
            return other[Random.Range(0, other.Count)];
        }
        else {
            return null;
        }


    }

    public Point GetRandomBackwardPath()
    {
        List<Point> backwardPoint = new List<Point>();
        List<Point> other = new List<Point>();
        foreach (Link l in _Links)
        {
            if (Vector3.Dot((l.getOtherPoint(this).transform.position - transform.position ).normalized, Vector3.right) < 0)
            {
                backwardPoint.Add(l.getOtherPoint(this));
            }
            else
            {
                other.Add(l.getOtherPoint(this));
            }
        }

        if (backwardPoint.Count > 0)
        {
            Debug.Log("got backwardpoint");
            return backwardPoint[Random.Range(0, backwardPoint.Count)];
        }
        else if (other.Count > 0)
        {
            //Debug.Log("got random point");
            return other[Random.Range(0, other.Count)];
        }
        else
        {
            return null;
        }

    }

    public void ClearAllDots() {

        foreach (Link l in _Links)
            l._CurrentDot = 0;
    }
}
