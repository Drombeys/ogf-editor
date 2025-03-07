using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OgfTool
{
    public class XRayLoader : IDisposable
    {
        public long ChunkPos { get; private set; } = 0;

        private readonly uint CHUNK_COMPRESSED = 0x80000000;

        public MemoryStream MemStream { get; private set; }
        public BinaryReader Reader { get; private set; }


        public void Destroy()
        {
            MemStream.Dispose();
            Reader.Dispose();
        }

        public byte ReadByte()
        {
            return Reader.ReadByte();
        }

        public int ReadInt32()
        {
            return Reader.ReadInt32();
        }

        public long ReadInt64()
        {
            return Reader.ReadInt64();
        }

        public float ReadFloat()
        {
            return Reader.ReadSingle();
        }

        public float[] ReadVector()
        {
            float[] vec = new float[3];

            vec[0] = Reader.ReadSingle();
            vec[1] = Reader.ReadSingle();
            vec[2] = Reader.ReadSingle();

            return vec;
        }

        public float[] ReadVector2()
        {
            float[] vec = new float[2];

            vec[0] = Reader.ReadSingle();
            vec[1] = Reader.ReadSingle();

            return vec;
        }

        public uint ReadUInt16()
        {
            return Reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return Reader.ReadUInt32();
        }

        public byte[] ReadBytes(int count)
        {
            return Reader.ReadBytes(count);
        }

        public bool SetData(byte[] input)
        {
            if (input == null) return false;
            MemStream = new MemoryStream(input);
            Reader = new BinaryReader(MemStream);
            return true;
        }

        public void SetStream(Stream stream)
        {
            Reader = new BinaryReader(stream);
        }

        public void SetReader(BinaryReader rd)
        {
            Reader = rd;
        }

        public bool find_chunk(int chunkId, bool skip = false, bool reset = false)
        {
            return find_chunkSize(chunkId, skip, reset) != 0;
        }

        public byte[] find_and_return_chunk_in_chunk(int chunkId, bool skip = false, bool reset = false)
        {
            int size = (int)find_chunkSize(chunkId, skip, reset);

            if (size > 0)
            {
                return ReadBytes(size);
            }
            else
                return null;
        }

        public uint find_chunkSize(int chunkId, bool skip = false, bool reset = false)
        {
            ChunkPos = 0;

            if (reset) Reader.BaseStream.Position = 0;

            while (Reader.BaseStream.Position < Reader.BaseStream.Length)
            {
                if (Reader.BaseStream.Position + 8 > Reader.BaseStream.Length)
                    return 0;

                uint dwType = Reader.ReadUInt32();
                uint dwSize = Reader.ReadUInt32();

                if ((dwType == chunkId || (dwType ^ CHUNK_COMPRESSED) == chunkId) && ( Reader.BaseStream.Position - 8 + dwSize <= Reader.BaseStream.Length ) )
                {
                    ChunkPos = Reader.BaseStream.Position - 8;
                    return dwSize;
                }
                else
                {
                    if (Reader.BaseStream.Position + dwSize < Reader.BaseStream.Length)
                        Reader.BaseStream.Position += dwSize;
                    else if (Reader.BaseStream.Position + 8 < Reader.BaseStream.Length)
                        Reader.BaseStream.Position += 4;
                    else
                        return 0;
                }
            }

            return 0;
        }

        public void open_chunk(BinaryWriter w, int chunkId)
        {
            w.Write(chunkId);
            ChunkPos = w.BaseStream.Position;
            w.Write(0);     // the place for 'size'
        }

        public void close_chunk(BinaryWriter w)
        {
            if (ChunkPos == 0)
            {
                throw new InvalidOperationException("no chunk!");
            }

            long pos = w.BaseStream.Position;
            w.BaseStream.Position = ChunkPos;
            w.Write((int)(pos - ChunkPos - 4));
            w.BaseStream.Position = pos;
            ChunkPos = 0;
        }

        public string read_stringData(ref bool data)
        {
            string str = "";
            data = false;

            while (Reader.BaseStream.Position < Reader.BaseStream.Length)
            {
                byte[] one = { Reader.ReadByte() };
                if (one[0] != 0 && one[0] != 0xA && one[0] != 0xD)
                {
                    str += Encoding.Default.GetString(one);
                }
                else
                {
                    if (one[0] == 0xD)
                    {
                        Reader.ReadByte();
                        data = true;
                    }
                    break;
                }
            }
            return str;
        }

        public string ReadStringZ()
        {
            string str = string.Empty;

            while (Reader.BaseStream.Position < Reader.BaseStream.Length)
            {
                byte[] one = { Reader.ReadByte() };
                if (one[0] != 0 && one[0] != 0xE)
                {
                    str += Encoding.Default.GetString(one);
                }
                else
                {
                    break;
                }
            }
            return str;
        }

        public string read_stringSize(uint size)
        {
            string str = "";

            for (uint i = 0; i < size; i++)
            {
                byte[] one = { Reader.ReadByte() };
                str += Encoding.Default.GetString(one);
            }
            return str;
        }


        public void write_stringZ(BinaryWriter w, string str)
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(Encoding.Default.GetBytes(str));
            temp.Add(0);

            w.Write(temp.ToArray());
        }

        public void write_u32(BinaryWriter w, uint num)
        {
            w.Write(num);
        }

        public void Dispose()
        {
            Destroy();
        }
    }
}
