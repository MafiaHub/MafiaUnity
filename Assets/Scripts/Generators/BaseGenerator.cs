using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    public class BaseGenerator
    {
        public Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();
        
        protected GameObject LoadCachedObject(string path)
        {
            if (models.ContainsKey(path) && models[path] != null)
            {
                GameObject rootObject;
                rootObject = GameObject.Instantiate(models[path], Vector3.zero, Quaternion.identity);
                rootObject.SetActive(true);
                rootObject.name = path;
                rootObject.transform.parent = null;
                return rootObject;
            }
            else
                return null;
        }

        protected GameObject StoreChachedObject(string path, GameObject obj)
        {
            if (!models.ContainsKey(path))
                models.Add(path, obj);

            return obj;
        }

        public virtual GameObject LoadObject(string path)
        {
            // NOTE(zaklaus): Implemented elsewhere...
            return null;
        }
    }
}
