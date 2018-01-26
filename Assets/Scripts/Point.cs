using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Point: MonoBehaviour{

    public List<Link> _Links = new List<Link>();

    Point(Vector3 pos) {
        transform.position = pos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
