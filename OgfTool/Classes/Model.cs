using Aspose.ThreeD.Entities;
using Aspose.ThreeD.Utilities;
using Aspose.ThreeD.Formats;
using Aspose.ThreeD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Aspose.ThreeD.Shading;

namespace OgfTool
{
    public enum OGF
    {
        OGF_HEADER = 1,
        OGF_TEXTURE = 2,
        OGF_S_BONE_NAMES = 13,  // * For skeletons only
        OGF_S_MOTIONS = 14, // * For skeletons only

        //build 729
        OGF2_TEXTURE = 2,
        OGF2_TEXTURE_L = 3,
        OGF2_BBOX = 6,
        OGF2_VERTICES = 7,
        OGF2_INDICES = 8,
        OGF2_VCONTAINER = 11,
        OGF2_BSPHERE = 12,

        OGF3_TEXTURE_L = 3,
        OGF3_CHILD_REFS = 5,
        OGF3_BBOX = 6,
        OGF3_VERTICES = 7,
        OGF3_INDICES = 8,
        OGF3_LODDATA = 9, // not sure
        OGF3_VCONTAINER = 10,
        OGF3_BSPHERE = 11,
        OGF3_CHILDREN_L = 12,
        OGF3_DPATCH = 15,  // guessed name
        OGF3_LODS = 16,   // guessed name
        OGF3_CHILDREN = 17,
        OGF3_S_SMPARAMS = 18,// build 1469
        OGF3_ICONTAINER = 19,// build 1865
        OGF3_S_SMPARAMS_NEW = 20,// build 1472 - 1865
        OGF3_LODDEF2 = 21,// build 1865
        OGF3_TREEDEF2 = 22,// build 1865
        OGF3_S_IKDATA_0 = 23,// build 1475 - 1580
        OGF3_S_USERDATA = 24,// build 1537 - 1865
        OGF3_S_IKDATA = 25,// build 1616 - 1829, 1844
        OGF3_S_MOTIONS_NEW = 26,// build 1616 - 1865
        OGF3_S_DESC = 27,// build 1844
        OGF3_S_IKDATA_2 = 28,// build 1842 - 1865
        OGF3_S_MOTION_REFS = 29,// build 1842

        OGF4_VERTICES = 3,
        OGF4_INDICES = 4,
        OGF4_P_MAP = 5,  //---------------------- unused
        OGF4_SWIDATA = 6,
        OGF4_VCONTAINER = 7, // not used ??
        OGF4_ICONTAINER = 8, // not used ??
        OGF4_CHILDREN = 9,   // * For skeletons only
        OGF4_CHILDREN_L = 10,    // Link to child visuals
        OGF4_LODDEF2 = 11,   // + 5 channel data
        OGF4_TREEDEF2 = 12,  // + 5 channel data
        OGF4_S_SMPARAMS = 15,    // * For skeletons only
        OGF4_S_IKDATA = 16,  // * For skeletons only
        OGF4_S_USERDATA = 17,    // * For skeletons only (Ini-file)
        OGF4_S_DESC = 18,    // * For skeletons only
        OGF4_S_MOTION_REFS = 19, // * For skeletons only
        OGF4_SWICONTAINER = 20,  // * SlidingWindowItem record container
        OGF4_GCONTAINER = 21,    // * both VB&IB
        OGF4_FASTPATH = 22,  // * extended/fast geometry
        OGF4_S_LODS = 23,    // * For skeletons only (Ini-file)
        OGF4_S_MOTION_REFS2 = 24,    // * changes in format
        OGF4_COLLISION_VERTICES = 25,
        OGF4_COLLISION_INDICES = 26, 
    };

    public enum MTL
    {
        GAMEMTL_CURRENT_VERSION = 0x0001,
        GAMEMTLS_CHUNK_VERSION = 0x1000,
        GAMEMTLS_CHUNK_AUTOINC = 0x1001,
        GAMEMTLS_CHUNK_MTLS = 0x1002,
        GAMEMTLS_CHUNK_MTLS_PAIR = 0x1003,
        GAMEMTL_CHUNK_MAIN = 0x1000,
        GAMEMTL_CHUNK_FLAGS = 0x1001,
        GAMEMTL_CHUNK_PHYSICS = 0x1002,
        GAMEMTL_CHUNK_FACTORS = 0x1003,
        GAMEMTL_CHUNK_FLOTATION = 0x1004,
        GAMEMTL_CHUNK_DESC = 0x1005,
        GAMEMTL_CHUNK_INJURIOUS = 0x1006,
        GAMEMTL_CHUNK_DENSITY = 0x1007,
        GAMEMTL_CHUNK_FACTORS_MP = 0x1008,
        GAMEMTLPAIR_CHUNK_PAIR = 0x1000,
        GAMEMTLPAIR_CHUNK_BREAKING = 0x1002,
        GAMEMTLPAIR_CHUNK_STEP = 0x1003,
        GAMEMTLPAIR_CHUNK_COLLIDE = 0x1005
    }

    public enum MotionKeyFlags
    {
        flTKeyPresent = (1<<0),
        flRKeyAbsent  = (1<<1),
        flTKey16IsBit = (1<<2),
        flTKeyFFT_Bit = (1<<3),
    };

    public enum ModelType
    {
        MT3_NORMAL = 0, // Fvisual
        MT3_HIERRARHY = 1,    // FHierrarhyVisual
        MT3_PROGRESSIVE = 2,  // FProgressiveFixedVisual
        MT3_SKELETON_GEOMDEF_PM = 3,  // CSkeletonX_PM
        MT3_SKELETON_ANIM = 4,    // CKinematics
        MT3_DETAIL_PATCH = 6, // FDetailPatch
        MT3_SKELETON_GEOMDEF_ST = 7,  // CSkeletonX_ST
        MT3_CACHED = 8,   // FCached
        MT3_PARTICLE = 9, // CPSVisual
        MT3_PROGRESSIVE2 = 10, // FProgressive
        MT3_LOD = 11,  // FLOD build 1472 - 1865
        MT3_TREE = 12, // FTreeVisual build 1472 - 1865
                        //				= 0xd,	// CParticleEffect 1844
                        //				= 0xe,	// CParticleGroup 1844
        MT3_SKELETON_RIGID = 15,   // CSkeletonRigid 1844

        MT4_NORMAL = 0, // Fvisual
        MT4_HIERRARHY = 1,    // FHierrarhyVisual
        MT4_PROGRESSIVE = 2,  // FProgressive
        MT4_SKELETON_ANIM = 3,    // CKinematicsAnimated
        MT4_SKELETON_GEOMDEF_PM = 4,  // CSkeletonX_PM
        MT4_SKELETON_GEOMDEF_ST = 5,  // CSkeletonX_ST
        MT4_LOD = 6,  // FLOD
        MT4_TREE_ST = 7,  // FTreeVisual_ST
        MT4_PARTICLE_EFFECT = 8,  // PS::CParticleEffect
        MT4_PARTICLE_GROUP = 9,   // PS::CParticleGroup
        MT4_SKELETON_RIGID = 10,   // CKinematics
        MT4_TREE_PM = 11,  // FTreeVisual_PM

        MT4_OMF = 64, // fake model Type to distinguish .omf
    };

    public class SSkelVert
    {
        public float[] Uv { get; set; }
        public float[] Offs { get; set; }
        public float[] norm { get; set; }
        public float[] tang { get; set; }
        public float[] binorm { get; set; }

        public uint[] bones_id { get; set; }
        public float[] bones_infl { get; set; }

        public float[] local_offset { get; set; }
        public float[] local_rotation { get; set; }
        public float[] local_offset2 { get; set; }
        public float[] local_rotation2 { get; set; }

        public float[] center { get; set; }
        public bool rotation_local { get; set; }

        public SSkelVert()
        {
            Uv = new float[2] { 0.0f, 0.0f };
            Offs = new float[3] { 0.0f, 0.0f, 0.0f };
            norm = new float[3] { 0.0f, 0.0f, 0.0f };
            tang = new float[3] { 0.0f, 0.0f, 0.0f };
            binorm = new float[3] { 0.0f, 0.0f, 0.0f };
            bones_id = new uint[4] { 0, 0, 0, 0 };
            bones_infl = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
            local_offset = new float[3] { 0.0f, 0.0f, 0.0f };
            local_rotation = new float[3] { 0.0f, 0.0f, 0.0f };
            center = new float[3] { 0.0f, 0.0f, 0.0f };
            local_offset2 = new float[3] { 0.0f, 0.0f, 0.0f };
            local_rotation2 = new float[3] { 0.0f, 0.0f, 0.0f };
            rotation_local = false;
        }

        public float[] Offset()
        {
            float[] offset = FVec.Add(Offs, local_offset);
            offset = FVec.Add(offset, local_offset2);
            offset = FVec.RotateXYZ(offset, local_rotation2);
            return FVec.RotateXYZ(offset, local_rotation, rotation_local ? center : new float[3]);
        }

        public float[] Offset2()
        {
            float[] offset = FVec.Add(Offs, local_offset);
            offset = FVec.RotateXYZ(offset, local_rotation2);
            return FVec.RotateXYZ(offset, local_rotation, rotation_local ? center : new float[3]);
        }

        public float[] Norm()
        {
            float[] vec = FVec.RotateXYZ(norm, local_rotation2);
            return FVec.RotateXYZ(vec, local_rotation);
        }

        public float[] Tang()
        {
            float[] vec = FVec.RotateXYZ(tang, local_rotation2);
            return FVec.RotateXYZ(vec, local_rotation);
        }

        public float[] Binorm()
        {
            float[] vec = FVec.RotateXYZ(binorm, local_rotation2);
            return FVec.RotateXYZ(vec, local_rotation);
        }

        public static void GenerateNormals(List<SSkelVert> Vertices, List<SSkelFace> Faces, bool generate_normal = true)
        {
            foreach (var vertex in Vertices)
            {
                if (generate_normal)
                {
                    vertex.norm = new float[3] { 0.0f, 0.0f, 0.0f };
                }

                vertex.tang = new float[3] { 0.0f, 0.0f, 0.0f };
                vertex.binorm = new float[3] { 0.0f, 0.0f, 0.0f };
            }

            foreach (var face in Faces)
            {
                int ia = face.Vertex[0];
                int ib = face.Vertex[1];
                int ic = face.Vertex[2];

                float[] dv1 = FVec.Sub(Vertices[ia].Offs, Vertices[ib].Offs);
                float[] dv2 = FVec.Sub(Vertices[ic].Offs, Vertices[ib].Offs);
                float[] duv1 = FVec2.Sub(Vertices[ia].Uv, Vertices[ib].Uv);
                float[] duv2 = FVec2.Sub(Vertices[ic].Uv, Vertices[ib].Uv);

                float r = 1.0f / (duv1[0] * duv2[1] - duv1[1] * duv2[0]);
                float[] tangent = FVec.Mul(FVec.Sub(FVec.Mul(dv1, duv2[1]), FVec.Mul(dv2, duv1[1])), r);
                float[] binormal = FVec.Mul(FVec.Sub(FVec.Mul(dv2, duv1[0]), FVec.Mul(dv1, duv2[0])), r);

                if (generate_normal)
                {
                    float[] normal = FVec.CrossProduct(dv1, dv2);
                    Vertices[ia].norm = FVec.Add(Vertices[ia].norm, normal);
                    Vertices[ib].norm = FVec.Add(Vertices[ib].norm, normal);
                    Vertices[ic].norm = FVec.Add(Vertices[ic].norm, normal);
                }

                Vertices[ia].tang = FVec.Add(Vertices[ia].tang, tangent);
                Vertices[ib].tang = FVec.Add(Vertices[ib].tang, tangent);
                Vertices[ic].tang = FVec.Add(Vertices[ic].tang, tangent);

                Vertices[ia].binorm = FVec.Add(Vertices[ia].binorm, binormal);
                Vertices[ib].binorm = FVec.Add(Vertices[ib].binorm, binormal);
                Vertices[ic].binorm = FVec.Add(Vertices[ic].binorm, binormal);
            }

            foreach (var vertex in Vertices)
            {
                if (generate_normal)
                {
                    vertex.norm = FVec.Normalize(vertex.norm);
                    vertex.norm = FVec.Mul(vertex.norm, -1.0f);
                }
                vertex.tang = FVec.Normalize(vertex.tang);
                vertex.binorm = FVec.Normalize(vertex.binorm);

                if (FVec.IsNan(vertex.tang))
                    vertex.tang = new float[3] { 0.0f, 0.0f, 0.0f };

                if (FVec.IsNan(vertex.binorm))
                    vertex.binorm = new float[3] { 0.0f, 0.0f, 0.0f };
            }
        }
    };

    public class SSkelFace
    {
        public int[] Vertex { get; private set; }
        public SSkelFace()
        {
            Vertex = new int[3] { 0, 0, 0 };
        }
    };

    public class VIPM_SWR
    {
        public uint Offset { get; set; }
        public ushort NumTris { get; set; }
        public ushort NumVerts { get; set; }

        public VIPM_SWR()
        {
            Offset = 0;
            NumTris = 0;
            NumVerts = 0;
        }
    };

    public class XRayModel
    {
        public enum ModelFormat
        {
            eUnknown = 0,
            eOGF = 1,
            eDM = 2,
            eDetail = 4,
            eObj = 8
        }

        [DllImport("Converter.dll")]
        private static extern void CalcBones([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)] ref BoneRenderTransform[] bones, int length, string child_list);

        [DllImport("Converter.dll")]
        private static extern void FixBonesBind([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)] ref BoneRenderTransform[] bones, int length, string child_list);

        [DllImport("Converter.dll")]
        private static extern void FixVertexOffset([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)] ref BoneRenderTransform[] bones, int length, string child_list, int bone_0, float x, float y, float z);

        [DllImport("Converter.dll")]
        private static extern int StartConvert(string path, string out_path, int mode, int convert_to_mode, string motion_list);

        private int RunConverter(string path, string out_path, int mode, int convert_to_mode)
        {
            string dll_path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\converter.dll";
            if (File.Exists(dll_path))
                return StartConvert(path, out_path, mode, convert_to_mode, "");
            else
            {
                MessageBox.Show("Can't find Converter.dll", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        delegate void WriteSceneMesh(List<SSkelVert> Verts, List<SSkelFace> Faces, string texture, bool allow_texture);
        delegate void AddChild();

        private int format;
        public uint BrokenType { get; set; }
        public bool IsCopModel { get; set; }

        public OgfHeader Header { get; set; }
        public Description Description { get; set; }
        public List<OgfChild> Childs { get; set; }
        public BoneData BoneData { get; set; }
        public IKData IkData { get; set; }
        public UserData UserData { get; set; }
        public Lod Lod { get; set; }
        public MotionRefs MotionRefs { get; set; }
        public OMF Motions { get; set; }

        public uint ChunkSize { get; set; }
        public long Pos { get; private set; }

        public float[] LocalOffset { get; set; }
        public float[] LocalRotation { get; set; }

        public byte[] SourceData { get; set; }
        public bool Opened { get; set; }
        public string FileName { get; set; }

        public XRayModel()
        {
            Invalidate();
            Opened = false;
        }

        public void Copy(XRayModel model)
        {
            Pos = model.Pos;
            ChunkSize = model.ChunkSize;
            BrokenType = model.BrokenType;
            Motions = model.Motions;
            Description = model.Description;
            Childs = model.Childs;
            BoneData = model.BoneData;
            IkData = model.IkData;
            UserData = model.UserData;
            Lod = model.Lod;
            MotionRefs = model.MotionRefs;
            IsCopModel = model.IsCopModel;
            Header = model.Header;
            SourceData = model.SourceData;

            LocalOffset = model.LocalOffset;
            LocalRotation = model.LocalRotation;
            format = model.format;
        }

        public void Invalidate()
        {
            Pos = 0;
            ChunkSize = 0;
            BrokenType = 0;
            Motions = new OMF();
            Description = null;
            Childs = new List<OgfChild>();
            BoneData = null;
            IkData = null;
            UserData = null;
            Lod = null;
            MotionRefs = null;
            IsCopModel = false;
            Header = new OgfHeader();
            SourceData = null;

            LocalOffset = new float[3];
            LocalRotation = new float[3];
            format = (int)ModelFormat.eUnknown;
        }

        public void Destroy()
        {
        }

        public bool IsProgressive()
        {
            foreach (var child in Childs)
            {
                if (child.Header != null && child.Header.IsProgressive())
                    return true;
            }
            return false;
        }

        public void RemoveProgressive(float lod)
        {
            for (int idx = 0; idx < Childs.Count; idx++)
            {
                if (Header.IsSkeleton())
                    Childs[idx].Header.GeomdefST();
                else
                    Childs[idx].Header.Normal();
                Childs[idx].Faces = Childs[idx].Faces_SWI(lod);
                Childs[idx].SWI.Clear();
            }
        }

        public void CalcBonesTransform()
        {
            if (BoneData == null || IkData == null)
            {
                return;
            }

            BoneRenderTransform[] transforms = BoneRenderTransform.Setup(this, out string child_list);
            CalcBones(ref transforms, transforms.Length, child_list);

            for (int i = 0; i < BoneData.Bones.Count; i++)
            {
                IkData.Bones[i].RenderTransform = transforms[i].OutPos();
            }
        }

        public void FixOldBonesBind()
        {
            if (BoneData != null && IkData != null && IkData.ChunkVersion == 2)
            {
                byte old_ver = IkData.ChunkVersion;
                IkData.ChunkVersion = 0;
                BoneRenderTransform[] transforms = BoneRenderTransform.Setup(this, out string child_list);
                IkData.ChunkVersion = old_ver;

                FixBonesBind(ref transforms, transforms.Length, child_list);

                for (int i = 0; i < BoneData.Bones.Count; i++)
                {
                    IkData.Bones[i].FixedPosition = transforms[i].OutPos();
                    IkData.Bones[i].FixedRotation = transforms[i].OutRot();
                }
            }
        }

        public float[] FixOldVertexOffset(SSkelVert vert)
        {
            BoneRenderTransform[] transforms = BoneRenderTransform.Setup(this, out string child_list);
            FixVertexOffset(ref transforms, transforms.Length, child_list, (int)vert.bones_id[0], vert.Offset()[0], vert.Offset()[1], vert.Offset()[2]);

            return new float[3] { transforms[0].OutPosX, transforms[0].OutPosY, transforms[0].OutPosZ };
        }

        public bool Is(ModelFormat fmt)
        {
            return BitMask.IsSet(format, (int)fmt);
        }

        public bool OpenFile(string filename, bool silent = false)
        {
            if (OpenFileInternal(filename, silent))
            { 
                Opened = true;
                FileName = filename;
                return true;
            }

            return false;
        }

        private bool LoadOGF(string filename, bool silent = false)
        {
            using (var xr_loader = new XRayLoader())
            {
                XRayModel model = new XRayModel
                {
                    SourceData = File.ReadAllBytes(filename)
                };

                using (var r = new BinaryReader(new MemoryStream(model.SourceData)))
                {
                    xr_loader.SetStream(r.BaseStream);

                    if (!xr_loader.find_chunk((int)OGF.OGF_HEADER, false, true))
                    {
                        if (!silent)
                            MessageBox.Show("Unsupported OGF format! Can't find header chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    else
                    {
                        model.Header.Load(xr_loader);

                        if (model.Header.FormatVersion < 3)
                        {
                            if (!silent)
                                MessageBox.Show($"Unsupported OGF version: {model.Header.FormatVersion}!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }

                    int DescChunk = (model.Header.FormatVersion == 4 ? (int)OGF.OGF4_S_DESC : (int)OGF.OGF3_S_DESC);
                    uint DescriptionSize = xr_loader.find_chunkSize(DescChunk, false, true);
                    if (DescriptionSize > 0)
                    {
                        model.Description = new Description();
                        model.BrokenType = model.Description.Load(xr_loader, DescriptionSize);
                    }

                    int ChildChunk = (model.Header.FormatVersion == 4 ? (int)OGF.OGF4_CHILDREN : (int)OGF.OGF3_CHILDREN);
                    bool bFindChunk = xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(ChildChunk, false, true));

                    model.Pos = xr_loader.ChunkPos;

                    int id = 0;

                    // Childs
                    if (bFindChunk)
                    {
                        while (true)
                        {
                            if (!xr_loader.find_chunk(id)) break;

                            Stream temp = xr_loader.Reader.BaseStream;

                            if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

                            OgfChild Child = new OgfChild();
                            if (!Child.Load(xr_loader))
                                break;

                            model.Childs.Add(Child);

                            id++;
                            xr_loader.SetStream(temp);
                        }

                        xr_loader.SetStream(r.BaseStream);
                    }
                    else
                    {
                        OgfChild Child = new OgfChild();
                        if (Child.Load(xr_loader))
                            model.Childs.Add(Child);
                    }

                    if (model.Childs.Count == 0)
                    {
                        if (!silent)
                            MessageBox.Show("Unsupported OGF format! Can't find children chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (model.Header.IsSkeleton())
                    {
                        // Bones
                        if (!xr_loader.find_chunk((int)OGF.OGF_S_BONE_NAMES, false, true))
                        {
                            if (!silent)
                                MessageBox.Show("Unsupported OGF format! Can't find bones chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        else
                        {
                            if (xr_loader.ChunkPos < model.Pos)
                                model.BrokenType = 2;

                            model.BoneData = new BoneData();
                            model.BoneData.Load(xr_loader);
                        }

                        // Ik Data
                        byte IKDataVers = 0;

                        int IKDataChunkRelease = (model.Header.FormatVersion == 4 ? (int)OGF.OGF4_S_IKDATA : (int)OGF.OGF3_S_IKDATA_2);
                        bool IKDataChunkFind = xr_loader.find_chunk(IKDataChunkRelease, false, true);

                        if (IKDataChunkFind) // Load Release chunk
                            IKDataVers = 4;
                        else
                        {
                            IKDataChunkFind = model.Header.FormatVersion == 3 && xr_loader.find_chunk((int)OGF.OGF3_S_IKDATA, false, true);

                            if (IKDataChunkFind) // Load Pre Release chunk
                                IKDataVers = 3;
                            else
                            {
                                IKDataChunkFind = model.Header.FormatVersion == 3 && xr_loader.find_chunk((int)OGF.OGF3_S_IKDATA_0, false, true);

                                if (IKDataChunkFind) // Load Builds chunk
                                    IKDataVers = 2;
                            }
                        }

                        if (IKDataVers != 0)
                        {
                            model.IkData = new IKData();
                            model.IkData.Load(xr_loader, model.BoneData.Bones.Count, IKDataVers);

                            model.FixOldBonesBind();
                            model.CalcBonesTransform();
                        }
                        else if (model.Header.FormatVersion == 4) // Chunk not find, exit if Release OGF
                        {
                            if (!silent)
                                MessageBox.Show("Unsupported OGF format! Can't find ik data chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        // Userdata
                        int UserDataChunk = (model.Header.FormatVersion == 4 ? (int)OGF.OGF4_S_USERDATA : (int)OGF.OGF3_S_USERDATA);
                        uint UserDataSize = xr_loader.find_chunkSize(UserDataChunk, false, true);
                        if (UserDataSize > 0)
                        {
                            model.UserData = new UserData();
                            model.UserData.Load(xr_loader, UserDataSize);
                        }

                        // Lod ref
                        if (model.Header.FormatVersion == 4 && xr_loader.find_chunk((int)OGF.OGF4_S_LODS, false, true))
                        {
                            model.Lod = new Lod();
                            model.Lod.Load(xr_loader);
                        }

                        // Motion Refs
                        int RefsChunk = (model.Header.FormatVersion == 4 ? (int)OGF.OGF4_S_MOTION_REFS : (int)OGF.OGF3_S_MOTION_REFS);
                        bool StringRefs = xr_loader.find_chunk(RefsChunk, false, true);

                        if (StringRefs || model.Header.FormatVersion == 4 && xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS2, false, true))
                        {
                            model.MotionRefs = new MotionRefs();
                            model.MotionRefs.Load(xr_loader, StringRefs);
                        }

                        //Motions
                        if (xr_loader.find_chunk((int)OGF.OGF_S_MOTIONS, false, true))
                        {
                            xr_loader.Reader.BaseStream.Position -= 8;
                            byte[] OMF = xr_loader.ReadBytes((int)xr_loader.Reader.BaseStream.Length - (int)xr_loader.Reader.BaseStream.Position);
                            model.Motions.SetData(OMF);
                        }
                    }
                }

                BitMask.Null(ref model.format);
                BitMask.Set(ref model.format, (int)ModelFormat.eOGF);

                Copy(model);
            }

            return true;
        }

        private bool LoadFromScene(string filename, LoadOptions options)
        {
            Invalidate();
            SourceData = File.ReadAllBytes(filename);
            Description = new Description();

            Scene scene = new Scene();
            scene.Open(filename, options);

            foreach (Node meshNode in scene.RootNode.ChildNodes)
            {
                OgfChild child = new OgfChild();
                child.Header = new OgfHeader();

                if (meshNode.Material != null)
                    child.Texture = meshNode.Material.Name;
                else
                    child.Texture = "default";
                child.Shader = "default";

                Mesh mesh = meshNode.Entity as Mesh;
                mesh = PolygonModifier.Triangulate(mesh);
                for (int v = 0; v < mesh.ControlPoints.Count; v++)
                {
                    SSkelVert vert = new SSkelVert();
                    vert.Offs = new float[3] { (float)mesh.ControlPoints[v].x, (float)mesh.ControlPoints[v].y, (float)mesh.ControlPoints[v].z };
                    if (mesh.GetElement(VertexElementType.Normal) is VertexElementNormal Normals)
                        vert.norm = new float[3] { (float)Normals.Data[v].x, (float)Normals.Data[v].y, (float)Normals.Data[v].z };
                    if (mesh.GetElement(VertexElementType.Tangent) is VertexElementTangent Tangents)
                        vert.tang = new float[3] { (float)Tangents.Data[v].x, (float)Tangents.Data[v].y, (float)Tangents.Data[v].z };
                    if (mesh.GetElement(VertexElementType.Binormal) is VertexElementBinormal Binormals)
                        vert.binorm = new float[3] { (float)Binormals.Data[v].x, (float)Binormals.Data[v].y, (float)Binormals.Data[v].z };
                    if (mesh.GetElement(VertexElementType.UV) is VertexElementUV UVs)
                        vert.Uv = new float[2] { (float)UVs.Data[v].x, Math.Abs(1.0f - (float)UVs.Data[v].y) };

                    child.Vertices.Add(vert);
                }

                for (int f = 0; f < mesh.PolygonCount; f++)
                {
                    SSkelFace face = new SSkelFace();
                    face.Vertex[0] = mesh.Polygons[f][0];
                    face.Vertex[1] = mesh.Polygons[f][1];
                    face.Vertex[2] = mesh.Polygons[f][2];
                    child.Faces.Add(face);
                }

                Childs.Add(child);
            }

            RecalcBBox(true);
            BitMask.Null(ref format);
            BitMask.Set(ref format, (int)ModelFormat.eObj);
            return true;
        }

        private bool LoadObj(string filename)
        {
            return LoadFromScene(filename, new ObjLoadOptions());
        }

        private bool LoadFBX(string filename)
        {
            return LoadFromScene(filename, new FbxLoadOptions());
        }

        private bool LoadDM(string filename)
        {
            Invalidate();
            SourceData = File.ReadAllBytes(filename);

            using (var xr_loader = new XRayLoader())
            {
                using (var r = new BinaryReader(new MemoryStream(SourceData)))
                {
                    xr_loader.SetStream(r.BaseStream);

                    OgfChild chld = new OgfChild();
                    chld.LoadDM(xr_loader);
                    Childs.Add(chld);
                }
            }

            BitMask.Null(ref format);
            BitMask.Set(ref format, (int)ModelFormat.eDM);
            return true;
        }

        private bool LoadDetail(string filename)
        {
            Invalidate();
            SourceData = File.ReadAllBytes(filename);

            using (var xr_loader = new XRayLoader())
            {
                using (var r = new BinaryReader(new MemoryStream(SourceData)))
                {
                    xr_loader.SetStream(r.BaseStream);
                    xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(1, false, true));

                    int det_id = 0;

                    while (true)
                    {
                        if (!xr_loader.find_chunk(det_id)) break;

                        Stream temp = xr_loader.Reader.BaseStream;

                        if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(det_id, false, true))) break;

                        OgfChild chld = new OgfChild();
                        chld.LoadDM(xr_loader);
                        Childs.Add(chld);

                        det_id++;
                        xr_loader.SetStream(temp);
                    }

                    float step_radius = 1.2f;
                    float view_offsX = 0.0f;
                    float view_offsZ = 0.0f;
                    int size = (int)Math.Round(Math.Sqrt(Childs.Count), 0);

                    for (int i = 0; i < Childs.Count; i++)
                    {
                        if (i % size == 0)
                        {
                            view_offsX = 0.0f;
                            view_offsZ += step_radius;
                        }

                        Childs[i].SetLocalOffsetMain(new float[3] { view_offsX, 0.0f, view_offsZ });

                        view_offsX += step_radius;
                    }
                }
            }

            BitMask.Null(ref format);
            BitMask.Set(ref format, (int)ModelFormat.eDM);
            BitMask.Set(ref format, (int)ModelFormat.eDetail);
            return true;
        }

        private bool OpenFileInternal(string filename, bool silent = false)
        {
            string format = Path.GetExtension(filename);

            if (format == ".dm")
                return LoadDM(filename);
            else if (format == ".details")
                return LoadDetail(filename);
            else if (format == ".ogf")
                return LoadOGF(filename, silent);
            else if (format == ".obj")
                return LoadObj(filename);
            else if (format == ".fbx")
                return LoadFBX(filename);

            return false;
        }

        public void SaveFile(string filename, bool backup = false, ModelFormat override_fmt = ModelFormat.eUnknown)
        {
            if (SourceData == null) return;

            if (override_fmt != ModelFormat.eUnknown)
                SaveFileAsFmt(filename, backup, override_fmt);
            else if (BitMask.IsSet(format, (int)ModelFormat.eDetail))
                SaveDetail(filename, backup);
            else if (BitMask.IsSet(format, (int)ModelFormat.eDM))
                SaveDM(filename, backup);
            else if (BitMask.IsSet(format, (int)ModelFormat.eOGF))
                SaveOGF(filename, backup);
            else if (BitMask.IsSet(format, (int)ModelFormat.eObj))
            {
                SaveObj saveObj = new SaveObj();
                saveObj.ShowDialog();

                switch (saveObj.Fmt)
                {
                    case Editor.ExportFormat.OGF:
                        SaveOGF(Path.ChangeExtension(filename, ".ogf"), backup);
                        break;
                    case Editor.ExportFormat.DM:
                        SaveDM(Path.ChangeExtension(filename, ".dm"), backup);
                        break;
                    case Editor.ExportFormat.Object:
                        SaveObject(Path.ChangeExtension(filename, ".object"), backup);
                        break;
                }
            }
        }

        private void SaveFileAsFmt(string filename, bool backup, ModelFormat fmt)
        {
            if (BitMask.IsSet((int)fmt, (int)ModelFormat.eDetail))
                SaveDetail(filename, backup);
            else if (BitMask.IsSet((int)fmt, (int)ModelFormat.eDM))
                SaveDM(filename, backup);
            else if (BitMask.IsSet((int)fmt, (int)ModelFormat.eOGF))
                SaveOGF(filename, backup);
        }

        public int SaveObject(string filename, bool backup = false)
        {
            string ext = Is(ModelFormat.eDM) && !Is(ModelFormat.eObj) ? ".dm" : ".ogf"; // Create temp ext

            if (File.Exists(filename + ext)) // Remove temp file if exist
                File.Delete(filename + ext);

            SaveFile(filename + ext, backup, Is(ModelFormat.eObj) ? ModelFormat.eOGF : ModelFormat.eUnknown);

            int code = RunConverter(filename + ext, filename, ext == ".dm" ? 2 : 0, 0);

            if (File.Exists(filename + ext)) // Remove temp file
                File.Delete(filename + ext);

            return code;
        }

        public int SaveBones(string filename)
        {
            return RunConverter(FileName, filename, 0, 1);
        }

        public int SaveSkl(string filename)
        {
            return RunConverter(FileName, filename, 0, 2);
        }

        public int SaveSkls(string filename)
        {
            return RunConverter(FileName, filename, 0, 3);
        }

        public bool SaveOMF(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                fileStream.Write(Motions.Data(), 0, Motions.Data().Length);
                fileStream.Close();
            }

            return true;
        }

        public void SaveOGF(string filename, bool backup = false)
        {
            List<byte> file_bytes = new List<byte>();

            TryRepairUserdata(UserData);
            using (var fileStream = new BinaryReader(new MemoryStream(SourceData)))
            {
                byte[] temp;
                if (!Header.IsStaticSingle())
                    file_bytes.AddRange(Header.Data());

                if (Description != null)
                {
                    byte[] DescriptionData = Description.Data();

                    file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_DESC));
                    file_bytes.AddRange(BitConverter.GetBytes(DescriptionData.Length));
                    file_bytes.AddRange(DescriptionData);
                }

                if (Header.IsStaticSingle()) // Single mesh
                {
                    file_bytes.AddRange(Childs[0].Data());
                    fileStream.BaseStream.Position += Childs[0].OldSize;
                }
                else // Hierrarhy mesh
                {
                    fileStream.ReadBytes((int)(Pos - fileStream.BaseStream.Position));

                    fileStream.ReadBytes(4);
                    uint OldChildrenChunkSize = fileStream.ReadUInt32();
                    fileStream.BaseStream.Position += OldChildrenChunkSize;

                    uint ChildrenChunkSize = 0;
                    foreach (var ch in Childs)
                    {
                        if (!ch.to_delete)
                            ChildrenChunkSize += (uint)ch.Data().Length + 8;
                    }

                    int ChildrenChunk = (Header.FormatVersion == 4 ? (int)OGF.OGF4_CHILDREN : (int)OGF.OGF3_CHILDREN);
                    file_bytes.AddRange(BitConverter.GetBytes(ChildrenChunk));
                    file_bytes.AddRange(BitConverter.GetBytes(ChildrenChunkSize));

                    int ChildChunk = 0;
                    foreach (var ch in Childs)
                    {
                        if (ch.to_delete) continue;

                        byte[] ChildData = ch.Data();

                        file_bytes.AddRange(BitConverter.GetBytes(ChildChunk));
                        file_bytes.AddRange(BitConverter.GetBytes(ChildData.Length));
                        file_bytes.AddRange(ChildData);
                        ChildChunk++;
                    }
                }

                if (Header.IsSkeleton())
                {
                    if (BoneData != null)
                    {
                        if (BrokenType == 0 && BoneData.Pos > 0 && (BoneData.Pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
                        {
                            temp = fileStream.ReadBytes((int)(BoneData.Pos - fileStream.BaseStream.Position));
                            file_bytes.AddRange(temp);
                        }

                        byte[] BonesData = BoneData.Data(BrokenType == 2);

                        file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF_S_BONE_NAMES));
                        file_bytes.AddRange(BitConverter.GetBytes(BonesData.Length));
                        file_bytes.AddRange(BonesData);

                        fileStream.ReadBytes(BoneData.OldSize + 8);
                    }

                    if (IkData != null)
                    {
                        if (BrokenType == 0 && IkData.Pos > 0 && (IkData.Pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
                        {
                            temp = fileStream.ReadBytes((int)(IkData.Pos - fileStream.BaseStream.Position));
                            file_bytes.AddRange(temp);
                        }

                        byte[] IKDataData = IkData.Data();

                        file_bytes.AddRange(BitConverter.GetBytes(IkData.ChunkID(Header.FormatVersion)));
                        file_bytes.AddRange(BitConverter.GetBytes(IKDataData.Length));
                        file_bytes.AddRange(IKDataData);

                        fileStream.ReadBytes(IkData.OldSize + 8);
                    }

                    if (UserData != null)
                    {
                        if (UserData.Pos > 0 && (UserData.Pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
                        {
                            temp = fileStream.ReadBytes((int)(UserData.Pos - fileStream.BaseStream.Position));
                            file_bytes.AddRange(temp);
                        }

                        if (!string.IsNullOrEmpty(UserData.Userdata)) // Пишем если есть что писать
                        {
                            uint UserDataChunk = (Header.FormatVersion == 4 ? (uint)OGF.OGF4_S_USERDATA : (uint)OGF.OGF3_S_USERDATA);
                            byte[] UserDataData = UserData.Data();

                            file_bytes.AddRange(BitConverter.GetBytes(UserDataChunk));
                            file_bytes.AddRange(BitConverter.GetBytes(UserDataData.Length));
                            file_bytes.AddRange(UserDataData);
                        }

                        if (UserData.OldSize > 0) // Сдвигаем позицию риадера если в модели был чанк
                            fileStream.ReadBytes(UserData.OldSize + 8);
                    }

                    if (Lod != null && Header.FormatVersion == 4) // Стринг лод только у релизных OGF
                    {
                        if (Lod.Pos > 0 && (Lod.Pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
                        {
                            temp = fileStream.ReadBytes((int)(Lod.Pos - fileStream.BaseStream.Position));
                            file_bytes.AddRange(temp);
                        }

                        if (!string.IsNullOrEmpty(Lod.LodPath)) // Пишем если есть что писать
                        {
                            byte[] LodData = Lod.Data();

                            file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_LODS));
                            file_bytes.AddRange(BitConverter.GetBytes(LodData.Length));
                            file_bytes.AddRange(LodData);
                        }

                        if (Lod.OldSize > 0) // Сдвигаем позицию риадера если в модели был чанк
                            fileStream.ReadBytes(Lod.OldSize + 8);
                    }

                    bool refs_created = false;
                    if (MotionRefs != null)
                    {
                        if (MotionRefs.Pos > 0 && (MotionRefs.Pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
                        {
                            temp = fileStream.ReadBytes((int)(MotionRefs.Pos - fileStream.BaseStream.Position));
                            file_bytes.AddRange(temp);
                        }

                        if (MotionRefs.Refs.Count > 0) // Пишем если есть что писать
                        {
                            refs_created = true;
                            byte[] MotionRefsData = MotionRefs.Data(MotionRefs.Soc);

                            if (!MotionRefs.Soc)
                                file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS2));
                            else
                            {
                                uint RefsChunk = (Header.FormatVersion == 4 ? (uint)OGF.OGF4_S_MOTION_REFS : (uint)OGF.OGF3_S_MOTION_REFS);
                                file_bytes.AddRange(BitConverter.GetBytes(RefsChunk));
                            }
                            file_bytes.AddRange(BitConverter.GetBytes(MotionRefsData.Length));
                            file_bytes.AddRange(MotionRefsData);
                        }

                        if (MotionRefs.OldSize > 0) // Сдвигаем позицию риадера если в модели был чанк
                            fileStream.ReadBytes(MotionRefs.OldSize + 8);
                    }

                    if (Motions.Data() != null && !refs_created)
                        file_bytes.AddRange(Motions.Data());
                }
                else if (!Is(ModelFormat.eObj))
                {
                    temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));
                    file_bytes.AddRange(temp);
                }
            }

            WriteFile(filename, file_bytes.ToArray(), backup);
        }

        public void SaveDetail(string filename, bool backup = false)
        {
            List<byte> file_bytes = new List<byte>();
            using (var fileStream = new BinaryReader(new MemoryStream(SourceData)))
            {
                fileStream.ReadBytes(4);
                uint OldDetailsSize = fileStream.ReadUInt32();
                fileStream.BaseStream.Position += OldDetailsSize;

                uint DetailsChunkSize = 0;
                foreach (var ch in Childs)
                {
                    if (!ch.to_delete)
                        DetailsChunkSize += (uint)ch.DmData().Length + 8;
                }

                file_bytes.AddRange(BitConverter.GetBytes(1));
                file_bytes.AddRange(BitConverter.GetBytes(DetailsChunkSize));

                int DetailID = 0;
                foreach (var ch in Childs)
                {
                    if (ch.to_delete) continue;

                    byte[] DetailData = ch.DmData();

                    file_bytes.AddRange(BitConverter.GetBytes(DetailID));
                    file_bytes.AddRange(BitConverter.GetBytes(DetailData.Length));
                    file_bytes.AddRange(DetailData);
                    DetailID++;
                }
                byte[] dm_data = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));
                file_bytes.AddRange(dm_data);
                WriteFile(filename, file_bytes.ToArray(), backup);
            }
        }

        private void WriteFile(string filename, byte[] data, bool bkp)
        {
            if (bkp)
            {
                string backup_path = filename + ".bak";

                if (File.Exists(backup_path))
                    File.Delete(backup_path);

                File.Copy(filename, backup_path);
            }

            using (var fileStream = new FileStream(filename, File.Exists(filename) ? FileMode.Truncate : FileMode.Create))
            {
                fileStream.Write(data, 0, data.Length);
                fileStream.Close();
            }
        }

        public void SaveDM(string filename, bool backup = false)
        {
            SaveDM(filename, 0, backup);
        }

        public void SaveDM(string filename, int child, bool bkp = false)
        {
            List<byte> file_bytes = new List<byte>();

            byte[] dm_data = Childs[child].DmData();
            file_bytes.AddRange(dm_data);
            WriteFile(filename, file_bytes.ToArray(), bkp);
        }

        public Scene GetScene(string filename, float lod = 0.0f, bool viewport_bones = false, bool viewport_bbox = false, bool viewport_textures = false)
        {
            Scene scene = new Scene();

            int node_idx = 0;
            WriteSceneMesh WriteMesh = (Vertices, Faces, Texture, WriteTexture) =>
            {
                string mesh_name = Path.GetFileName($"{Texture}#{node_idx}");
                Node meshNode = new Node(mesh_name);

                Mesh mesh = new Mesh();
                var Normals = mesh.CreateElement(VertexElementType.Normal, MappingMode.ControlPoint, ReferenceMode.Direct) as VertexElementNormal;
                var Tangents = mesh.CreateElement(VertexElementType.Tangent, MappingMode.ControlPoint, ReferenceMode.Direct) as VertexElementTangent;
                var Binormals = mesh.CreateElement(VertexElementType.Binormal, MappingMode.ControlPoint, ReferenceMode.Direct) as VertexElementBinormal;
                var UVs = mesh.CreateElement(VertexElementType.UV, MappingMode.ControlPoint, ReferenceMode.Direct) as VertexElementUV;

                for (int i = 0; i < Vertices.Count; i++)
                {
                    float[] verts = FVec.MirrorZ(SetupObjOffset(Vertices[i]));
                    mesh.ControlPoints.Add(new Vector4(verts[0], verts[1], verts[2], 0.0f));
                    float[] norms = FVec.MirrorZ(Vertices[i].Norm());
                    Normals.Data.Add(new Vector4(norms[0], norms[1], norms[2], 0.0f));
                    float[] tang = FVec.MirrorZ(Vertices[i].Tang());
                    Tangents.Data.Add(new Vector4(tang[0], tang[1], tang[2], 0.0f));
                    float[] binorm = FVec.MirrorZ(Vertices[i].Binorm());
                    Binormals.Data.Add(new Vector4(binorm[0], binorm[1], binorm[2], 0.0f));
                    float[] uv = new float[2] { Vertices[i].Uv[0], Math.Abs(1.0f - Vertices[i].Uv[1]) };
                    UVs.Data.Add(new Vector4(uv[0], uv[1], 0.0f, 0.0f));
                }

                PolygonBuilder builder = new PolygonBuilder(mesh);
                for (int i = 0; i < Faces.Count; i++)
                {
                    builder.Begin();
                    builder.AddVertex(Faces[i].Vertex[2]);
                    builder.AddVertex(Faces[i].Vertex[1]);
                    builder.AddVertex(Faces[i].Vertex[0]);
                    builder.End();
                }

                PhongMaterial mat = new PhongMaterial();
                if (WriteTexture)
                {
                    Texture diffuse = new Texture();
                    diffuse.Name = mesh_name;
                    diffuse.FileName = $"{Texture}.png";
                    mat.SetTexture("DiffuseColor", diffuse);
                }
                mat.Name = mesh_name;
                mat.SpecularColor = new Vector3(0.0f, 0.0f, 0.0f);
                mat.Shininess = 100;
                meshNode.Material = mat;
                meshNode.Entity = mesh;
                scene.RootNode.AddChildNode(meshNode);
                node_idx++;
            };

            List<SSkelVert> sSkelVerts = new List<SSkelVert>();
            List<SSkelFace> sSkelFaces = new List<SSkelFace>();

            foreach (var ch in Childs)
            {
                if (ch.to_delete) continue;

                sSkelVerts.Clear();
                sSkelFaces.Clear();
                sSkelVerts.AddRange(ch.Vertices);
                sSkelFaces.AddRange(ch.Faces_SWI(lod));
                WriteMesh(sSkelVerts, sSkelFaces, viewport_bones ? "null_texture" : Path.GetFileName(ch.Texture), viewport_textures);
            }

            if (viewport_bbox)
            {
                if (!Header.IsStaticSingle())
                {
                    sSkelVerts.Clear();
                    sSkelFaces.Clear();
                    sSkelVerts.AddRange(Header.BBox.GetVisualVerts());
                    sSkelFaces.AddRange(Header.BBox.GetVisualFaces(sSkelVerts));
                    WriteMesh(sSkelVerts, sSkelFaces, "bbox_main_texture", true);
                }

                foreach (var ch in Childs)
                {
                    if (ch.to_delete) continue;

                    sSkelVerts.Clear();
                    sSkelFaces.Clear();
                    sSkelVerts.AddRange(ch.Header.BBox.GetVisualVerts());
                    sSkelFaces.AddRange(ch.Header.BBox.GetVisualFaces(sSkelVerts));
                    WriteMesh(sSkelVerts, sSkelFaces, "bbox_texture", true);
                }
            }

            if (viewport_bones)
            {
                for (int i = 0; i < IkData.Bones.Count; i++)
                {
                    float bbox_size = 0.024f;
                    BBox bone_box = new BBox();
                    bone_box.Min = new float[3] { -bbox_size / 2, -bbox_size / 2, -bbox_size / 2 };
                    bone_box.Max = new float[3] { bbox_size / 2, bbox_size / 2, bbox_size / 2 };

                    bone_box.Min = FVec.Add(bone_box.Min, IkData.Bones[i].RenderTransform);
                    bone_box.Max = FVec.Add(bone_box.Max, IkData.Bones[i].RenderTransform);

                    sSkelVerts.Clear();
                    sSkelFaces.Clear();
                    sSkelVerts.AddRange(bone_box.GetVisualVerts());
                    sSkelFaces.AddRange(bone_box.GetVisualFaces(sSkelVerts));
                    WriteMesh(sSkelVerts, sSkelFaces, BoneData.Bones[i].Name, true);
                }
            }

            return scene;
        }

        public void SaveObj(string filename, float lod = 0.0f, bool viewport_bones = false, bool viewport_bbox = false, bool viewport_textures = false)
        {
            Scene scene = GetScene(filename, lod, viewport_bones, viewport_bbox, viewport_textures);
            scene.Save(filename, FileFormat.WavefrontOBJ);
        }

        public void SaveFBX(string filename, float lod = 0.0f, bool viewport_bones = false, bool viewport_bbox = false, bool viewport_textures = false)
        {
            Scene scene = GetScene(filename, lod, viewport_bones, viewport_bbox, viewport_textures);
            scene.Save(filename, FileFormat.FBX7700Binary);
        }

        private float[] SetupObjOffset(SSkelVert vert)
        {
            if (!Header.IsStaticSingle() && IkData != null && IkData.ChunkVersion == 2)
                return FixOldVertexOffset(vert);

            return vert.Offset();
        }

        public bool NeedRepair()
        {
            return BrokenType > 0;
        }

        private void TryRepairUserdata(UserData data)
        {
            if (Header.FormatVersion == 4 && data != null && data.OldFormat && MessageBox.Show("Userdata has old format, update?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                data.OldFormat = false;
        }

        public void RecalcBBox(bool recalc_childs)
        {
            if (Header == null)
                Header = new OgfHeader();

            Header.BBox.Invalidate();

            foreach (OgfChild child in Childs)
            {
                if (!child.to_delete)
                {
                    if (recalc_childs)
                        child.RecalcBBox();
                    Header.BBox.Merge(child.Header.BBox);
                }
            }

            Header.BSphere.CreateSphere(Header.BBox);
        }

        public void AddBone(string name, string parent_bone, int pos)
        {
            if (Opened && Header.IsSkeleton())
            {
                // Create null OBB
                List<byte> obb = new List<byte>();
                for (int i = 0; i < 60; i++)
                    obb.Add(0);

                Bone bone = new Bone();
                bone.Name = name;
                bone.ParentName = parent_bone;
                bone.Fobb = obb.ToArray();

                BoneData.Bones.Insert(pos, bone);

                if (IkData != null)
                {
                    IKBone ikbone = new IKBone();
                    int ImportBytes = ((IkData.ChunkVersion == 4) ? 76 : ((IkData.ChunkVersion == 3) ? 72 : 60));

                    // Create null Bone Shape
                    List<byte> shape = new List<byte>();
                    for (int i = 0; i < 112 + ImportBytes; i++)
                        shape.Add(0);

                    if (IkData.ChunkVersion == 4)
                        ikbone.Version = 1;

                    ikbone.Material = "default_object";
                    ikbone.KinematicData = shape.ToArray();
                    ikbone.Rotation = new float[3];
                    ikbone.Position = new float[3];
                    ikbone.Mass = 10.0f;
                    ikbone.CenterMass = new float[3];

                    IkData.Bones.Insert(pos, ikbone);
                }
            }
        }

        public void RemoveBone(string bone)
        {
            if (Opened && Header.IsSkeleton() && BoneData != null)
            {
                RemoveBone(BoneData.GetBoneID(bone));
            }
        }

        public void RemoveBone(int bone)
        {
            if (Opened && Header.IsSkeleton())
            {
                BoneData.RemoveBone(bone);

                if (IkData != null)
                    IkData.RemoveBone(bone);
            }
        }

        public void ChangeParent(string old, string _new)
        {
            if (Opened && Header.IsSkeleton())
            {
                for (int i = 0; i < BoneData.Bones.Count; i++)
                {
                    if (BoneData.Bones[i].ParentName == old)
                        BoneData.Bones[i].ParentName = _new;
                }
            }
        }

        public void ChangeModelFormat()
        {
            if (Opened)
            {
                if (Header.FormatVersion != 4)
                {
                    MessageBox.Show("Can't convert model. Unsupported OGF version: " + Header.FormatVersion.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                IsCopModel = !IsCopModel;

                if (IsCopModel)
                {
                    if (MotionRefs != null)
                        MotionRefs.Soc = false;

                    foreach (var ch in Childs)
                    {
                        if (ch.links >= 0x12071980)
                            ch.links /= 0x12071980;
                    }
                }
                else
                {
                    uint links = 0;

                    foreach (var ch in Childs)
                        links = Math.Max(links, ch.LinksCount());

                    if (links > 2 && MessageBox.Show("Model has more than 2 links. After converting to SoC model will lose influence data, continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        IsCopModel = !IsCopModel;
                        return;
                    }

                    foreach (var ch in Childs)
                    {
                        if (ch.LinksCount() > 2)
                            ch.SetLinks(1);
                    }

                    if (Motions.Anims != null)
                    {
                        foreach (var Anim in Motions.Anims)
                        {
                            bool key16bit = (Anim.flags & (int)MotionKeyFlags.flTKey16IsBit) == (int)MotionKeyFlags.flTKey16IsBit;
                            bool keynocompressbit = (Anim.flags & (int)MotionKeyFlags.flTKeyFFT_Bit) == (int)MotionKeyFlags.flTKeyFFT_Bit;

                            if (key16bit || keynocompressbit)
                            {
                                if (MessageBox.Show("Build-in motions are in " + (keynocompressbit ? "no compression" : "16 bit compression") + " format, not supported in SoC. Delete motions?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                                    Motions.SetData(null);
                                break;
                            }
                        }
                    }

                    if (MotionRefs != null)
                        MotionRefs.Soc = true;

                    foreach (var ch in Childs)
                    {
                        if (ch.links < 0x12071980)
                            ch.links *= 0x12071980;
                    }
                }
            }
        }
    }
}
