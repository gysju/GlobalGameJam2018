using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Link:MonoBehaviour  {

    public Point _PointA;
    public Point _PointB;

    Link(Point a, Point b) {
        _PointA = a;
        _PointB = b;
    }

    private void Start()
    {
        _PointA._Links.Add(this);
        _PointB._Links.Add(this);
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (!_PointA || !_PointB )
            return;

        Gizmos.DrawLine(_PointA.transform.position, _PointB.transform.position);
    }

    public Point getOtherPoint(Point p) {

        if (_PointA != p)
        {
            return _PointA;
        }
        else {
            return _PointB;
        }

    }


}
