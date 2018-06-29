using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenMafia
{
    namespace MafiaFormats
    {
        [Flags]
        public enum MaterialFlag : uint
        {
            MATERIALFLAG_TEXTUREDIFFUSE = 0x00040000,          // whether diffuse texture is present
            MATERIALFLAG_COLORED = 0x08000000,                 // whether to use diffuse color (only applies with diffuse texture)
            MATERIALFLAG_MIPMAPPING = 0x00800000,
            MATERIALFLAG_ANIMATEDTEXTUREDIFFUSE = 0x04000000,
            MATERIALFLAG_ANIMATEXTEXTUREALPHA = 0x02000000,
            MATERIALFLAG_DOUBLESIDEDMATERIAL = 0x10000000,     // whether backface culling should be off
            MATERIALFLAG_ENVIRONMENTMAP = 0x00080000,          // simulates glossy material with environment texture
            MATERIALFLAG_NORMALTEXTUREBLEND = 0x00000100,      // blend between diffuse and environment texture normally
            MATERIALFLAG_MULTIPLYTEXTUREBLEND = 0x00000200,    // blend between diffuse and environment texture by multiplying
            MATERIALFLAG_ADDITIVETEXTUREBLEND = 0x00000400,    // blend between diffuse and environment texture by addition
            MATERIALFLAG_CALCREFLECTTEXTUREY = 0x00001000,
            MATERIALFLAG_PROJECTREFLECTTEXTUREY = 0x00002000,
            MATERIALFLAG_PROJECTREFLECTTEXTUREZ = 0x00004000,
            MATERIALFLAG_ADDITIONALEFFECT = 0x00008000,        // should be ALPHATEXTURE | COLORKEY | ADDITIVEMIXING
            MATERIALFLAG_ALPHATEXTURE = 0x40000000,
            MATERIALFLAG_COLORKEY = 0x20000000,
            MATERIALFLAG_ADDITIVEMIXING = 0x80000000           // the object is blended against the world by adding RGB (see street lamps etc.)
        }

        public enum MeshType : uint
        {
            MESHTYPE_STANDARD = 0x01,           // visual mesh
            MESHTYPE_COLLISION = 0x02,          // NOTE(zaklaus): Imaginary type based on mesh name "wcol*"
            MESHTYPE_SECTOR = 0x05,             // part of space, used for culling, effective lighting etc.
            MESHTYPE_DUMMY = 0x06,              // invisible bounding box
            MESHTYPE_TARGET = 0x07,             // used in human models (as a shooting target?)
            MESHTYPE_BONE = 0x0a                // for skeletal animation
        }

        public enum VisualMeshType : uint  // subtype of mesh, when MeshType == MESHTYPE_STANDARD
        {
            VISUALMESHTYPE_STANDARD = 0x0,      // normal mesh
            VISUALMESHTYPE_SINGLEMESH = 0x02,   // mesh with bones
            VISUALMESHTYPE_SINGLEMORPH = 0x03,  // combination of morph (for face) and skeletal (for body) animation
            VISUALMESHTYPE_BILLBOARD = 0x04,    // billboarding mesh (rotates towards camera
            VISUALMESHTYPE_MORPH = 0x05,        // mesh with morphing (non-skeletal) animation, e.g. curtains in wind
            VISUALMESHTYPE_GLOW = 0x06,         // has no geometry, only shows glow texture
            VISUALMESHTYPE_MIRROR = 0x08        // reflects the scene
        }

        [Flags]
        public enum MeshRenderFlag : uint
        {
            MESHRENDERFLAG_USEDEPTHBIAS = 0x0001,  // whether to receive shadows
            MESHRENDERFLAG_USESHADOWS = 0x0002,
            MESHRENDERFLAG_UNKNOWN = 0x0008,       // always 1
            MESHRENDERFLAG_USEPROJECTION = 0x0020, // used for projecting textures, such as blood
            MESHRENDERFLAG_FORBIDFOG = 0x0080
        }

        [Flags]
        public enum MeshOccludingFlag : uint
        {
            MESHOCCLUDINGFLAG_NORMAL = 0x09,
            MESHOCCLUDINGFLAG_SECTOR = 0x7D,
            MESHOCCLUDINGFLAG_WALL = 0x3D,       // mesh in sector (walls)
            MESHOCCLUDINGFLAG_PORTAL = 0x1D,     // mesh in portal
            MESHOCCLUDINGFLAG_INACTIVE = 0x11
        }

        public struct Material
        {
            public MaterialFlag flags;
            public Vector3 ambient;
            public Vector3 diffuse;  // only used if there is no diffuse texture, or if COLORED flag is set
            public Vector3 emission;  // always used
            public float transparency; // 0.0 - invisible; 1.0 - opaque

            // environment map
            public float envRatio; // parameter for interpolating between env. and diffuse map (only for NORMAL blending flag)
            public string envMapName;
            public string diffuseMapName;
            public string alphaMapName;

            // anim map
            public uint animSequenceLength;
            public ushort unk0;
            public uint framePeriod;
            public uint unk1;
            public uint unk2;
        }

        public struct Vertex
        {
            public Vector3 pos;
            public Vector3 normal;
            public Vector2 uv;
        }


        public struct Face
        {
            public ushort a;
            public ushort b;
            public ushort c;
        }

        public struct FaceGroup
        {
            public List<Face> faces;
            public uint materialID;      // 1-based, 0 = default material
        }

        public struct LOD
        {
            public float relativeDistance;
            public List<Vertex> vertices;
            public List<FaceGroup> faceGroups;
        }

        public struct Standard
        {
            public ushort instanced;
            public List<LOD> lods;
        }

        public struct Target
        {
            public ushort unk0;
            public List<ushort> targets;
        }

        public struct Bone
        {
            public Matrix4x4 transform;
            public uint boneID;
        }

        public struct Portal
        {
            public uint unk0; // always 4.
            public uint[] unk1; //6 values
            public List<Vector3> vertices;
        }

        public struct Sector
        {
            public uint unk0; // always 2049.
            public uint unk1; // always 0.
            public List<Vector3> vertices;
            public List<Face> faces;
            public Vector3 minBox;
            public Vector3 maxBox;
            public List<Portal> portals;
        }

        public struct Billboard
        {
            public Standard standard;
            public uint rotationAxis;  // 0 - X, 1 - Y, 2 - Z
            public bool ignoreCamera;  // 0 - rotate around center point, 1 - rotate around mRotationAxis
        }

        public struct Dummy
        {
            // bounding box
            public Vector3 minBox;
            public Vector3 maxBox;
        }

        public struct GlowData
        {
            public float position;
            public ushort materialID;
        }

        public struct Glow
        {
            public List<GlowData> glowData;
        }

        public struct Mirror
        {
            public Vector3 minBox;
            public Vector3 maxBox;
            public float[] unk0; // 4
            public Matrix4x4 reflectionMatrix;
            public Vector3 backgroundColor;
            public float viewDistance;
            public List<Vector3> vertices;
            public List<Face> faces;
        }

        public struct MorphLODVertex
        {
            public Vector3 position;
            public Vector3 normals;
        }

        public struct MorphLOD
        {
            public List<MorphLODVertex> vertices;
            public byte unk0;
            public List<ushort> vertexLinks; // addresses vertices from Standard's LOD mesh  
        }

        public struct Morph
        {
            public Standard standard;
            public byte frameCount;
            //public byte LODLevel;      // should be equal to Standard.LODLevel
            public byte unk0;
            public List<MorphLOD> LODs;
            public Vector3 minBox;
            public Vector3 maxBox;
            public float[] unk1; //4
        }

        public struct SingleMeshLODJoint
        {
            public Matrix4x4 transform;
            public uint oneWeightedVertCount; // amount of vertices that should have a weight of 1.0f
            public uint boneID; // this is likely a reference to a paired bone, which takes the remainder (1.0f - w) of weight
            public Vector3 minBox;
            public Vector3 maxBox;
            public List<float> weights;
        }

        public struct SingleMeshLOD
        {
            public uint nonWeightedVertCount;
            public Vector3 minBox;
            public Vector3 maxBox;
            public List<SingleMeshLODJoint> joints;
        }

        public struct SingleMesh
        {
            public Standard standard;
            public List<SingleMeshLOD> LODs; // LODLevel == Standard.LODLevel.
        }

        public struct SingleMorph
        {
            public SingleMesh singleMesh;
            public Morph morph;         // Morph without Standard Mesh!
        }

        public struct Mesh
        {
            public MeshType meshType;
            // standard mesh type
            public VisualMeshType visualMeshType;
            public MeshRenderFlag meshRenderFlags;
            public ushort parentID; // 0 - not connected
            public Vector3 pos;
            public Vector3 scale;
            public Quaternion rot;
            public MeshOccludingFlag cullingFlags;
            public string meshName;
            public string meshParams;
            public Standard standard;
            public Dummy dummy;
            public Mirror mirror;
            public Glow glow;
            public Billboard billboard;
            public Sector sector;
            public Target target;
            public Bone bone;
            public Morph morph;
            public SingleMesh singleMesh;
            public SingleMorph singleMorph;
        }

        public struct Model
        {
            public char[] signature; //4
            public ushort formatVersion; // PC : 0x1D (29)
            public ulong timestamp;
            public List<Material> materials;
            public List<Mesh> meshes;
            public bool use5DS;
        }

        public class Reader4DS : BaseLoader
        {
            void readMaterial(ref Model model, BinaryReader reader)
            {
                var matCount = reader.ReadUInt16();
                model.materials = new List<Material>();

                for (var i = 0; i < matCount; i++)
                {
                    Material mat = new Material();
                    mat.flags = (MaterialFlag)reader.ReadUInt32();
                    mat.ambient = ReadVector3(reader);
                    mat.diffuse = ReadVector3(reader);
                    mat.emission = ReadVector3(reader);
                    mat.transparency = reader.ReadSingle();


                    if ((mat.flags & MaterialFlag.MATERIALFLAG_ENVIRONMENTMAP) != 0)
                    {
                        mat.envRatio = reader.ReadSingle();
                        mat.envMapName = ReadString(reader);
                    }

                    mat.diffuseMapName = ReadString(reader);

                    if ((mat.flags & MaterialFlag.MATERIALFLAG_ALPHATEXTURE) != 0)
                    {
                        mat.alphaMapName = ReadString(reader);
                    }

                    if ((mat.flags & MaterialFlag.MATERIALFLAG_ANIMATEDTEXTUREDIFFUSE) != 0)
                    {
                        mat.animSequenceLength = reader.ReadUInt32();
                        mat.unk0 = reader.ReadUInt16();
                        mat.framePeriod = reader.ReadUInt32();
                        mat.unk1 = reader.ReadUInt32();
                        mat.unk2 = reader.ReadUInt32();
                    }

                    model.materials.Add(mat);
                }
            }

            LOD readLOD(BinaryReader reader)
            {
                LOD newLOD = new LOD();
                newLOD.relativeDistance = reader.ReadSingle();
                var vertexCount = reader.ReadUInt16();
                newLOD.vertices = new List<Vertex>();

                for (var i = 0; i < vertexCount; i++)
                {
                    Vertex newVertex = new Vertex();
                    newVertex.pos = ReadVector3(reader);
                    newVertex.normal = ReadVector3(reader);
                    newVertex.uv = ReadVector2(reader);

                    newLOD.vertices.Add(newVertex);
                }

                var faceGroupCount = reader.ReadByte();
                newLOD.faceGroups = new List<FaceGroup>();

                for (var i = 0; i < faceGroupCount; i++)
                {
                    FaceGroup newFaceGroup = new FaceGroup();
                    newFaceGroup.faces = new List<Face>();

                    var faceCount = reader.ReadUInt16();

                    for (var j = 0; j < faceCount; j++)
                    {
                        Face newFace = new Face();
                        newFace.a = reader.ReadUInt16();
                        newFace.b = reader.ReadUInt16();
                        newFace.c = reader.ReadUInt16();

                        newFaceGroup.faces.Add(newFace);
                    }

                    newFaceGroup.materialID = reader.ReadUInt16();
                    newLOD.faceGroups.Add(newFaceGroup);
                }

                return newLOD;
            }

            Standard readStandard(BinaryReader reader)
            {
                Standard newStandard = new Standard();
                newStandard.instanced = reader.ReadUInt16();
                newStandard.lods = new List<LOD>();

                if (newStandard.instanced == 0)
                {
                    var LODLevel = reader.ReadByte();

                    for (var i = 0; i < LODLevel; i++)
                        newStandard.lods.Add(readLOD(reader));

                }

                return newStandard;
            }

            Mirror readMirror(BinaryReader reader)
            {
                Mirror newMirror = new Mirror();
                newMirror.minBox = ReadVector3(reader);
                newMirror.maxBox = ReadVector3(reader);

                newMirror.unk0 = new float[4];

                for (var i = 0; i < 4; i++)
                    newMirror.unk0[i] = reader.ReadSingle();

                newMirror.reflectionMatrix = new Matrix4x4();

                /*
                 *  for (var y = 0; y < 4; y++)
                    for (var x = 0; x < 4; x++)
                        newMirror.reflectionMatrix[x, y] = reader.ReadSingle();
                 * */
                newMirror.reflectionMatrix = ReadMatrix(reader);

                newMirror.backgroundColor = ReadVector3(reader);
                newMirror.viewDistance = reader.ReadSingle();

                var vertexCount = reader.ReadUInt32();
                var faceCount = reader.ReadUInt32();

                newMirror.vertices = new List<Vector3>();
                newMirror.faces = new List<Face>();

                for (var i = 0; i < vertexCount; i++)
                {
                    newMirror.vertices.Add(ReadVector3(reader));
                }

                for (var i = 0; i < faceCount; i++)
                {
                    Face newFace = new Face();
                    newFace.a = reader.ReadUInt16();
                    newFace.b = reader.ReadUInt16();
                    newFace.c = reader.ReadUInt16();
                    newMirror.faces.Add(newFace);
                }

                return newMirror;
            }

            Glow readGlow(BinaryReader reader)
            {
                Glow newGlow = new Glow();
                var glowCount = reader.ReadByte();

                newGlow.glowData = new List<GlowData>();

                for (var i = 0; i < glowCount; i++)
                {
                    GlowData newGlowData = new GlowData();
                    newGlowData.position = reader.ReadSingle();
                    newGlowData.materialID = reader.ReadUInt16();

                    newGlow.glowData.Add(newGlowData);
                }

                return newGlow;
            }

            Portal readPortal(BinaryReader reader)
            {
                Portal newPortal = new Portal();
                var vertexCount = reader.ReadByte();
                newPortal.unk0 = reader.ReadUInt32();

                newPortal.unk1 = new uint[6];

                for (var i = 0; i < 6; i++)
                    newPortal.unk1[i] = reader.ReadUInt32();

                newPortal.vertices = new List<Vector3>();

                for (var i = 0; i < vertexCount; i++)
                {
                    newPortal.vertices.Add(ReadVector3(reader));
                }

                return newPortal;
            }

            Sector readSector(BinaryReader reader)
            {
                Sector newSector = new Sector();
                newSector.unk0 = reader.ReadUInt32();
                newSector.unk1 = reader.ReadUInt32();

                var vertexCount = reader.ReadUInt32();
                var faceCount = reader.ReadUInt32();

                newSector.vertices = new List<Vector3>();

                for (var i = 0; i < vertexCount; i++)
                {
                    newSector.vertices.Add(ReadVector3(reader));
                }

                newSector.faces = new List<Face>();

                for (var i = 0; i < faceCount; i++)
                {
                    Face newFace = new Face();
                    newFace.a = reader.ReadUInt16();
                    newFace.b = reader.ReadUInt16();
                    newFace.c = reader.ReadUInt16();
                    newSector.faces.Add(newFace);
                }

                newSector.minBox = ReadVector3(reader);
                newSector.maxBox = ReadVector3(reader);

                var portalCount = reader.ReadByte();
                newSector.portals = new List<Portal>();

                for (var i = 0; i < portalCount; i++)
                    newSector.portals.Add(readPortal(reader));


                return newSector;
            }

            Target readTarget(BinaryReader reader)
            {
                Target newTarget = new Target();
                newTarget.unk0 = reader.ReadUInt16();
                var targetCount = reader.ReadByte();

                newTarget.targets = new List<ushort>();

                for (var i = 0; i < targetCount; i++)
                    newTarget.targets.Add(reader.ReadUInt16());


                return newTarget;
            }

            Morph readMorph(BinaryReader reader, bool ignoreStandard)
            {
                Morph newMorph = new Morph();

                // NOTE(zaklaus): Single Morph contains Standard Mesh in Single Mesh already.
                if (!ignoreStandard)
                {
                    newMorph.standard = readStandard(reader);
                }
                // NOTE(zaklaus): ELSE ignore Standard Mesh, since Single Mesh has it.
                newMorph.frameCount = reader.ReadByte();
                if (newMorph.frameCount > 0)
                {
                    var LODLevel = reader.ReadByte();
                    newMorph.unk0 = reader.ReadByte();

                    newMorph.LODs = new List<MorphLOD>();

                    for (var i = 0; i < LODLevel; i++)
                    {
                        MorphLOD newMorphLOD = new MorphLOD();
                        var vertexCount = reader.ReadUInt16();

                        newMorphLOD.vertices = new List<MorphLODVertex>();

                        for (var j = 0; j < newMorph.frameCount * vertexCount; j++)
                        {
                            MorphLODVertex newVertex = new MorphLODVertex();
                            newVertex.normals = ReadVector3(reader);
                            newVertex.position = ReadVector3(reader);

                            newMorphLOD.vertices.Add(newVertex);
                        }

                        if (newMorph.frameCount * vertexCount > 0)
                            newMorphLOD.unk0 = reader.ReadByte();

                        newMorphLOD.vertexLinks = new List<ushort>();

                        for (var j = 0; j < vertexCount; j++)
                            newMorphLOD.vertexLinks.Add(reader.ReadUInt16());

                        newMorph.LODs.Add(newMorphLOD);
                    }

                    newMorph.minBox = ReadVector3(reader);
                    newMorph.maxBox = ReadVector3(reader);

                    newMorph.unk1 = new float[4];
                    for (var i = 0; i < 4; i++)
                        newMorph.unk1[i] = reader.ReadSingle();
                }

                return newMorph;
            }

            SingleMeshLODJoint readSingleMeshLodJoint(BinaryReader reader)
            {
                SingleMeshLODJoint newSingleMeshJoint = new SingleMeshLODJoint();
                newSingleMeshJoint.transform = ReadMatrix(reader);
                newSingleMeshJoint.oneWeightedVertCount = reader.ReadUInt32();
                var weightCount = reader.ReadUInt32();
                newSingleMeshJoint.boneID = reader.ReadUInt32();
                newSingleMeshJoint.minBox = ReadVector3(reader);
                newSingleMeshJoint.maxBox = ReadVector3(reader);

                newSingleMeshJoint.weights = new List<float>();

                for (var i = 0; i < weightCount; i++)
                    newSingleMeshJoint.weights.Add(reader.ReadSingle());

                return newSingleMeshJoint;
            }

            SingleMeshLOD readSingleMeshLOD(BinaryReader reader)
            {
                SingleMeshLOD newLOD = new SingleMeshLOD();

                // Every LOD's vertext buffer is sorted in the following order:
                // - non-weighted vertices
                // - BONE0's fully-weighted vertices (1.0f weight)
                // - BONE0's weighted vertices
                // - BONE1's fully-weighted vertices (1.0f weight)
                // - BONE1's weighted vertices
                // and so on
                var jointCount = reader.ReadByte();
                newLOD.nonWeightedVertCount = reader.ReadUInt32();
                newLOD.minBox = ReadVector3(reader);
                newLOD.maxBox = ReadVector3(reader);

                newLOD.joints = new List<SingleMeshLODJoint>();

                for (var i = 0; i < jointCount; i++)
                    newLOD.joints.Add(readSingleMeshLodJoint(reader));

                return newLOD;
            }

            SingleMesh readSingleMesh(BinaryReader reader)
            {
                SingleMesh newMesh = new SingleMesh();
                newMesh.standard = readStandard(reader);

                newMesh.LODs = new List<SingleMeshLOD>();

                for (var i = 0; i < newMesh.standard.lods.Count; i++)
                    newMesh.LODs.Add(readSingleMeshLOD(reader));

                return newMesh;
            }

            SingleMorph readSingleMorph(BinaryReader reader)
            {
                SingleMorph newMorph = new SingleMorph();
                newMorph.singleMesh = readSingleMesh(reader);
                newMorph.morph = readMorph(reader, true);
                return newMorph;
            }

            void readMesh(ref Model model, BinaryReader reader)
            {
                var meshCount = reader.ReadUInt16();
                model.meshes = new List<Mesh>();

                for (var i = 0; i < meshCount; i++)
                {
                    Mesh newMesh = new Mesh();
                    newMesh.meshType = (MeshType)reader.ReadByte();

                    if (newMesh.meshType == MeshType.MESHTYPE_STANDARD)
                    {
                        newMesh.visualMeshType = (VisualMeshType)reader.ReadByte();
                        newMesh.meshRenderFlags = (MeshRenderFlag)reader.ReadUInt16();
                    }

                    newMesh.parentID = reader.ReadUInt16();
                    newMesh.pos = ReadVector3(reader);
                    newMesh.scale = ReadVector3(reader);
                    newMesh.rot = ReadQuat(reader);

                    var rot = newMesh.rot;
                    var tmpRot = new Quaternion(rot.y, rot.z, rot.w, -1 * rot.x);
                    newMesh.rot = tmpRot;

                    newMesh.cullingFlags = (MeshOccludingFlag)reader.ReadByte();

                    newMesh.meshName = ReadString(reader);
                    newMesh.meshParams = ReadString(reader);

                    

                    switch (newMesh.meshType)
                    {
                        case MeshType.MESHTYPE_STANDARD:
                            {
                                switch (newMesh.visualMeshType)
                                {
                                    case VisualMeshType.VISUALMESHTYPE_STANDARD:
                                        {
                                            newMesh.standard = readStandard(reader);
                                        }
                                        break;

                                    case VisualMeshType.VISUALMESHTYPE_MIRROR:
                                        {
                                            newMesh.mirror = readMirror(reader);
                                        }
                                        break;

                                    case VisualMeshType.VISUALMESHTYPE_GLOW:
                                        {
                                            newMesh.glow = readGlow(reader);
                                        }
                                        break;

                                    case VisualMeshType.VISUALMESHTYPE_BILLBOARD:
                                        {
                                            Billboard newBillboard = new Billboard();
                                            newBillboard.standard = readStandard(reader);
                                            newBillboard.rotationAxis = reader.ReadUInt32();
                                            newBillboard.ignoreCamera = reader.ReadByte() > 0;
                                            newMesh.billboard = newBillboard;
                                        }
                                        break;

                                    case VisualMeshType.VISUALMESHTYPE_MORPH:
                                        {
                                            newMesh.morph = readMorph(reader, false);
                                        }
                                        break;

                                    case VisualMeshType.VISUALMESHTYPE_SINGLEMESH:
                                        {
                                            newMesh.singleMesh = readSingleMesh(reader);
                                        }
                                        break;

                                    case VisualMeshType.VISUALMESHTYPE_SINGLEMORPH:
                                        {
                                            newMesh.singleMorph = readSingleMorph(reader);
                                        }
                                        break;

                                    default:
                                        {
                                        }
                                        break;
                                }
                            }
                            break;

                        case MeshType.MESHTYPE_DUMMY:
                            {
                                Dummy newDummy = new Dummy();
                                newDummy.minBox = ReadVector3(reader);
                                newDummy.maxBox = ReadVector3(reader);
                                newMesh.dummy = newDummy;
                            }
                            break;

                        case MeshType.MESHTYPE_SECTOR:
                            {
                                newMesh.sector = readSector(reader);
                            }
                            break;

                        case MeshType.MESHTYPE_TARGET:
                            {
                                newMesh.target = readTarget(reader);
                            }
                            break;

                        case MeshType.MESHTYPE_BONE:
                            {
                                Bone newBone = new Bone();
                                newBone.transform = ReadMatrix(reader);
                                newBone.boneID = reader.ReadUInt32();
                                newMesh.bone = newBone;
                            }
                            break;

                        default:
                            {
                            }
                            break;
                    }

                    // NOTE(zaklaus): Check whether this is a collision mesh.
                    // happens AFTER we load the required content to skip it.
                    string meshName = newMesh.meshName;
                    if (meshName.Contains("wcol"))
                    {
                        newMesh.meshType = MeshType.MESHTYPE_COLLISION;
                    }

                    model.meshes.Add(newMesh);
                }
            }

            public Model loadModel(BinaryReader reader)
            {
                Model newModel = new Model();
                newModel.signature = new char[4];
                for (var i = 0; i < 4; i++)
                    newModel.signature[i] = reader.ReadChar();

                string sig = new string(newModel.signature);
                if (!sig.Contains("4DS"))
                {
                    Debug.Log("Not a valid 4DS model!");
                    return newModel;
                }

                newModel.formatVersion = reader.ReadUInt16();
                newModel.timestamp = reader.ReadUInt64();

                readMaterial(ref newModel, reader);
                readMesh(ref newModel, reader);
                newModel.use5DS = reader.ReadByte() > 0;

                return newModel;
            }
        }
    }
}