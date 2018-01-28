using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public static Player _Instance = null;
    public bool _Immobile = true;
    public Point _Start;
    public Point _Target;
    public Link _CurrentLink;
    public float _Speed = 3f;
    [HideInInspector] public float _Range = 3f;
    [ColorUsage(true,true, 0, 100, 0, 100)]public Color _Color;

    Vector3 LastInput = Vector3.right;

    protected Coroutine GoToPointCorroutine;

    public AnimationCurve _SpeedCurve;
    public AnimationCurve _RangeCurve;

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


        if (_Target) {
            _Target.getMostAccurateDestinaton(LastInput);
        }

        if ( GoToPointCorroutine == null && !_Immobile )
            GoToPointCorroutine = StartCoroutine(goToPoint());

    }

    public virtual IEnumerator goToPoint() {

        _CurrentLink = _Start.GetConnectingLink(_Target);
        //float duration = (_Start.transform.position - _Target.transform.position).magnitude/_Speed ;
        float duration = Mathf.Lerp((_Start.transform.position - _Target.transform.position).magnitude / _Speed, 10f / _Speed, 0.5f);
        float t = 0;

        while (t < duration) {
            t += Time.deltaTime;

            transform.position = Vector3.Lerp(_Start.transform.position, _Target.transform.position, _SpeedCurve.Evaluate( t / duration) );
            _Range = _RangeCurve.Evaluate(t / duration);

            yield return null;              
        }
        transform.position = _Target.transform.position;

        if (_CurrentLink)
            _CurrentLink.OnCrossed();

        if (_Target == LevelGenerator._Instance._Points[LevelGenerator._Instance._Points.Count - 1])
        {
            Win();
        }
        else
        {
            _Target.ClearAllDots();
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
                        _Target = _Target.GetRandomForwardPath() ;
                    
                        break;
                    }
                case Point.PointType.Back:
                    {
                        Point temp = _Start;
                        _Start = _Target;
                        _Target._Type = Point.PointType.Normal;
                        _Target = temp;

                    
                        break;
                    }
            }
        }

        StopAllCoroutines();
        GoToPointCorroutine = null;
    }

    public void Win()
    {
        _Immobile = true;
        CanvasManager._Instance.GoToWinMenu();
    }

    [ContextMenu("KillPlayer")]
    public void Kill() {

        _Immobile = true;
        CanvasManager._Instance.GoToDeathMenu();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.color = Color.yellow;
        
        Gizmos.DrawLine(transform.position, transform.position + LastInput);
    }
}
