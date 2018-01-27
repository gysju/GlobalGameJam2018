using UnityEngine;

public class CameraTarget : MonoBehaviour
{

    Player player;
    public float JoystickRange;
    public float SmoothFactor = 3;
    private Vector3 smoothOffset;
	
	void Update ()
    {
        if (player == null)
            player = Object.FindObjectOfType<Player>();
        else
        {
            Vector3 userOffset = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (userOffset.magnitude > 1.0f)
                userOffset.Normalize();
            smoothOffset = Vector3.Lerp(smoothOffset, userOffset * JoystickRange, Time.deltaTime * SmoothFactor);
            transform.position = player.transform.position + smoothOffset;
        }
	}
}
