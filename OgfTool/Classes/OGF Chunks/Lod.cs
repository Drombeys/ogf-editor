using System.Collections.Generic;
using System.Text;

namespace OgfTool
{
    public class Lod
    {
        public long Pos { get; set; }
        public int OldSize { get; set; }
        public string LodPath { get; set; }
        private bool data_str;

        public Lod()
        {
            Pos = 0;
            OldSize = 0;
            LodPath = "";
            data_str = true;
        }

        public void Load(XRayLoader xr_loader)
        {
            Pos = xr_loader.ChunkPos;
            LodPath = xr_loader.read_stringData(ref data_str);
            OldSize = Data().Length;
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(Encoding.Default.GetBytes(LodPath));
            if (data_str)
            {
                temp.Add(0xD);
                temp.Add(0xA);
            }
            else
                temp.Add(0);

            return temp.ToArray();
        }
    }
}
