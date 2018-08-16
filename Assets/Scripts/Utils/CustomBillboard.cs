using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public class CustomBillboard : MonoBehaviour
    {
        GameObject cameraObject = null;

        void Start()
        {
            cameraObject = GameObject.Find("Main Camera");
        }

        // Update is called once per frame
        void Update()
        {
            if (cameraObject == null)
                return;
                
            transform.LookAt(cameraObject.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }
}
