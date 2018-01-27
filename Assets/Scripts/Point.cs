using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Point: MonoBehaviour{

    private LevelGenerator levelGenerator;

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

        //GeneratedType();
    }

    private void Update()
    {
        if (_Type == PointType.Dead)
            return;

        if (transform.position.x < levelGenerator._DeathZone.transform.position.x)
            _Type = PointType.Dead;
    }

    public void GeneratedType()
    {

        PointType type;
        float val = Random.Range(0.0f, 1.0f);

        if (val <= levelGenerator._NormalPointsSpawnPercentage || GetClearPathCount() <= 2)
            type = PointType.Normal;
        else if (val <= levelGenerator._NormalPointsSpawnPercentage 
                      + levelGenerator._BackPointsSpawnPercentage)
            type = PointType.Fried;
        else if (val <= levelGenerator._NormalPointsSpawnPercentage 
                      + levelGenerator._BackPointsSpawnPercentage
                      + levelGenerator._FriedPointsSpawnPercentage)
            type = PointType.Back;
        else
            type = PointType.Dead;

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
                return Color.black;
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
            if (l.getOtherPoint(this)._Type == PointType.Normal) {
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
            if (Vector3.Dot((l.getOtherPoint(this).transform.position - transform.position).normalized, Vector3.right) < 0)
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
            return backwardPoint[Random.Range(0, backwardPoint.Count)];
        }
        else if (other.Count > 0)
        {
            return other[Random.Range(0, other.Count)];
        }
        else
        {
            return null;
        }

    }
}
