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

                var objects = new List<KeyValuePair<GameObject, MafiaFormats.Scene2BINLoader.Object>>();

                var backdrop = new GameObject("Backdrop sector");
                backdrop.transform.parent = rootObject.transform;
                StoreReference(mission, backdrop.name, backdrop);

                var primary = new GameObject("Primary sector");
                primary.transform.parent = rootObject.transform;
                StoreReference(mission, primary.name, primary);

                foreach (var obj in sceneLoader.objects)
                {
                    GameObject newObject;

                    if (obj.Value.name == "Primary sector")
                        continue;


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

                foreach (var obj in objects)
                {
                    var newObject = obj.Key;

                    if (obj.Value.parentName != null)
                    {
                        var parentObject = FetchReference(mission, obj.Value.parentName);
                        
                        if (parentObject != null)
                            newObject.transform.parent = parentObject.transform;
                        else
                            newObject.transform.parent = rootObject.transform;
                    }
                    else
                        newObject.transform.parent = rootObject.transform;

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
    }
}
