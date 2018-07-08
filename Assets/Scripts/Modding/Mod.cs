using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MafiaUnity
{
    [Serializable]
    public class Mod
    {
        public string name;
        public string author;
        public string version;
        public string gameVersion;

        [SerializeField] List<string> dependencies;

        public void Init()
        {

        }

        public void Destroy()
        {

        }
    }
}