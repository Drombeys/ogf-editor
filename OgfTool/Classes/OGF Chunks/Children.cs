using System;
using System.Collections.Generic;
using System.Text;

namespace OgfTool
{
    public class OgfChild
    {
        public string Texture { get; set; }
        public string Shader { get; set; }

        public uint Flags { get; set; }
        public float MinScale { get; set; }
        public float MaxScale { get; set; }

        public int OldSize { get; set; }

        public uint links { get; set; }
        public uint FVF { get; set; }
        public bool to_delete { get; set; }

        public List<SSkelVert> Vertices { get; }
        public List<SSkelFace> Faces { get; set; }
        public List<VIPM_SWR> SWI { get; set; }
        public OgfHeader Header { get; set; }

        public OgfChild()
        {
            Vertices = new List<SSkelVert>();
            Faces = new List<SSkelFace>();
            SWI = new List<VIPM_SWR>();
            OldSize = 0;
            links = 0;
            to_delete = false;
            FVF = 0x112;
            MinScale = 1.0f;
            MaxScale = 1.0f;
            Flags = 0;
        }
        public float[] GetLocalOffset()
        {
            if (Vertices.Count > 0)
                return Vertices[0].local_offset;
            else
                return new float[3];
        }

        public float[] GetLocalOffsetMain()
        {
            if (Vertices.Count > 0)
                return Vertices[0].local_offset2;
            else
                return new float[3];
        }

        public float[] GetLocalRotation()
        {
            if (Vertices.Count > 0)
                return Vertices[0].local_rotation;
            else
                return new float[3];
        }

        public bool GetLocalRotationFlag()
        {
            if (Vertices.Count > 0)
                return Vertices[0].rotation_local;
            else
                return false;
        }

        public void SetLocalOffset(float[] offs)
        {
            for (int i = 0; i < Vertices.Count; i++)
                Vertices[i].local_offset = offs;
        }

        public void SetLocalRotation(float[] rot, float[] center, bool local)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].local_rotation = rot;
                Vertices[i].center = center;
                Vertices[i].rotation_local = local;
            }
        }

        public void SetLocalOffsetMain(float[] offs)
        {
            for (int i = 0; i < Vertices.Count; i++)
                Vertices[i].local_offset2 = offs;
        }

        public void SetLocalRotationMain(float[] rot)
        {
            for (int i = 0; i < Vertices.Count; i++)
                Vertices[i].local_rotation2 = rot;
        }

        private int CalcLod(float lod)
        {
            return (int)Math.Floor(lod * (SWI.Count - 1) + 0.5f);
        }

        public List<SSkelFace> Faces_SWI(float lod)
        {
            if (SWI.Count == 0) return Faces;

            List<SSkelFace> sSkelFaces = new List<SSkelFace>();

            VIPM_SWR SWR = SWI[CalcLod(lod)];

            for (int i = (int)SWR.Offset / 3; i < ((int)SWR.Offset / 3) + SWR.NumTris; i++)
            {
                sSkelFaces.Add(Faces[i]);
            }

            return sSkelFaces;
        }

        public uint LinksCount()
        {
            return links >= 0x12071980 ? links / 0x12071980 : links;
        }

        public void SetLinks(uint count)
        {
            if (links >= 0x12071980)
                links = count * 0x12071980;
            else
                links = count;
        }

        public void MeshNormalize(bool generate_normal = true)
        {
            SSkelVert.GenerateNormals(Vertices, Faces, generate_normal);
        }

        public void RecalcBBox()
        {
            if (Header == null)
                Header = new OgfHeader();

            Header.BBox.CreateBox(Vertices);
            Header.BSphere.CreateSphere(Header.BBox);
        }

        public void LoadDM(XRayLoader xr_loader)
        {
            Shader = xr_loader.ReadStringZ();
            Texture = xr_loader.ReadStringZ();
            Flags = xr_loader.ReadUInt32();
            MinScale = xr_loader.ReadFloat();
            MaxScale = xr_loader.ReadFloat();

            OldSize = Shader.Length + Texture.Length + 2;

            uint verts = xr_loader.ReadUInt32();
            uint faces = xr_loader.ReadUInt32() / 3;

            for (int i = 0; i < verts; i++)
            {
                SSkelVert Vert = new SSkelVert();
                Vert.Offs = xr_loader.ReadVector();
                Vert.Uv = xr_loader.ReadVector2();
                Vertices.Add(Vert);
            }

            for (int i = 0; i < faces; i++)
            {
                SSkelFace Face = new SSkelFace();
                Face.Vertex[0] = (ushort)xr_loader.ReadUInt16();
                Face.Vertex[1] = (ushort)xr_loader.ReadUInt16();
                Face.Vertex[2] = (ushort)xr_loader.ReadUInt16();
                Faces.Add(Face);
            }

            RecalcBBox();
            MeshNormalize();
        }

        public bool Load(XRayLoader xr_loader)
        {
            // Header start
            if (!xr_loader.find_chunk((int)OGF.OGF_HEADER, false, true)) 
                return false;

            Header = new OgfHeader();
            Header.Load(xr_loader);
            // Header end

            // Texture start
            if (!xr_loader.find_chunk((int)OGF.OGF_TEXTURE, false, true))
                return false;

            Texture = xr_loader.ReadStringZ();
            Shader = xr_loader.ReadStringZ();
            // Texture end

            // Verts start
            int VertsChunk = (Header.FormatVersion == 3 ? (int)OGF.OGF3_VERTICES : (int)OGF.OGF4_VERTICES);

            if (!xr_loader.find_chunk(VertsChunk, false, true))
                return false;

            FVF = links = xr_loader.ReadUInt32();
            uint verts = xr_loader.ReadUInt32();

            if (Header.IsStaticSingle())
                links = 0;
            else
                FVF = 0;

            for (int i = 0; i < verts; i++)
            {
                SSkelVert Vert = new SSkelVert();
                switch (LinksCount())
                {
                    case 1:
                        Vert.Offs = xr_loader.ReadVector();
                        Vert.norm = xr_loader.ReadVector();
                        if (Header.FormatVersion == 4)
                        {
                            Vert.tang = xr_loader.ReadVector();
                            Vert.binorm = xr_loader.ReadVector();
                        }
                        Vert.Uv = xr_loader.ReadVector2();

                        Vert.bones_id[0] = xr_loader.ReadUInt32();
                        break;
                    case 2:
                        Vert.bones_id[0] = xr_loader.ReadUInt16();
                        Vert.bones_id[1] = xr_loader.ReadUInt16();

                        Vert.Offs = xr_loader.ReadVector();
                        Vert.norm = xr_loader.ReadVector();
                        Vert.tang = xr_loader.ReadVector();
                        Vert.binorm = xr_loader.ReadVector();
                        Vert.bones_infl[0] = xr_loader.ReadFloat();
                        Vert.Uv = xr_loader.ReadVector2();
                        break;
                    case 3:
                    case 4:
                        for (int j = 0; j < LinksCount(); j++)
                            Vert.bones_id[j] = xr_loader.ReadUInt16();

                        Vert.Offs = xr_loader.ReadVector();
                        Vert.norm = xr_loader.ReadVector();
                        Vert.tang = xr_loader.ReadVector();
                        Vert.binorm = xr_loader.ReadVector();

                        for (int j = 0; j < LinksCount() - 1; j++)
                            Vert.bones_infl[j] = xr_loader.ReadFloat();

                        Vert.Uv = xr_loader.ReadVector2();
                        break;
                    default:
                        Vert.Offs = xr_loader.ReadVector();
                        Vert.norm = xr_loader.ReadVector();
                        Vert.Uv = xr_loader.ReadVector2();
                        break;
                }
                Vertices.Add(Vert);
            }
            // Verts end

            // Indices start
            int FacesChunk = (Header.FormatVersion == 3 ? (int)OGF.OGF3_INDICES : (int)OGF.OGF4_INDICES);

            if (!xr_loader.find_chunk(FacesChunk, false, true))
                return false;

            uint faces = xr_loader.ReadUInt32() / 3;

            for (uint i = 0; i < faces; i++)
            {
                SSkelFace Face = new SSkelFace();
                Face.Vertex[0] = (ushort)xr_loader.ReadUInt16();
                Face.Vertex[1] = (ushort)xr_loader.ReadUInt16();
                Face.Vertex[2] = (ushort)xr_loader.ReadUInt16();
                Faces.Add(Face);
            }
            // Indices end

            // SWR start
            if (Header.FormatVersion == 4)
            {
                if (xr_loader.find_chunk((int)OGF.OGF4_SWIDATA, false, true))
                {
                    xr_loader.ReadUInt32();
                    xr_loader.ReadUInt32();
                    xr_loader.ReadUInt32();
                    xr_loader.ReadUInt32();

                    uint swi_size = xr_loader.ReadUInt32();

                    if (xr_loader.Reader.BaseStream.Position + swi_size * 8 <= xr_loader.Reader.BaseStream.Length)
                    {
                        for (uint i = 0; i < swi_size; i++)
                        {
                            VIPM_SWR SWR = new VIPM_SWR();
                            SWR.Offset = xr_loader.ReadUInt32();
                            SWR.NumTris = (ushort)xr_loader.ReadUInt16();
                            SWR.NumVerts = (ushort)xr_loader.ReadUInt16();
                            SWI.Add(SWR);
                        }
                    }
                }
            }
            // SWR end

            // Fix Tangent Basis
            if (links == 0 || links == 1 && Header.FormatVersion != 4)
                MeshNormalize(false);

            OldSize = Data().Length;

            return true;
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            // Header start
            temp.AddRange(Header.Data());
            // Header end

            // Texture start
            temp.AddRange(BitConverter.GetBytes((uint)OGF.OGF_TEXTURE));
            temp.AddRange(BitConverter.GetBytes(Texture.Length + Shader.Length + 2));

            temp.AddRange(Encoding.Default.GetBytes(Texture));
            temp.Add(0);
            temp.AddRange(Encoding.Default.GetBytes(Shader));
            temp.Add(0);
            // Texture end

            // Verts start
            uint VertsChunk = (Header.FormatVersion == 4 ? (uint)OGF.OGF4_VERTICES : (uint)OGF.OGF3_VERTICES);
            temp.AddRange(BitConverter.GetBytes(VertsChunk));
            temp.AddRange(BitConverter.GetBytes((uint)GetVertsChunk().Length));

            temp.AddRange(GetVertsChunk());
            // Verts end

            // Indices start
            uint FacesChunk = (Header.FormatVersion == 4 ? (uint)OGF.OGF4_INDICES : (uint)OGF.OGF3_INDICES);
            temp.AddRange(BitConverter.GetBytes(FacesChunk));
            temp.AddRange(BitConverter.GetBytes(Faces.Count * 3 * 2 + 4));

            temp.AddRange(BitConverter.GetBytes(Faces.Count * 3));
            for (int i = 0; i < Faces.Count; i++)
            {
                temp.AddRange(BitConverter.GetBytes((ushort)Faces[i].Vertex[0]));
                temp.AddRange(BitConverter.GetBytes((ushort)Faces[i].Vertex[1]));
                temp.AddRange(BitConverter.GetBytes((ushort)Faces[i].Vertex[2]));
            }
            // Indices end

            // SWR start
            if (SWI.Count > 0)
            {
                temp.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_SWIDATA));
                temp.AddRange(BitConverter.GetBytes(4 + 4 + 4 + 4 + 4 + SWI.Count * 8));

                temp.AddRange(BitConverter.GetBytes(0));
                temp.AddRange(BitConverter.GetBytes(0));
                temp.AddRange(BitConverter.GetBytes(0));
                temp.AddRange(BitConverter.GetBytes(0));
                temp.AddRange(BitConverter.GetBytes(SWI.Count));

                for (int i = 0; i < SWI.Count; i++)
                {
                    temp.AddRange(BitConverter.GetBytes(SWI[i].Offset));
                    temp.AddRange(BitConverter.GetBytes(SWI[i].NumTris));
                    temp.AddRange(BitConverter.GetBytes(SWI[i].NumVerts));
                }
            }
            // SWR end

            return temp.ToArray();
        }

        public byte[] DmData()
        {
            List<byte> temp = new List<byte>();

            // Texture start
            temp.AddRange(Encoding.Default.GetBytes(Shader));
            temp.Add(0);
            temp.AddRange(Encoding.Default.GetBytes(Texture));
            temp.Add(0);
            // Texture end

            // Params start
            temp.AddRange(BitConverter.GetBytes(Flags));
            temp.AddRange(BitConverter.GetBytes(MinScale));
            temp.AddRange(BitConverter.GetBytes(MaxScale));
            temp.AddRange(BitConverter.GetBytes(Vertices.Count));
            temp.AddRange(BitConverter.GetBytes(Faces.Count * 3));
            // Params end

            // Verts start
            for (int i = 0; i < Vertices.Count; i++)
            {
                temp.AddRange(FVec.GetBytes(Vertices[i].Offset2()));
                temp.AddRange(FVec2.GetBytes(Vertices[i].Uv));
            }
            // Verts end

            // Indices start
            for (int i = 0; i < Faces.Count; i++)
            {
                temp.AddRange(BitConverter.GetBytes(Faces[i].Vertex[0]));
                temp.AddRange(BitConverter.GetBytes(Faces[i].Vertex[1]));
                temp.AddRange(BitConverter.GetBytes(Faces[i].Vertex[2]));
            }
            // Indices end

            return temp.ToArray();
        }

        private byte[] GetVertsChunk()
        {
            List<byte> temp = new List<byte>();

            if (Header.IsStaticSingle())
                temp.AddRange(BitConverter.GetBytes(FVF));
            else
            {
                if (links == 0)
                {
                    AutoClosingMessageBox.Show("Error! Links in dynamic model should not be a zero. Set to 1.", "Error", 10000, System.Windows.Forms.MessageBoxIcon.Error);
                    links = 1;
                }

                temp.AddRange(BitConverter.GetBytes(links));
            }

            temp.AddRange(BitConverter.GetBytes(Vertices.Count));

            for (int i = 0; i < Vertices.Count; i++)
            {
                SSkelVert vert = Vertices[i];

                switch (LinksCount())
                {
                    case 1:
                        temp.AddRange(FVec.GetBytes(vert.Offset()));
                        temp.AddRange(FVec.GetBytes(vert.Norm()));

                        if (Header.FormatVersion == 4)
                        {
                            temp.AddRange(FVec.GetBytes(vert.Tang()));
                            temp.AddRange(FVec.GetBytes(vert.Binorm()));
                        }

                        temp.AddRange(FVec2.GetBytes(vert.Uv));
                        temp.AddRange(BitConverter.GetBytes(vert.bones_id[0]));
                        break;
                    case 2:
                        temp.AddRange(BitConverter.GetBytes((short)vert.bones_id[0]));
                        temp.AddRange(BitConverter.GetBytes((short)vert.bones_id[1]));

                        temp.AddRange(FVec.GetBytes(vert.Offset()));
                        temp.AddRange(FVec.GetBytes(vert.Norm()));

                        temp.AddRange(FVec.GetBytes(vert.Tang()));
                        temp.AddRange(FVec.GetBytes(vert.Binorm()));

                        temp.AddRange(BitConverter.GetBytes(vert.bones_infl[0]));
                        temp.AddRange(FVec2.GetBytes(vert.Uv));
                        break;
                    case 3:
                    case 4:
                        for (int j = 0; j < LinksCount(); j++)
                            temp.AddRange(BitConverter.GetBytes((short)vert.bones_id[j]));

                        temp.AddRange(FVec.GetBytes(vert.Offset()));
                        temp.AddRange(FVec.GetBytes(vert.Norm()));

                        temp.AddRange(FVec.GetBytes(vert.Tang()));
                        temp.AddRange(FVec.GetBytes(vert.Binorm()));

                        for (int j = 0; j < LinksCount() - 1; j++)
                            temp.AddRange(BitConverter.GetBytes(vert.bones_infl[j]));

                        temp.AddRange(FVec2.GetBytes(vert.Uv));
                        break;
                    default: // Static
                        temp.AddRange(FVec.GetBytes(vert.Offset()));
                        temp.AddRange(FVec.GetBytes(vert.Norm()));
                        temp.AddRange(FVec2.GetBytes(vert.Uv));
                        break;
                }
            }

            return temp.ToArray();
        }
    }
}
