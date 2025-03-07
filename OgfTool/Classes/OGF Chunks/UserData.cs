using System.Collections.Generic;
using System.Text;

namespace OgfTool
{
    public class UserData
    {
        public long Pos { get; set; }
        public int OldSize { get; set; }
        public string Userdata { get; set; }
        public bool OldFormat { get; set; }

        public UserData()
        {
            Pos = 0;
            OldSize = 0;
            Userdata = string.Empty;
            OldFormat = false;
        }

        public void Load(XRayLoader xr_loader, uint chunk_size)
        {
            Pos = xr_loader.ChunkPos;

            long UserdataStreamPos = xr_loader.Reader.BaseStream.Position;
            Userdata = xr_loader.ReadStringZ();

            if (Userdata.Length + 1 != chunk_size)
            {
                OldFormat = true;
                xr_loader.Reader.BaseStream.Position = UserdataStreamPos;
                Userdata = xr_loader.read_stringSize(chunk_size);
            }

            OldSize = Data().Length;
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(Encoding.Default.GetBytes(Userdata));
            if (!OldFormat)
                temp.Add(0);

            return temp.ToArray();
        }
    }
}
