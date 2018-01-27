using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour {

    public static CameraMovement _Instance = null;
    public Image _FadePlane;

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

    // Update is called once per frame
    void LateUpdate () {
        if( Player._Instance != null )
            transform.position = Vector3.Lerp(transform.position, new Vector3(Player._Instance.transform.position.x, transform.position.y, transform.position.z), 0.1f );
	}

    public void Snap() {
        if (Player._Instance != null)
            transform.position = new Vector3(Player._Instance.transform.position.x, transform.position.y, transform.position.z);
    }
}
