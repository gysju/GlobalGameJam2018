using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Point: MonoBehaviour{

    public enum PointType {
        Normal,
        Dead,
        Fried,
        Back


    }

    public PointType _Type = PointType.Normal;
    public PointType _InitialType = PointType.Normal;

    public List<Link> _Links = new List<Link>();

    public Point(Vector3 pos) {
        transform.position = pos;
    }

    private void Awake()
    {
        _Links = new List<Link>();
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
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, 0.1f);
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
