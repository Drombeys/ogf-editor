using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OgfTool
{
    public class Description
    {
        public long Pos { get; set; }
        public int OldSize { get; set; }
        public bool FourByte { get; set; }

        public string Source { get; set; }
        public string ExportTool { get; set; }
        public long ExportTime { get; set; }
        public string OwnerName { get; set; }
        public long CreationTime { get; set; }
        public string ExportModifNameTool { get; set; }
        public long ModifiedTime { get; set; }

        public Description()
        {
            Pos = 0;
            OldSize = 0;
            FourByte = true;

            Source = string.Empty;
            ExportTool = string.Empty;
            ExportTime = 0;
            OwnerName = string.Empty;
            CreationTime = 0;
            ExportModifNameTool = string.Empty;
            ModifiedTime = 0;
        }

        public byte Load(XRayLoader xr_loader, uint chunk_size)
        {
            byte broken_type = 0;

            Pos = xr_loader.ChunkPos;

            // Читаем таймеры в 8 байт
            long reader_start_pos = xr_loader.Reader.BaseStream.Position;
            Source = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
            ExportTool = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
            ExportTime = xr_loader.ReadInt64();
            OwnerName = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
            CreationTime = xr_loader.ReadInt64();
            ExportModifNameTool = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
            ModifiedTime = xr_loader.ReadInt64();
            long description_end_pos = xr_loader.Reader.BaseStream.Position;

            if ((description_end_pos - reader_start_pos) != chunk_size) // Размер не состыковывается, пробуем читать 4 байта
            {
                xr_loader.Reader.BaseStream.Position = reader_start_pos;
                Source = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
                ExportTool = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
                ExportTime = xr_loader.ReadUInt32();
                OwnerName = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
                CreationTime = xr_loader.ReadUInt32();
                ExportModifNameTool = Regex.Replace(xr_loader.ReadStringZ(), @"\p{C}+", string.Empty);
                ModifiedTime = xr_loader.ReadUInt32();
                description_end_pos = xr_loader.Reader.BaseStream.Position;

                FourByte = true; // Ставим флаг на то что мы прочитали чанк с 4х байтными таймерами

                if ((description_end_pos - reader_start_pos) != chunk_size) // Все равно разный размер? Походу модель сломана
                {
                    broken_type = 1;

                    // Чистим таймеры, так как прочитаны битые байты
                    ExportTime = 0;
                    CreationTime = 0;
                    ModifiedTime = 0;
                }
            }

            OldSize = Data().Length;

            return broken_type;
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(Encoding.Default.GetBytes(Source));
            temp.Add(0);
            temp.AddRange(Encoding.Default.GetBytes(ExportTool));
            temp.Add(0);
            if (!FourByte)
                temp.AddRange(BitConverter.GetBytes(ExportTime));
            else
                temp.AddRange(BitConverter.GetBytes((uint)ExportTime));
            temp.AddRange(Encoding.Default.GetBytes(OwnerName));
            temp.Add(0);
            if (!FourByte)
                temp.AddRange(BitConverter.GetBytes(CreationTime));
            else
                temp.AddRange(BitConverter.GetBytes((uint)CreationTime));
            temp.AddRange(Encoding.Default.GetBytes(ExportModifNameTool));
            temp.Add(0);
            if (!FourByte)
                temp.AddRange(BitConverter.GetBytes(ModifiedTime));
            else
                temp.AddRange(BitConverter.GetBytes((uint)ModifiedTime));

            return temp.ToArray();
        }
    }
}
