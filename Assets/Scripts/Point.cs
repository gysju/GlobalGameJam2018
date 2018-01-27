using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Point: MonoBehaviour{

    public List<Link> _Links = new List<Link>();

    public Point(Vector3 pos) {
        transform.position = pos;
    }


    public Point getMostAccurateDestinaton(Vector3 targetDirection)
    {
        Point bestPoint = null;
        float currentDot = -2;
        float bestPointDot = -2;
        foreach (Link l in _Links)
        {
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
}
