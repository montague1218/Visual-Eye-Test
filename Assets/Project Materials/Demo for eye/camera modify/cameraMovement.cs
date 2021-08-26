using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cameraMovement : MonoBehaviour {

    public GameObject parentCamera;
    [Header("Camera angle detail display")]
    public Text log;
    public bool showLog = false;

    [Header("Camera limitation")]
    public float mulXLocal = 1, mulYLocal = 1, mulZLocal = 1;
    public bool LimitX = true;
    public bool LimitY = false;
    public bool LimitZ = false;

    [Range(-85, 85)]
    public float minX = -85;
    [Range(-85, 85)]
    public float maxX = 85;
    [Range(-180, 180)]
    public float minY = -135;
    [Range(-180, 180)]
    public float maxY = 135;
    [Range(-180, 180)]
    public float minZ = -135;
    [Range(-180, 180)]
    public float maxZ = 135;

    private void Update()
    {

        float x = parentCamera.transform.localEulerAngles.x;
        float y = parentCamera.transform.localEulerAngles.y;
        float z = parentCamera.transform.localEulerAngles.z;

        x = x >= 180f ? (x - 360f) : (x <= -180f ? (x + 360f) : x);
        x *= mulXLocal;
        x = LimitX ? Mathf.Clamp(x, minX, maxX) : x;

        y = y >= 180f ? (y - 360f) : (y <= -180f ? (y + 360f) : y);
        y *= mulYLocal;
        y = LimitY ? Mathf.Clamp(y, minY, maxY) : y;

        z = z >= 180f ? (z - 360f) : (z <= -180f ? (z + 360f) : z);
        z *= mulZLocal;
        z = LimitZ ? Mathf.Clamp(z, minZ, maxZ) : z;

        transform.localEulerAngles = new Vector3(x, y, z);

        if (showLog) { 
            log.text = "lEr " + x + " " + y + " " + z + "\n\n";
            log.text += "tr " + transform.eulerAngles.x + " " + transform.eulerAngles.y + " " + transform.eulerAngles.z + "\n";
            log.text += "tlr " + transform.localEulerAngles.x + " " + transform.localEulerAngles.y + " " + transform.localEulerAngles.z + "\n";
            }
    }


}
