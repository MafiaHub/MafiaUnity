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
            if (models.ContainsKey(path))
                return obj;

            var copy = GameObject.Instantiate(obj, obj.transform);
            copy.SetActive(false);
            copy.name = path + " (cached)";

            models.Add(path, copy);

            return copy;
        }

        public virtual GameObject LoadObject(string path, Mission mission)
        {
            // NOTE(zaklaus): Implemented elsewhere...
            return null;
        }

        protected void StoreReference(Mission mission, string name, GameObject gameObject)
        {
            if (mission == null) return;

            if (!mission.referenceMap.ContainsKey(name))
                mission.referenceMap.Add(name, gameObject);
        }

        protected void StoreCacheReference(Mission mission, string name, GameObject gameObject)
        {
            if (mission == null) return;

            if (!mission.cacheReferenceMap.ContainsKey(name))
                mission.cacheReferenceMap.Add(name, gameObject);
        }
        
        public static GameObject FetchCacheReference(Mission mission, string name)
        {
            if (mission == null) return null;

            GameObject gameObject = null;
            mission.cacheReferenceMap.TryGetValue(name, out gameObject);

            if (gameObject == null)
                mission.referenceMap.TryGetValue(name, out gameObject);

            return gameObject;
        }

        public static GameObject FetchReference(Mission mission, string name)
        {
            if (mission == null) return null;

            GameObject gameObject = null;
            mission.referenceMap.TryGetValue(name, out gameObject);

            return gameObject;
        }
    }
}
