using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Player {


    public void Awake()
    {
        
    }

    IEnumerator goToPoint()
    {

        _CurrentLink = _Start.GetConnectingLink(_Target);
        //float duration = (_Start.transform.position - _Target.transform.position).magnitude/_Speed ;
        float duration = Mathf.Lerp((_Start.transform.position - _Target.transform.position).magnitude / _Speed, 10f / _Speed, 0.5f);
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            transform.position = Vector3.Lerp(_Start.transform.position, _Target.transform.position, _SpeedCurve.Evaluate(t / duration));

            yield return null;
        }
        transform.position = _Target.transform.position;

        if (_CurrentLink)
            _CurrentLink.OnCrossed();


        switch (_Target._Type)
        {
            case Point.PointType.Normal:            
            case Point.PointType.Fried:
                {
                    _Start = _Target;
                    _Target = _Target.GetRandomBackwardPath();

                    break;
                }
            case Point.PointType.Dead:
                {
                    Kill();
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
}
