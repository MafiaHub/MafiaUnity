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
            var dir = Camera.main.transform.position - transform.position;
            dir.y = 0f;
            transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
