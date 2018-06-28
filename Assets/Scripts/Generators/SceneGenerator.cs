using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
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
                fs = new FileStream(GameManager.instance.gamePath + path, FileMode.Open);
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

                foreach (var obj in sceneLoader.objects)
                {
                    if (obj.Value.type != MafiaFormats.Scene2BINLoader.ObjectType.OBJECT_TYPE_MODEL)
                        continue;

                    if (obj.Value.modelName == null)
                        continue;

                    var model = GameManager.instance.modelGenerator.LoadObject("models/" + obj.Value.modelName);

                    if (model == null)
                        continue;

                    model.transform.parent = rootObject.transform;
                    model.transform.localPosition = obj.Value.pos;
                    model.transform.localRotation = obj.Value.rot;
                    model.transform.localScale = obj.Value.scale;
                }
            }

            StoreChachedObject(path, rootObject);

            return rootObject;
        }
    }
}
