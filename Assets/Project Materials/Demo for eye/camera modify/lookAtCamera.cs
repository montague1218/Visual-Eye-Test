using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtCamera : MonoBehaviour {

    public Camera mainCamera;

	void Update () {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
        mainCamera.transform.rotation * Vector3.up);
	}
}
