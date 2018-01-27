using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Link:MonoBehaviour  {

    public Point _PointA;
    public Point _PointB;

    public Link buildLink(Point a, Point b) {
        _PointA = a;
        _PointB = b;

        if (_PointA._Links == null) _PointA._Links = new List<Link>();
        if (_PointB._Links == null) _PointB._Links = new List<Link>();
        _PointA._Links.Add(this);
        _PointB._Links.Add(this);
        return this;
    }

    private void Start()
    {

        
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
