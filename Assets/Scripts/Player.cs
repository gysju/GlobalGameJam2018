using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class Player : MonoBehaviour {

    public static Player _Instance = null;
    public bool _Immobile = true;
    public Point _Start;
    public Point _Target;
    public Link _CurrentLink;
    public float _Speed = 3f;
    public bool isPlayer;
    [HideInInspector] public float _Range = 3f;
    [ColorUsage(true,true, 0, 100, 0, 100)]public Color _Color;

    Vector3 LastInput = Vector3.right;

    protected Coroutine GoToPointCorroutine;

    public AnimationCurve _SpeedCurve;
    public AnimationCurve _RangeCurve;
    public AnimationCurve _FlockCurve;

    AudioSource _audioSource;
    private LevelMesh level;

    void Start()
    {
        level = LevelReader.CurrentLevel;
    }

    // Use this for initialization
    private void Awake()
    {
        isPlayer = true;
        if (!_Instance)
        {
            _Instance = this;
            _Speed = LevelGenerator._Instance.CurrentLevel.PlayerSpeed;
            _audioSource = GetComponent<AudioSource>();
        }
        else {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            LastInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0).normalized;


        if (_Target) {
            _Target.getMostAccurateDestinaton(LastInput);
        }

        if ( GoToPointCorroutine == null && !_Immobile )
            GoToPointCorroutine = StartCoroutine(goToPoint());

    }

    public virtual IEnumerator goToPoint() {

        StartCoroutine(SetVibration(0.1f, 0.1f));
        string[] s = new string[] { "SndMovePT01", "SndMovePT02", "SndMovePT03", "SndMovePT04", "SndMovePT06" };
        SoundManager.Instance.PlaySoundOneShot(s, _audioSource);

        //string[] moveSnds = new string[] { "SndMove", "SndMove2" };
        //SoundManager.Instance.PlaySoundOneShot(moveSnds, _audioSource);

        _CurrentLink = _Start.GetConnectingLink(_Target);
        //float duration = (_Start.transform.position - _Target.transform.position).magnitude/_Speed ;
        float duration = Mathf.Lerp((level.GetPos(_Start) - level.GetPos(_Target)).magnitude / _Speed, 10f / _Speed, 0.5f);
        float t = 0;

        //SoundManager.Instance.PlaySoundOnShot("", _audioSource);

        while (t < duration) {
            t += Time.deltaTime;

            Vector2 pStart = level.GetPos(_Start);
            Vector2 pEnd = level.GetPos(_Target);
            transform.position = Vector3.Lerp(pStart, pEnd, _SpeedCurve.Evaluate( t / duration) );
            _Range = _RangeCurve.Evaluate(t / duration);
            Vector2 direction = (pEnd - pStart).normalized;
            if (isPlayer)
                LevelReaction.AddForceToNode(_Target, _FlockCurve.Evaluate(t / duration) * direction);
            yield return null;              
        }
        transform.position = level.GetPos(_Target);

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
                        SoundManager.Instance.SpawnPlaySound("SndDie", Vector3.zero);
                        Kill();
                        break;
                    }
                case Point.PointType.Fried:
                    {
                        _Start = _Target;
                        _Target = _Target.GetRandomForwardPath();
                        StartCoroutine(SetVibration(1f, 0.1f));

                        break;
                    }
                case Point.PointType.Back:
                    {
                        Point temp = _Start;
                        _Start = _Target;
                        _Target.SetDefaultType();
                        _Target = temp;
                        StartCoroutine(SetVibration(1f, 0.1f));

                        string[] snd = new string[] { "SndBouncePT1", "SndBouncePT2"};
                        SoundManager.Instance.SpawnPlaySound("SndBounce", Vector3.zero);
                        SoundManager.Instance.PlaySoundOneShot(s, _audioSource);

                        break;
                    }
            }
        }

        StopCoroutine(GoToPointCorroutine);
        GoToPointCorroutine = null;
    }

    public void Win()
    {
        SoundManager.Instance.SpawnPlaySound("SndVictoryPT01", Vector3.zero);
        SoundManager.Instance.SpawnPlaySound("SndVictory", Vector3.zero);
        StartCoroutine(SetVibration(0.2f, 0.5f));
        _Immobile = true;
        CanvasManager._Instance.GoToWinMenu();
    }

    [ContextMenu("KillPlayer")]
    public void Kill() {

        //SoundManager.Instance.PlaySoundOnShot("", _audioSource);
        string[] s = new string[] { "SndDeathPT01", "SndDeathPT01a", "SndDeathPT01b", "SndDeathPT01c", "SndDeathPT01d", "SndDeathPT02" };
        SoundManager.Instance.PlaySoundOneShot(s, _audioSource);
        
        _Immobile = true;
        CanvasManager._Instance.GoToDeathMenu();
        StartCoroutine(SetVibration(1.0f, 0.25f));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, 0.1f);
        Gizmos.color = Color.yellow;
        
        Gizmos.DrawLine(transform.position, transform.position + LastInput);
    }

    private IEnumerator SetVibration(float intensity, float duration)
    {
        GamePad.SetVibration(0, intensity, intensity);

        float t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;

            yield return null;
        }
        GamePad.SetVibration(0, 0.0f, 0.0f);
    }
}
