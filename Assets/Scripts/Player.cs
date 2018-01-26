using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Point _Start;
    public Point _Target;

    public float _Speed = 1f;

	// Use this for initialization
	void Start () {
        StartCoroutine(goToPoint());

    }


    void Update () {

	}

     


    IEnumerator goToPoint() {


        float duration = (_Start.transform.position - _Target.transform.position).magnitude/_Speed ;
        float t = 0;

        while (t < duration) {
            t += Time.deltaTime;

            transform.position = Vector3.Lerp(_Start.transform.position, _Target.transform.position, t / duration);

            yield return null;              
        }
        transform.position = _Target.transform.position;

        _Start = _Target;
        _Target = _Target.getMostAccurateDestinaton(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0 ).normalized);

        if( _Target!= null)
            StartCoroutine(goToPoint());
    }





    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0).normalized);


    }
    
}
