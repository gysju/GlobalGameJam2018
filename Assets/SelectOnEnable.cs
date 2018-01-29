using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectOnEnable : MonoBehaviour {
    public Button _ButtonToEnable;

    void OnEnable()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject( _ButtonToEnable.gameObject );
            _ButtonToEnable.OnSelect(null);
        }
    }
}
