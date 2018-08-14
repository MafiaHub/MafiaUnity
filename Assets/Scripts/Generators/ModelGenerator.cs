using B83.Image.BMP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace MafiaUnity
{
    public class ModelGenerator : BaseGenerator
    {
        Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

        public override GameObject LoadObject(string path)
        {
            GameObject rootObject = LoadCachedObject(path);
            
            if (rootObject == null)
                rootObject = new GameObject(path);
            else
                return rootObject;

            Stream fs;

            try
            {
                fs = GameManager.instance.fileSystem.GetStreamFromPath(path);
            }
            catch (Exception ex)
            {
                GameObject.DestroyImmediate(rootObject);
                Debug.LogWarning(ex.ToString());
                return null;
            }

            using (BinaryReader reader = new BinaryReader(fs))
            {
                var modelLoader = new MafiaFormats.Reader4DS();
                var model = modelLoader.loadModel(reader);

                var meshId = 0;

                var children = new List<KeyValuePair<int, Transform>>();

                foreach (var mafiaMesh in model.meshes)
                {
                    var child = new GameObject(mafiaMesh.meshName, typeof(MeshFilter));
                    var meshFilter = child.GetComponent<MeshFilter>();
                    
                    children.Add(new KeyValuePair<int, Transform>(mafiaMesh.parentID, child.transform));

                    if (mafiaMesh.meshType == MafiaFormats.MeshType.Bone)
                    {
                        var bone = child.AddComponent<Bone>();
                        bone.data = mafiaMesh.bone;
                        continue;
                    }
                    else if (mafiaMesh.meshType == MafiaFormats.MeshType.Collision)
                    {
                        Material[] temp;
                        child.AddComponent<MeshCollider>().sharedMesh = GenerateMesh(mafiaMesh, child, mafiaMesh.standard.lods[0], model, out temp);
                        continue;
                    }
                    else if (mafiaMesh.meshType != MafiaFormats.MeshType.Standard)
                        continue;

                    if (mafiaMesh.standard.instanced != 0)
                        continue;
                    
                    Material[] materials;

                    switch (mafiaMesh.visualMeshType)
                    {
                        case MafiaFormats.VisualMeshType.Standard:
                        {
                            // TODO build up more lods
                            if (mafiaMesh.standard.lods.Count > 0)
                            {
                                var meshRenderer = child.AddComponent<MeshRenderer>();
                                meshFilter.mesh = GenerateMesh(mafiaMesh, child, mafiaMesh.standard.lods[0], model, out materials);
                                meshRenderer.materials = materials;
                                meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;

                                bool isTwoSided = model.materials.FindAll(x => (x.flags.HasFlag(MafiaFormats.MaterialFlag.Doublesided_Material))).Count > 0;

                                if (isTwoSided)
                                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                                // Handle special textures
                                foreach (var m in meshRenderer.sharedMaterials)
                                {
                                    var name = m.GetTexture("_MainTex")?.name;

                                    if (IsTextureGlow(name))
                                    {
                                        var glowTexture = (Texture2D)Resources.Load("Flares/" + Path.GetFileNameWithoutExtension(name));

                                        m.shader = Shader.Find("Unlit/Transparent");
                                        m.SetTexture("_MainTex", glowTexture);
                                        
                                        break;
                                    }
                                }
                            }
                            else
                                continue;
                        }
                        break;

                        case MafiaFormats.VisualMeshType.Single_Mesh:
                        {
                            var meshRenderer = child.AddComponent<SkinnedMeshRenderer>();
                            meshFilter.mesh = GenerateMesh(mafiaMesh, child, mafiaMesh.singleMesh.standard.lods[0], model, out materials);
                            meshRenderer.materials = materials;
                            meshRenderer.sharedMesh = meshFilter.sharedMesh;
                            meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;

                            var data = child.AddComponent<SkinnedMeshData>();
                            data.mesh = mafiaMesh.singleMesh;
                        }
                        break;

                        case MafiaFormats.VisualMeshType.Single_Morph:
                        {
                            var meshRenderer = child.AddComponent<SkinnedMeshRenderer>();
                            meshFilter.mesh = GenerateMesh(mafiaMesh, child, mafiaMesh.singleMorph.singleMesh.standard.lods[0], model, out materials);
                            meshRenderer.materials = materials;
                            meshRenderer.sharedMesh = meshFilter.sharedMesh;
                            meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;

                            var data = child.AddComponent<SkinnedMeshData>();
                            data.mesh = mafiaMesh.singleMorph.singleMesh;
                        }
                        break;

                        case MafiaFormats.VisualMeshType.Billboard:
                        {
                            // TODO build up more lods
                            var standard = mafiaMesh.billboard.standard;
                            
                            if (standard.lods.Count > 0)
                            {
                                //NOTE: (DavoSK) Add our custom billboard here
                                child.AddComponent<CustomBillboard>();

                                var meshRenderer = child.AddComponent<MeshRenderer>();

                                meshFilter.mesh = GenerateMesh(mafiaMesh, child, standard.lods[0], model, out materials);
                                meshRenderer.materials = materials;

                                bool isTwoSided = model.materials.FindAll(x => (x.flags.HasFlag(MafiaFormats.MaterialFlag.Doublesided_Material))).Count > 0;

                                if (isTwoSided)
                                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                                // Handle special textures
                                foreach (var m in meshRenderer.sharedMaterials)
                                {
                                    var name = m.GetTexture("_MainTex")?.name;

                                    if (IsTextureGlow(name))
                                    {
                                        var glowTexture = (Texture2D)Resources.Load("Flares/" + Path.GetFileNameWithoutExtension(name));

                                        m.shader = Shader.Find("Unlit/Transparent");
                                        m.SetTexture("_MainTex", glowTexture);

                                        break;
                                    }
                                }
                            }
                            else
                                continue;
                        }
                        break;

                        case MafiaFormats.VisualMeshType.Glow:
                        {
                            List<string> usedMaps = new List<string>();

                            foreach (var g in mafiaMesh.glow.glowData)
                            {
                                if (g.materialID-1 >= model.materials.Count)
                                    continue;

                                var matID = g.materialID-1;

                                var mat = model.materials[matID];
                                var mapName = mat.diffuseMapName;

                                if (usedMaps.Contains(mapName))
                                    continue;

                                foreach (var m in model.meshes)
                                {
                                    if (m.standard.lods == null)
                                        continue;

                                    if (m.standard.lods.Count < 1)
                                        continue;

                                    bool used = false;

                                    foreach (var gr in m.standard.lods[0].faceGroups)
                                    {
                                        if (gr.materialID == matID)
                                        {
                                            GenerateGlow(mapName, rootObject, m.pos);

                                            used = true;
                                            break;
                                        }
                                    }

                                    if (used == true)
                                        break;
                                }

                                usedMaps.Add(mapName);
                            }
                        }
                        break;

                        // TODO add more visual types

                        default: continue;
                    }
                    
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

                // NOTE(zaklaus): Do some extra work if this is a skinned mesh
                var baseObject = rootObject.transform.Find("base");

                if (baseObject != null)
                {
                    var skinnedMesh = baseObject.GetComponent<SkinnedMeshRenderer>();

                    if (skinnedMesh != null)
                    {
                        var data = baseObject.GetComponent<SkinnedMeshData>();
                        var boneData = data.mesh.LODs[0];
                        var bones = new List<Bone>(skinnedMesh.GetComponentsInChildren<Bone>());
                        var boneArray = new Transform[bones.Count];
                        
                        foreach (var b in bones)
                        {
                            boneArray[b.data.boneID] = b.transform;
                        }
                        
                        var boneTransforms = new List<Transform>(boneArray);
                        var bindPoses = new Matrix4x4[bones.Count];
                        var boneWeights = new BoneWeight[skinnedMesh.sharedMesh.vertexCount];

                        skinnedMesh.bones = boneArray;
                        
                        int skipVertices = (int)boneData.nonWeightedVertCount;
                        
                        for (int i = 0; i < boneData.joints.Count; i++)
                        {
                            bindPoses[i] = boneData.joints[i].transform;

                            for (int j = 0; j < boneData.joints[i].oneWeightedVertCount; j++)
                            {
                                boneWeights[skipVertices + j].boneIndex0 = i;
                                boneWeights[skipVertices + j].weight0 = 1f;
                            }

                            skipVertices += (int)boneData.joints[i].oneWeightedVertCount;

                            for (int j = 0; j < boneData.joints[i].weights.Count; j++)
                            {
                                boneWeights[skipVertices + j].boneIndex0 = i;
                                boneWeights[skipVertices + j].weight0 = boneData.joints[i].weights[j];
                                boneWeights[skipVertices + j].boneIndex1 = (int)boneData.joints[i].boneID;
                                boneWeights[skipVertices + j].weight1 = 1f - boneData.joints[i].weights[j]; 
                            }

                            skipVertices += boneData.joints[i].weights.Count;

                        }

                        skinnedMesh.sharedMesh.bindposes = bindPoses;
                        skinnedMesh.sharedMesh.boneWeights = boneWeights;
                    }
                }

                children.Clear();
            }
            
            StoreChachedObject(path, rootObject);
            
            fs.Close();

            return rootObject;
        }

        Mesh GenerateMesh(MafiaFormats.Mesh mafiaMesh, GameObject ent, MafiaFormats.LOD firstMafiaLOD, MafiaFormats.Model model, out Material[] materials)
        {
            var mesh = new Mesh();
            
            var bmp = new BMPLoader();
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
            
            mesh.name = mafiaMesh.meshName;

            mesh.SetVertices(unityVerts);
            mesh.SetUVs(0, unityUV);
            mesh.SetNormals(unityNormals);

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

                var matId = (int)Mathf.Max(0, Mathf.Min(model.materials.Count - 1, faceGroup.materialID - 1));

                if (model.materials.Count > 0)
                {
                    var mafiaMat = model.materials[matId];

                    Material mat;

                    if ((mafiaMat.flags & MafiaFormats.MaterialFlag.Colorkey) != 0)
                    {
                        //mat = new Material(Shader.Find("Standard"));
                        mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse"));
                        mat.SetFloat("_Cutoff", 0.9f);
                        // mat.SetFloat("_Mode", 1f); // Set rendering mode to Cutout
                        // mat.SetFloat("_Glossiness", 0f);
                        // mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        // mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        // mat.SetInt("_ZWrite", 1);
                        // mat.DisableKeyword("_ALPHATEST_ON");
                        // mat.EnableKeyword("_ALPHABLEND_ON");
                        // mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        // mat.renderQueue = 3000;
                    }
                    else if (mafiaMat.transparency < 1)
                    {
                        mat = new Material(Shader.Find("Standard"));
                        mat.SetFloat("_Mode", 3f); // Set rendering mode to Transparent
                        mat.SetFloat("_Glossiness", 0f);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 1);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                    else
                    {
                        mat = new Material(Shader.Find("Mafia/Diffuse"));
                        //mat.SetFloat("_Glossiness", 0f);
                    }

                    if (mafiaMat.diffuseMapName != null ||
                        mafiaMat.alphaMapName != null)
                    {
                        if ((mafiaMat.flags & MafiaFormats.MaterialFlag.Colorkey) != 0)
                            BMPLoader.useTransparencyKey = true;

                        Texture2D tex = null;
                        string finalMapName = "";

                        if ((mafiaMat.flags & MafiaFormats.MaterialFlag.Textured_Diffuse) != 0)
                            finalMapName = mafiaMat.diffuseMapName;
                        else if (mafiaMat.alphaMapName != null)
                            finalMapName = mafiaMat.alphaMapName;

                        var modMapName = GameManager.instance.fileSystem.GetPath(Path.Combine("maps", finalMapName));

                        if (cachedTextures.ContainsKey(modMapName))
                            tex = cachedTextures[modMapName];

                        if (tex == null)
                        {
                            BMPImage image = null;

                            try
                            {
                                image = bmp.LoadBMP(GameManager.instance.fileSystem.GetStreamFromPath(Path.Combine("maps", finalMapName)));
                            }
                            catch
                            {
                                if ((mafiaMat.flags & MafiaFormats.MaterialFlag.Textured_Diffuse) != 0)
                                    Debug.LogWarningFormat("Image {0} couldn't be loaded!", mafiaMat.diffuseMapName);
                                else if (mafiaMat.alphaMapName != null)
                                    Debug.LogWarningFormat("Image {0} couldn't be loaded!", mafiaMat.alphaMapName);

                            }

                            if (image != null)
                                tex = image.ToTexture2D();
                        }

                        BMPLoader.useTransparencyKey = false;

                        if (tex != null)
                        {
                            tex.name = finalMapName;

                            mat.SetTexture("_MainTex", tex);

                            if (GameManager.instance.cvarManager.Get("filterMode", "1") == "0")
                                tex.filterMode = FilterMode.Point;

                            if (!cachedTextures.ContainsKey(modMapName))
                                cachedTextures.Add(modMapName, tex);
                        }

                        if (mafiaMat.transparency < 1)
                            mat.SetColor("_Color", new Color32(255, 255, 255, (byte)(mafiaMat.transparency * 255)));

                        if ((mafiaMat.flags & (MafiaFormats.MaterialFlag.Animated_Texture_Diffuse | MafiaFormats.MaterialFlag.Animated_Texture_Alpha)) != 0)
                        {
                            List<Texture2D> frames = new List<Texture2D>();

                            string fileName = null;

                            if ((mafiaMat.flags & MafiaFormats.MaterialFlag.Animated_Texture_Diffuse) != 0)
                                fileName = mafiaMat.diffuseMapName;
                            else
                                fileName = mafiaMat.alphaMapName;

                            if ((mafiaMat.flags & MafiaFormats.MaterialFlag.Colorkey) != 0)
                                BMPLoader.useTransparencyKey = true;

                            if (fileName != null)
                            {
                                var path = fileName.Split('.');
                                string baseName = path[0];
                                string ext = path[1];

                                baseName = baseName.Substring(0, baseName.Length - 2);

                                for (int k = 0; k < mafiaMat.animSequenceLength; k++)
                                {
                                    try
                                    {
                                        var animPath = Path.Combine("maps", baseName + k.ToString("D2") + "." + ext);
                                        var frameImage = bmp.LoadBMP(GameManager.instance.fileSystem.GetStreamFromPath(animPath));

                                        if (frameImage == null)
                                            continue;

                                        var frame = frameImage.ToTexture2D();
                                        frames.Add(frame);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.LogError(ex.ToString());
                                    }
                                }

                                var framePlayer = ent.AddComponent<TextureAnimationPlayer>();

                                framePlayer.frames = frames;
                                framePlayer.framePeriod = mafiaMat.framePeriod;
                                framePlayer.material = mat;
                            }

                            BMPLoader.useTransparencyKey = false;
                        }
                    }
                    
                    mats.Add(mat);
                }

                faceGroupId++;
            }

            materials = mats.ToArray();

            return mesh;
        }

        void GenerateGlow(string mapName, GameObject rootObject, Vector3 pos)
        {
            var flareObject = new GameObject("Flare " + mapName);
            flareObject.transform.parent = rootObject.transform;
            flareObject.transform.localPosition = pos;

            string glowName = Path.GetFileNameWithoutExtension(mapName);

            var glow = flareObject.AddComponent<LensFlare>();
            flareObject.AddComponent<LensFlareFixedDistance>();

            var flarePrefab = (Flare)Resources.Load("Flares/" + glowName + "_FLARE");

            if (flarePrefab == null)
            {
                Debug.LogWarningFormat("Flare {0} couldn't be found!", glowName);
                GameObject.DestroyImmediate(flareObject);
                return;
            }

            var flare = (Flare)GameObject.Instantiate(flarePrefab);
            glow.flare = flare;
            glow.fadeSpeed = 8f;
            glow.brightness = 2f;
        }

        bool IsTextureGlow(string mapName)
        {
            string glowName = Path.GetFileNameWithoutExtension(mapName);

            return glowNames.Contains(glowName);
        }

        List<string> glowNames = new List<string> {
            "00GLOW",
            "2CLGL",
            "2CBGL",
        };
    }
}