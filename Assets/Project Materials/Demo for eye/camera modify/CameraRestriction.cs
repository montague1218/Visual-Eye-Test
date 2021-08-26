using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRestriction : MonoBehaviour {

    [SerializeField, Range(-135, 135)]
    private float minX = -135f;
    [SerializeField, Range(-135, 135)]
    private float maxX = 135f;

    // Update is called once per frame
    void Update()
    {
        if (transform.rotation.x < minX)
            transform.localRotation = Quaternion.Euler(minX, transform.rotation.y, transform.rotation.z);

        if (QuizModule.QuizManager.instance.current_UI == 4 || QuizModule.QuizManager.instance.current_UI == 5 ||
            QuizModule.QuizManager.instance.current_UI == 7)
            ResetRotation();
    }

    
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void EnableView()
    {
        Show("TransparentFX");
        Show("Ignore Raycast");
        Show("Water");
        Show("UI");
    }

    public void DisableView()
    {
        Hide("TransparentFX");
        Hide("Ignore Raycast");
        Hide("Water");
        Hide("UI");
    }

    // Turn on the bit using an OR operation:
    private void Show(string layer)
    {
        Camera camera = gameObject.GetComponent<Camera>();
        camera.cullingMask |= 1 << LayerMask.NameToLayer(layer);
    }

    // Turn off the bit using an AND operation with the complement of the shifted int:
    private void Hide(string layer)
    {
        Camera camera = gameObject.GetComponent<Camera>();
        camera.cullingMask &= ~(1 << LayerMask.NameToLayer(layer));
    }

    // Toggle the bit using a XOR operation:
    private void Toggle(string layer)
    {
        Camera camera = gameObject.GetComponent<Camera>();
        camera.cullingMask ^= 1 << LayerMask.NameToLayer(layer);
    }
}
