﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public static Player _Instance = null;
    public bool _Immobile = true;
    public Point _Start;
    public Point _Target;

    public float _Speed = 3f;

    Vector3 LastInput = Vector3.right;

    Coroutine GoToPointCorroutine;

    public AnimationCurve _SpeedCurve;


    // Use this for initialization
    private void Awake()
    {
        if (!_Instance)
        {
            _Instance = this;

        }
        else {
            Destroy(this);
        }
    }

    private void Update()
    {

        if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            LastInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0).normalized;

        if ( GoToPointCorroutine == null && !_Immobile )
            GoToPointCorroutine = StartCoroutine(goToPoint());

    }

    IEnumerator goToPoint() {


        //float duration = (_Start.transform.position - _Target.transform.position).magnitude/_Speed ;
        float duration = Mathf.Lerp((_Start.transform.position - _Target.transform.position).magnitude / _Speed, 10f / _Speed, 0.5f);
        float t = 0;

        while (t < duration) {
            t += Time.deltaTime;

            transform.position = Vector3.Lerp(_Start.transform.position, _Target.transform.position, _SpeedCurve.Evaluate( t / duration) );

            yield return null;              
        }
        transform.position = _Target.transform.position;


        switch (_Target._Type)
        {
            case Point.PointType.Normal:
                {

                    _Start = _Target;
                    _Target = _Target.getMostAccurateDestinaton(LastInput);
                    break;
                }
            case Point.PointType.Dead:
                {
                    Kill();
                    break;
                }
            case Point.PointType.Fried:
                {
                    _Start = _Target;
                    _Target = _Target._Links[Random.Range(0, _Target._Links.Count-1)].getOtherPoint(_Target);
                    
                    break;
                }
            case Point.PointType.Back:
                {
                    Point temp = _Start;
                    _Start = _Target;
                    _Target = temp;
                    break;
                }
        }

        StopAllCoroutines();
        GoToPointCorroutine = null;
    }
    [ContextMenu("KillPlayer")]
    public void Kill() {

        _Immobile = true;
        LevelGenerator._Instance.StartResetRoutine();

    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + LastInput);


    }
    
}
