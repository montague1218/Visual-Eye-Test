using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game3Module
{
    public static class PreserveRotation
    {
        /// 
        /// In default settings, VR camera must be free-moving objects
        /// In order to preserve the rotation for the camera on only one axis,
        /// say X-axis, we allow the left/right cameras (child) to rotate,
        /// whilst we rotate main camera (parent) in opposite x-direction. So the relative
        /// x-rotation between the parent and any objects belong to it remains constant.
        ///

        [System.Obsolete]
        public static void RotateAround_X(Transform parent, Transform child)
        {
            Quaternion L = child.localRotation;
            Vector3 e = L.eulerAngles;
            parent.RotateAround(Vector3.up, -e.x);
        }


       
        public static void Parallel_X_Translated(Transform parent, Transform child, float angle)
        {
            Quaternion L = child.localRotation;
            Vector3 e = L.eulerAngles;
            Vector3 e2 = parent.rotation.eulerAngles;
            parent.rotation = Quaternion.Euler(angle - e.x, e2.y, e2.z);
        }

        public static void Parallel_X(Transform parent, Transform child)
        {
            Parallel_X_Translated(parent, child, 0f);
        }

        public static void Parallel_Y_Translated(Transform parent, Transform child, float angle)
        {
            Quaternion L = child.localRotation;
            Vector3 e = L.eulerAngles;
            Vector3 e2 = parent.rotation.eulerAngles;
            parent.rotation = Quaternion.Euler(e2.x, angle - e.y, e2.z);
            Debug.Log(child.rotation.eulerAngles);
        }

        public static void Parallel_Y(Transform parent, Transform child)
        {
            Parallel_Y_Translated(parent, child, 0f);
        }


        public static void Clamp_Y(Transform parentA, Transform parentB, Transform child, float x_max, float dy)
        {
            Vector3 e = child.rotation.eulerAngles;
            float t = e.x;
            if (t > 180f) t -= 360f;

            if (t < -1f)
            {
                Parallel_X(parentB, child);
            }
            else if (t > x_max)
            {
                Parallel_X_Translated(parentB, child, x_max);
                Parallel_Y_Translated(parentA, child, dy);
            }
            else
            {
                Parallel_Y_Translated(parentA, child, dy);
            }

            Debug.Log(t);
        }

    }
}