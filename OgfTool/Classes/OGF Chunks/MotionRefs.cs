using System;
using System.Collections.Generic;
using System.Text;

namespace OgfTool
{
    public class MotionRefs
    {
        public long Pos { get; set; }
        public List<string> Refs { get; set; }
        public bool Soc { get; set; }
        public int OldSize { get; set; }

        public MotionRefs()
        {
            Pos = 0;
            OldSize = 0;
            Refs = new List<string>();
            Soc = false;
        }

        public void Load(XRayLoader xr_loader, bool string_refs)
        {
            Pos = xr_loader.ChunkPos;

            if (string_refs)
            {
                Soc = true;
                string motions = xr_loader.ReadStringZ();
                string motion = string.Empty;
                for (int i = 0; i < motions.Length; i++)
                {
                    if (motions[i] != ',')
                    {
                        motion += motions[i];
                    }
                    else
                    {
                        Refs.Add(motion);
                        motion = string.Empty;
                    }

                }

                if (!string.IsNullOrEmpty(motion))
                    Refs.Add(motion);
            }
            else
            {
                uint count = xr_loader.ReadUInt32();

                for (int i = 0; i < count; i++)
                    Refs.Add(xr_loader.ReadStringZ());
            }

            OldSize = Data(Soc).Length;
        }

        public byte[] Data(bool v3)
        {
            List<byte> temp = new List<byte>();

            if (!v3)
            {
                temp.AddRange(BitConverter.GetBytes(Refs.Count));

                foreach (var str in Refs)
                {
                    temp.AddRange(Encoding.Default.GetBytes(str));
                    temp.Add(0);
                }
            }
            else
            {
                string strref = Refs[0];
                if (Refs.Count > 1)
                {
                    for (int i = 1; i < Refs.Count; i++)
                        strref += "," + Refs[i];
                }

                temp.AddRange(Encoding.Default.GetBytes(strref));
                temp.Add(0);
            }

            return temp.ToArray();
        }
    }
}
