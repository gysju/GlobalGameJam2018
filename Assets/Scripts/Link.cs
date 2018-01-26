using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Link:MonoBehaviour  {


    public List<Point> _Points = new List<Point>();

    Link(Point a, Point b) {
        _Points.Add(a);
        _Points.Add(b);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (_Points.Count <= 1)
            return;

        Gizmos.DrawLine(_Points[0].transform.position, _Points[1].transform.position);
    }
}
