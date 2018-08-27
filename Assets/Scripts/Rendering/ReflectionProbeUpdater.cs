using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionProbeUpdater : MonoBehaviour {

    ReflectionProbe probe = null;

    private void Awake()
    {
        probe = GetComponent<ReflectionProbe>();
    }

    private void FixedUpdate()
    {
        probe.RenderProbe();
    }
}
