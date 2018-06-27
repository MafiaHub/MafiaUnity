using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    public class CityGenerator : BaseGenerator
    {
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
                fs = new FileStream(GameManager.instance.gamePath + path, FileMode.Open);
            }
            catch
            {
                return null;
            }

            using (var reader = new BinaryReader(fs))
            {
                var cacheBINLoader = new MafiaFormats.CacheBINLoader();

                cacheBINLoader.ReadCache(reader);

                foreach (var obj in cacheBINLoader.objects)
                {
                    var parentObject = new GameObject(obj.objectName);
                    parentObject.transform.parent = rootObject.transform;

                    foreach (var instance in obj.instances)
                    {
                        var model = GameManager.instance.modelGenerator.LoadObject("models/" + instance.modelName);

                        if (model == null)
                            continue;

                        model.transform.parent = parentObject.transform;
                        model.transform.localPosition = instance.pos;
                        model.transform.localRotation = instance.rot;
                        model.transform.localScale = instance.scale;
                    }
                }
            }

            rootObject.isStatic = true;

            StoreChachedObject(path, rootObject);

            return rootObject;
        }
    }
}
