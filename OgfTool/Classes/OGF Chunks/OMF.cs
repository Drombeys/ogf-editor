using System.Collections.Generic;
using System.IO;

namespace OgfTool
{
    public class OMF
    {
        public class Anim
        {
            public byte flags;

            public Anim()
            {
                flags = 0;
            }
        }

        private byte[] omfData;
        private string omfText;

        public List<Anim> Anims { get; private set; }

        public OMF()
        {
            omfData = null;
            Anims = null;
            omfText = string.Empty;
        }

        public bool SetData(byte[] _data)
        {
            if (_data == null)
            {
                omfData = null;
                Anims = null;
                omfText = string.Empty;
            }
            else
            {
                omfData = _data;

                using (XRayLoader xr_loader = new XRayLoader())
                {
                    using (var r = new BinaryReader(new MemoryStream(Data())))
                    {
                        xr_loader.SetStream(r.BaseStream);

                        if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF_S_MOTIONS, false, true)))
                        {
                            omfText = "";
                            Anims = new List<Anim>();

                            int id = 0;

                            while (true)
                            {
                                if (!xr_loader.find_chunk(id)) break;

                                Stream temp = xr_loader.Reader.BaseStream;

                                if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

                                Anim anim = new Anim();

                                if (id == 0)
                                    omfText += $"Motions count : {xr_loader.ReadUInt32()}\n";
                                else
                                {
                                    omfText += $"\n{id}. {xr_loader.ReadStringZ()}";
                                    xr_loader.ReadUInt32();
                                    anim.flags = xr_loader.ReadByte();
                                }

                                id++;
                                xr_loader.SetStream(temp);
                                Anims.Add(anim);
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public byte[] Data()
        {
            return omfData;
        }

        public override string ToString()
        {
            return omfText;
        }
    }
}
