using System;
using System.Collections.Generic;

namespace OgfTool
{
    public class OgfHeader
    {
        public byte FormatVersion { get; private set; }
        public byte Type { get; private set; }
        public short ShaderId { get; private set; }
        public BBox BBox { get; private set; }
        public BSphere BSphere { get; private set; }

        public OgfHeader()
        {
            BBox = new BBox();
            BSphere = new BSphere();
            FormatVersion = 4;
            Type = 0;
            ShaderId = 0;
        }

        public void Load(XRayLoader xr_loader)
        {
            FormatVersion = xr_loader.ReadByte();
            Type = xr_loader.ReadByte();
            ShaderId = (short)xr_loader.ReadUInt16();

            if (FormatVersion == 4)
            {
                BBox.Load(xr_loader);
                BSphere.Load(xr_loader);
            }
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(BitConverter.GetBytes((uint)OGF.OGF_HEADER));
            temp.AddRange(BitConverter.GetBytes((FormatVersion == 4 ? 44 : 4)));

            temp.Add(FormatVersion);
            temp.Add(Type);
            temp.AddRange(BitConverter.GetBytes(ShaderId));

            if (FormatVersion == 4)
            {
                temp.AddRange(BBox.Data());
                temp.AddRange(BSphere.Data());
            }

            return temp.ToArray();
        }

        public bool IsProgressive()
        {
            if (FormatVersion == 4)
                return Type == (byte)ModelType.MT4_PROGRESSIVE || Type == (byte)ModelType.MT4_SKELETON_GEOMDEF_PM;
            else
                return Type == (byte)ModelType.MT3_PROGRESSIVE || Type == (byte)ModelType.MT3_SKELETON_GEOMDEF_PM;
        }

        public bool IsSkeleton()
        {
            if (FormatVersion == 4)
                return Type == (byte)ModelType.MT4_SKELETON_ANIM || Type == (byte)ModelType.MT4_SKELETON_RIGID;
            else
                return Type == (byte)ModelType.MT3_SKELETON_ANIM || Type == (byte)ModelType.MT3_SKELETON_RIGID;
        }

        public bool IsAnimated()
        {
            if (FormatVersion == 4)
                return Type == (byte)ModelType.MT4_SKELETON_ANIM;
            else
                return Type == (byte)ModelType.MT3_SKELETON_ANIM;
        }

        public bool IsStatic()
        {
            if (FormatVersion == 4)
                return Type == (byte)ModelType.MT4_HIERRARHY;
            else
                return Type == (byte)ModelType.MT3_HIERRARHY;
        }

        public bool IsStaticSingle()
        {
            if (FormatVersion == 4)
                return Type == (byte)ModelType.MT4_NORMAL || Type == (byte)ModelType.MT4_PROGRESSIVE;
            else
                return Type == (byte)ModelType.MT3_NORMAL || Type == (byte)ModelType.MT3_PROGRESSIVE || Type == (byte)ModelType.MT3_PROGRESSIVE2;
        }

        public void Skeleton()
        {
            if (FormatVersion == 4)
                Type = (byte)ModelType.MT4_SKELETON_RIGID;
            else
                Type = (byte)ModelType.MT3_SKELETON_RIGID;
        }


        public void Animated()
        {
            if (FormatVersion == 4)
                Type = (byte)ModelType.MT4_SKELETON_ANIM;
            else
                Type = (byte)ModelType.MT3_SKELETON_ANIM;
        }

        public void GeomdefST()
        {
            if (FormatVersion == 4)
                Type = (byte)ModelType.MT4_SKELETON_GEOMDEF_ST;
            else
                Type = (byte)ModelType.MT3_SKELETON_GEOMDEF_ST;
        }

        public void Normal()
        {
            if (FormatVersion == 4)
                Type = (byte)ModelType.MT4_NORMAL;
            else
                Type = (byte)ModelType.MT3_NORMAL;
        }

        public void Static(List<OgfChild> childs)
        {
            int childs_count = 0;
            foreach (OgfChild chld in childs)
            {
                if (!chld.to_delete)
                    childs_count++;
            }

            if (childs_count <= 1)
            {
                StaticSingle(childs);
                return;
            }

            if (FormatVersion == 4)
                Type = (byte)ModelType.MT4_HIERRARHY;
            else
                Type = (byte)ModelType.MT3_HIERRARHY;
        }

        public void StaticSingle(List<OgfChild> childs)
        {
            int childs_count = 0;
            foreach (OgfChild chld in childs)
            {
                if (!chld.to_delete)
                    childs_count++;
            }

            if (childs_count > 1)
            {
                Static(childs);
                return;
            }

            if (FormatVersion == 4)
                Type = (childs[0].SWI.Count > 0) ? (byte)ModelType.MT4_PROGRESSIVE : (byte)ModelType.MT4_NORMAL;
            else
                Type = (childs[0].SWI.Count > 0) ? (byte)ModelType.MT3_PROGRESSIVE : (byte)ModelType.MT3_NORMAL;
        }
    };
}
