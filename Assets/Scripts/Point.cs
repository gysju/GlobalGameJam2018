using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Point: MonoBehaviour{

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
        GeneratedType();
    }

    private void Update()
    {
        if (transform.position.x < LevelGenerator._Instance._DeathZone.transform.position.x)
            _Type = PointType.Dead;
    }

    public void GeneratedType()
    {
        PointType type;
        float val = Random.Range(0.0f, 1.0f);

        if (val <= LevelGenerator._Instance._NormalPointsSpawnPercentage)
            type = PointType.Normal;
        else if (val <= LevelGenerator._Instance._NormalPointsSpawnPercentage 
                      + LevelGenerator._Instance._BackPointsSpawnPercentage)
            type = PointType.Fried;
        else if (val <= LevelGenerator._Instance._NormalPointsSpawnPercentage 
                      + LevelGenerator._Instance._BackPointsSpawnPercentage
                      + LevelGenerator._Instance._FriedPointsSpawnPercentage)
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
}
