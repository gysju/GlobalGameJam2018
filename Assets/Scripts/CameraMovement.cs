using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour {

    public static CameraMovement _Instance = null;
    public Image _FadePlane;
    public float _HeightBias = 10.0f;

    private void Awake()
    {
        if (!_Instance)
        {
            _Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void LateUpdate () {
        camMovement();
    }

    void camMovement()
    {
        if (Player._Instance != null)
        {
            //X
            transform.position = Vector3.Lerp(transform.position, new Vector3(Player._Instance.transform.position.x, transform.position.y, transform.position.z), 0.1f);
            //Y
            if (Player._Instance.transform.position.y > transform.position.y + _HeightBias)
            {
                float bias = Player._Instance.transform.position.y - (transform.position.y + _HeightBias);
                transform.position += Vector3.up * bias;
            }
            else if (Player._Instance.transform.position.y < transform.position.y - _HeightBias)
            {
                float bias =  (transform.position.y - _HeightBias) - Player._Instance.transform.position.y;
                transform.position -= Vector3.up * bias;
            }
        }
        else
            transform.position = Vector3.Lerp(transform.position, new Vector3(0, transform.position.y, transform.position.z), 0.02f);
    }

    public void Snap() {
        if (Player._Instance != null)
            transform.position = new Vector3(Player._Instance.transform.position.x, transform.position.y, transform.position.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Vector3 pos = new Vector3( transform.position.x, transform.position.y, 0.0f);

        Gizmos.DrawLine(pos - Vector3.right * 100 - Vector3.up * _HeightBias, pos + Vector3.right * 100 - Vector3.up * _HeightBias );
        Gizmos.DrawLine(pos - Vector3.right * 100 + Vector3.up * _HeightBias, pos + Vector3.right * 100 + Vector3.up * _HeightBias );
    }
}
