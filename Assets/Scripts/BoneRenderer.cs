using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneRenderer : MonoBehaviour {

    private const float drawRadius = 0.05f;
    
    private void OnDrawGizmos()
    {
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
