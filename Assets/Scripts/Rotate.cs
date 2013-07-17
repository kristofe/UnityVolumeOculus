using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
    public float movementScale;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Quaternion q = transform.rotation;
        Vector3 rot = q.eulerAngles;
        rot.x += OVRGamepadController.GetAxisRightY() * movementScale;
        rot.y += OVRGamepadController.GetAxisRightX() * movementScale;
        rot.x += Input.GetAxis("Vertical") * movementScale;
        rot.y += Input.GetAxis("Horizontal") * movementScale;
        q.eulerAngles = rot;
        transform.rotation = q;
	}
}
