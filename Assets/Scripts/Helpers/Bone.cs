using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    public class Bone : MonoBehaviour {

        private const float drawRadius = .05f;
        public MafiaFormats.Bone data;
        public bool drawBones = false;

        private void OnDrawGizmos()
        {
            if (!drawBones)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, drawRadius);

            // TODO cache the results on a cost of possible miss?

            Gizmos.color = Color.red;
            foreach (Transform child in transform)
            {
                Gizmos.DrawLine(transform.position, child.position);
            }
        }   
    }
}