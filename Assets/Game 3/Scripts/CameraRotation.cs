using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game3Module
{
    public class CameraRotation : MonoBehaviour
    {
        public Camera leftcam;
        public Camera rightcam;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //Restrict_Y();
        }

        public void PreserveRotation_X()
        {
            PreserveRotation.Parallel_X(transform, leftcam.transform);
            PreserveRotation.Parallel_X(transform, rightcam.transform);
        }

        public void PreserveRotation_Y(float angle)
        {
            PreserveRotation.Parallel_Y_Translated(transform, leftcam.transform, angle);
            PreserveRotation.Parallel_Y_Translated(transform, rightcam.transform, angle);
        }

        public void ClampRotation_Y(Transform parentB, float x_max, float dy)
        {
            PreserveRotation.Clamp_Y(transform, parentB, leftcam.transform, x_max, dy);
            PreserveRotation.Clamp_Y(transform, parentB, rightcam.transform, x_max, dy);
        }

        public void ResetRotation()
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
}