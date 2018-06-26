using B83.Image.BMP;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    public class GameLoader
    {

        #region Singleton
        static GameLoader instanceObject;
        public static GameLoader instance { get { if (instanceObject == null) instanceObject = new GameLoader(); return instanceObject; } }
        #endregion
        
        public string gamePath { get; private set; }

        public ModelLoader modelLoader = new ModelLoader();
            
        public bool SetGamePath(string path)
        {
            if (ValidateGamePath(path))
            {
                gamePath = path;
                return true;
            }

            return false;
        }

        bool ValidateGamePath(string path)
        {
            // TODO: Validate if game files are present there.
            return true;
        }
    }

    public class ModelLoader
    {
        public Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();

        private GameObject rootCachedStorage = new GameObject("Cached Models");

        public GameObject LoadModel(string path)
        {
            GameObject rootObject;
            if (models.ContainsKey(path) && models[path] != null)
            {
                rootObject = GameObject.Instantiate(models[path], Vector3.zero, Quaternion.identity);
                rootObject.SetActive(true);
                rootObject.name = path;
                rootObject.transform.parent = null;
                return rootObject;
            }
            else
                rootObject = new GameObject(path);

            FileStream fs;

            try
            {
                fs = new FileStream(GameLoader.instance.gamePath + path, FileMode.Open);
            }
            catch
            {
                return null;
            }
            
            using (BinaryReader reader = new BinaryReader(fs))
            {
                var modelLoader = new MafiaFormats.Reader4DS();
                var bmp = new BMPLoader();
                var model = modelLoader.loadModel(reader);

                var meshId = 0;

                var children = new List<KeyValuePair<int, Transform>>();

                foreach (var mafiaMesh in model.meshes)
                {
                    var child = new GameObject(mafiaMesh.meshName, typeof(MeshRenderer), typeof(MeshFilter));
                    var meshFilter = child.GetComponent<MeshFilter>();
                    var meshRenderer = child.GetComponent<MeshRenderer>();

                    children.Add(new KeyValuePair<int, Transform>(mafiaMesh.parentID, child.transform));

                    if (mafiaMesh.meshType != MafiaFormats.MeshType.MESHTYPE_STANDARD ||
                        mafiaMesh.visualMeshType != MafiaFormats.VisualMeshType.VISUALMESHTYPE_STANDARD)
                        continue;

                    if (mafiaMesh.standard.instanced != 0)
                        continue;

                    var firstMafiaLOD = mafiaMesh.standard.lods[0];
                    List<Material> mats = new List<Material>();

                    List<Vector3> unityVerts = new List<Vector3>();
                    List<Vector3> unityNormals = new List<Vector3>();
                    List<Vector2> unityUV = new List<Vector2>();

                    foreach (var vert in firstMafiaLOD.vertices)
                    {
                        unityVerts.Add(vert.pos);
                        unityNormals.Add(vert.normal);
                        unityUV.Add(new Vector2(vert.uv.x, -1 * vert.uv.y));
                    }

                    var mesh = new Mesh();
                    mesh.name = mafiaMesh.meshName;

                    mesh.SetVertices(unityVerts);
                    mesh.SetUVs(0, unityUV);
                    mesh.SetNormals(unityNormals);
                    meshFilter.mesh = mesh;

                    mesh.subMeshCount = firstMafiaLOD.faceGroups.Count;

                    var faceGroupId = 0;

                    foreach (var faceGroup in firstMafiaLOD.faceGroups)
                    {
                        List<int> unityIndices = new List<int>();
                        foreach (var face in faceGroup.faces)
                        {
                            unityIndices.Add(face.a);
                            unityIndices.Add(face.b);
                            unityIndices.Add(face.c);
                        }

                        mesh.SetTriangles(unityIndices.ToArray(), faceGroupId);

                        var mat = new Material(Shader.Find("Diffuse"));

                        var matId = (int)Mathf.Max(0, Mathf.Min(model.materials.Count - 1, faceGroup.materialID - 1));

                        if (matId > 0)
                        {
                            var mafiaMat = model.materials[matId];

                            // TODO support more types as well as transparency

                            if ((mafiaMat.flags & MafiaFormats.MaterialFlag.MATERIALFLAG_TEXTUREDIFFUSE) != 0)
                            {
                                var image = bmp.LoadBMP(GameLoader.instance.gamePath + "maps/" + mafiaMat.diffuseMapName);
                                Texture2D tex = image.ToTexture2D();
                                mat.SetTexture("_MainTex", tex);
                            }
                        }

                        mats.Add(mat);
                        faceGroupId++;
                    }
                    
                    meshRenderer.materials = mats.ToArray();

                    meshId++;
                }
                
                for (int i = 0; i < children.Count; i++)
                {
                    var parentId = children[i].Key;
                    var mafiaMesh = model.meshes[i];

                    if (parentId > 0)
                        children[i].Value.parent = children[parentId - 1].Value;
                    else
                        children[i].Value.parent = rootObject.transform;

                    children[i].Value.localPosition = mafiaMesh.pos;
                    children[i].Value.localRotation = mafiaMesh.rot;
                    children[i].Value.localScale = mafiaMesh.scale;
                }

                children.Clear();
            }

            var clonedObject = GameObject.Instantiate(rootObject, Vector3.zero, Quaternion.identity);
            clonedObject.name = "[CACHED] " + path;
            clonedObject.SetActive(false);

            if (rootCachedStorage == null)
                rootCachedStorage = new GameObject("Cached Models");

            clonedObject.transform.parent = rootCachedStorage.transform;
            models.Add(path, clonedObject);

            fs.Close();

            return rootObject;
        }
    }
}