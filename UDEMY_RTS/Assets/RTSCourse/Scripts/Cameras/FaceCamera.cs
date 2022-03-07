using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTSCourse.Cameras
{
    public class FaceCamera : MonoBehaviour
    {

        private Transform mainCameraTransform;
        // Start is called before the first frame update
        void Start()
        {
            mainCameraTransform = Camera.main.transform;
        }

        private void LateUpdate() 
        {
            transform.LookAt(
                transform.position + mainCameraTransform.rotation * Vector3.forward,
                mainCameraTransform.rotation * Vector3.up);
        }
    }
}