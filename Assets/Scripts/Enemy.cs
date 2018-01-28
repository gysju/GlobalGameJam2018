using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Player {

    public float _CollisionDistance;

    public void Awake()
    {
        _Start = LevelGenerator._Instance._Points[LevelGenerator._Instance._Points.Count - 1];
        _Target = _Start._Links[0].getOtherPoint(_Start);
        _Immobile = false;
    }

    private void Update()
    {

        if (_Instance && !_Instance._Immobile && (_Instance.transform.position - transform.position).magnitude < _CollisionDistance)
        {
            SoundManager.Instance.SpawnPlaySound("SndDieByEnemy", Vector3.zero);
            _Instance.Kill();
        }
        if (GoToPointCorroutine == null && !_Immobile)
            GoToPointCorroutine = StartCoroutine(goToPoint());

    }

    public override IEnumerator goToPoint()
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
                    //Kill();
                    LevelGenerator._Instance._Enemies.Remove(this);
                    Destroy(gameObject);
                    break;
                }
            case Point.PointType.Back:
                {
                   //Point temp = _Start;
                   //_Start = _Target;
                   //_Target = temp;
                    _Start = _Target;
                    _Target = _Target.GetRandomForwardPath();
                    break;
                }
        }

        StopAllCoroutines();
        GoToPointCorroutine = null;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

}
