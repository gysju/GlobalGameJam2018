﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Link:MonoBehaviour  {

    public Point _PointA;
    public Point _PointB;
    public float _CurrentDot = 0;
    public bool _Open = true;
    public Link _LinkedPassage;
    public Link buildLink(Point a, Point b) {
        _PointA = a;
        _PointB = b;
        
        _PointA._Links.Add(this);
        _PointB._Links.Add(this);
        return this;
    }
    

    private void OnDrawGizmos()
    {
        if (!_PointA || !_PointB )
            return;

        switch (_PointB._Type)
        {
            case Point.PointType.Normal:
                {
                    Gizmos.color = Color.cyan;
                    break;
                }
            case Point.PointType.Dead:
                {
                    Gizmos.color = Color.red;
                    break;
                }
            case Point.PointType.Fried:
                {
                    Gizmos.color = Color.blue;
                    break;
                }
            case Point.PointType.Back:
                {
                    Gizmos.color = Color.black;
                    break;
                }
        }
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

    public bool Intersect(Vector3 p1, Vector3 p2) {

            

        return LineSegmentsIntersection( _PointA.transform.position, _PointB.transform.position, p1, p2);
    }

    public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 p4)
    {
        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u <= 0.0f || u >= 1.0f || v <= 0.0f || v >= 1.0f)
        {
            return false;
        }

        return true;
    }

    public void OnCrossed() {
        if (_LinkedPassage)
            _LinkedPassage._Open = true;
    }

    public void SetLinkedClosedPath(Link l) {
        _LinkedPassage = l;
        l._Open = false;
    }
}
