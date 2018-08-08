using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public class CustomBillboard : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(Camera.main.transform);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }
}
