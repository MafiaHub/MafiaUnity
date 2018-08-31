using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MafiaUnity
{
    public class SceneGenerator : BaseGenerator
    {
        public MafiaFormats.Scene2BINLoader lastLoader;

        public override GameObject LoadObject(string path, Mission mission)
        {
            GameObject rootObject = LoadCachedObject(path);

            if (rootObject == null)
                rootObject = new GameObject(path);
            else
                return rootObject;

            Stream fs;

            try
            {
                fs = GameAPI.instance.fileSystem.GetStreamFromPath(path);
            }
            catch
            {
                return null;
            }

            using (var reader = new BinaryReader(fs))
            {
                var sceneLoader = new MafiaFormats.Scene2BINLoader();
                lastLoader = sceneLoader;

                sceneLoader.Load(reader);
                fs.Close();

                // TODO: Check if refs are null, clear then
                ModelGenerator.cachedTextures.Clear();

                var objects = new List<KeyValuePair<GameObject, MafiaFormats.Scene2BINLoader.Object>>();

                var backdrop = new GameObject("Backdrop sector");
                backdrop.transform.parent = rootObject.transform;
                StoreReference(mission, backdrop.name, backdrop);

                foreach (var obj in sceneLoader.objects)
                {
                    GameObject newObject;

                    if (obj.Value.modelName == null || (obj.Value.type != MafiaFormats.Scene2BINLoader.ObjectType.Model && obj.Value.specialType == 0))
                        newObject = new GameObject();
                    else
                        newObject = GameAPI.instance.modelGenerator.LoadObject(Path.Combine("models", obj.Value.modelName), null);
                    
                    if (newObject == null)
                        continue;
                        
                    newObject.name = obj.Value.name;

                    StoreReference(mission, newObject.name, newObject);

                    newObject.transform.localPosition = obj.Value.pos;
                    newObject.transform.localRotation = obj.Value.rot;
                    newObject.transform.localScale = obj.Value.scale;
                    
                    objects.Add(new KeyValuePair<GameObject, MafiaFormats.Scene2BINLoader.Object>(newObject, obj.Value));
                }

                var primary = FetchReference(mission, "Primary sector");
                var objDef = primary.AddComponent<ObjectDefinition>();
                var dummySectorData = new MafiaFormats.Scene2BINLoader.Object();
                dummySectorData.type = MafiaFormats.Scene2BINLoader.ObjectType.Sector;
                objDef.data = dummySectorData;
                primary.transform.parent = rootObject.transform;

                foreach (var obj in objects)
                {
                    var newObject = obj.Key;

                    if (obj.Value.isPatch)
                    {
                        var sumMag = obj.Value.pos.magnitude+obj.Value.rot.eulerAngles.magnitude+obj.Value.pos.magnitude;

                        if (sumMag == 0f)
                        {
                            GameObject.DestroyImmediate(newObject, true);
                            continue;
                        }

                        var redefObject = FetchReference(mission, newObject.name);

                        if (redefObject != null)
                        {
                            if (obj.Value.parentName != null)
                            {
                                var parent = FindParent(mission, obj.Value.parentName);

                                if (parent != null)
                                    redefObject.transform.parent = parent.transform;
                            }

                            redefObject.transform.localPosition = obj.Value.pos;
                            redefObject.transform.localRotation = obj.Value.rot;
                            //redefObject.transform.localScale = obj.Value.scale;

                            GameObject.DestroyImmediate(newObject, true);
                            continue;
                        }
                    }

                    newObject.transform.parent = FindParent(mission, obj.Value.parentName, rootObject).transform;

                    newObject.transform.localPosition = obj.Value.pos;
                    newObject.transform.localRotation = obj.Value.rot;
                    newObject.transform.localScale = obj.Value.scale;

                    var specObject = newObject.AddComponent<ObjectDefinition>();
                    specObject.data = obj.Value;
                    specObject.Init();
                }
                
            }

            // NOTE(zaklaus): Hardcode 'Primary sector' scale to (1,1,1)
            var primarySector = GameObject.Find("Primary sector");
            
            if (primarySector != null)
                primarySector.transform.localScale = new Vector3(1,1,1);

            StoreChachedObject(path, rootObject);

            return rootObject;
        }

        GameObject FindParent(Mission mission, string name, GameObject defaultObject=null)
        {
            if (name != null)
            {
                var parentObject = FetchReference(mission, name);

                if (parentObject != null)
                    return parentObject;
                else
                    return defaultObject;
            }
            else
                return defaultObject;
        }
    }
}
