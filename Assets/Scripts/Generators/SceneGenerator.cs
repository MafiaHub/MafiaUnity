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

        public override GameObject LoadObject(string path)
        {
            GameObject rootObject = LoadCachedObject(path);

            if (rootObject == null)
                rootObject = new GameObject(path);
            else
                return rootObject;

            FileStream fs;

            try
            {
                fs = new FileStream(GameManager.instance.fileSystem.GetPath(path), FileMode.Open);
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

                var objects = new List<KeyValuePair<GameObject, MafiaFormats.Scene2BINLoader.Object>>();

                var backdrop = new GameObject("Backdrop sector");
                backdrop.transform.parent = rootObject.transform;

                foreach (var obj in sceneLoader.objects)
                {
                    GameObject newObject;

                    if (obj.Value.modelName == null || obj.Value.type != MafiaFormats.Scene2BINLoader.ObjectType.Model)
                        newObject = new GameObject();
                    else
                        newObject = GameManager.instance.modelGenerator.LoadObject(Path.Combine("models", obj.Value.modelName));
                    
                    newObject.name = obj.Value.name;

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
                        var parentObject = GameObject.Find(obj.Value.parentName);
                        
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
