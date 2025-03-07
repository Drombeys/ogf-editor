using System;
using System.Collections.Generic;
using System.Text;

namespace OgfTool
{
    public class IKBone
    {
        public string Material { get; set; }
        public float Mass { get; set; }
        public uint Version { get; set; }
        public float[] CenterMass { get; set; }
        public float[] Position { get; set; }
        public float[] FixedPosition { get; set; }
        public float[] FixedRotation { get; set; }
        public float[] Rotation { get; set; }
        public byte[] KinematicData { get; set; }
        public float[] RenderTransform { get; set; }

        public IKBone()
        {
            Version = 0;
            Mass = 0.0f;
            RenderTransform = new float[3];
            FixedPosition = new float[3];
            FixedRotation = new float[3];
        }
    }

    public class IKData
    {
        public long Pos { get; set; }
        public int OldSize { get; set; }
        public byte ChunkVersion { get; set; }

        public List<IKBone> Bones { get; set; }

        public IKData()
        {
            Pos = 0;
            OldSize = 0;
            ChunkVersion = 0;
            Bones = new List<IKBone>();
        }

        public void RemoveBone(int bone)
        {
            Bones.RemoveAt(bone);
        }

        public void Load(XRayLoader xr_loader, int bones_count, byte chunk_ver)
        {
            Pos = xr_loader.ChunkPos;
            ChunkVersion = chunk_ver;

            for (int i = 0; i < bones_count; i++)
            {
                IKBone bone = new IKBone();

                List<byte> kinematic_data = new List<byte>();

                byte[] temp_byte;

                if (ChunkVersion == 4)
                    bone.Version = xr_loader.ReadUInt32();

                bone.Material = xr_loader.ReadStringZ();

                temp_byte = xr_loader.ReadBytes(112);   // struct SBoneShape
                kinematic_data.AddRange(temp_byte);

                int ImportBytes = ((ChunkVersion == 4) ? 76 : ((ChunkVersion == 3) ? 72 : 60));
                temp_byte = xr_loader.ReadBytes(ImportBytes); // Import
                kinematic_data.AddRange(temp_byte);

                bone.KinematicData = kinematic_data.ToArray();

                bone.Rotation = xr_loader.ReadVector();
                bone.Position = xr_loader.ReadVector();

                bone.Mass = xr_loader.ReadFloat();
                bone.CenterMass = xr_loader.ReadVector();

                Bones.Add(bone);
            }

            OldSize = Data().Length;
        }

        public uint ChunkID(byte vers)
        {
            switch (ChunkVersion)
            {
                case 4:
                    return (vers == 4 ? (uint)OGF.OGF4_S_IKDATA : (uint)OGF.OGF3_S_IKDATA_2);
                case 3:
                    return (uint)OGF.OGF3_S_IKDATA;
                case 2:
                    return (uint)OGF.OGF3_S_IKDATA_0;
            }

            return 4;
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            for (int i = 0; i < Bones.Count; i++)
            {
                if (ChunkVersion == 4)
                    temp.AddRange(BitConverter.GetBytes(Bones[i].Version));

                temp.AddRange(Encoding.Default.GetBytes(Bones[i].Material));
                temp.Add(0);

                temp.AddRange(Bones[i].KinematicData);

                temp.AddRange(BitConverter.GetBytes(Bones[i].Rotation[0]));
                temp.AddRange(BitConverter.GetBytes(Bones[i].Rotation[1]));
                temp.AddRange(BitConverter.GetBytes(Bones[i].Rotation[2]));

                temp.AddRange(BitConverter.GetBytes(Bones[i].Position[0]));
                temp.AddRange(BitConverter.GetBytes(Bones[i].Position[1]));
                temp.AddRange(BitConverter.GetBytes(Bones[i].Position[2]));

                temp.AddRange(BitConverter.GetBytes(Bones[i].Mass));

                temp.AddRange(BitConverter.GetBytes(Bones[i].CenterMass[0]));
                temp.AddRange(BitConverter.GetBytes(Bones[i].CenterMass[1]));
                temp.AddRange(BitConverter.GetBytes(Bones[i].CenterMass[2]));
            }

            return temp.ToArray();
        }
    }
}
