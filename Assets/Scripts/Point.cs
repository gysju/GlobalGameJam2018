using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Point", menuName = "Level", order = 50)]
public class Point : ScriptableObject {

    public List<Link> _Links = new List<Link>();
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
