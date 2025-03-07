using System;
using System.Collections.Generic;
using System.Text;

namespace OgfTool
{
    public class Bone
    {
        public string Name { get; set; }
        public string ParentName { get; set; }
        public byte[] Fobb { get; set; }

        public List<int> ChildsId { get; private set; }

        public Bone()
        {
            ChildsId = new List<int>();
        }

        public string GetNotNullName() => string.IsNullOrEmpty(Name) ? "noname_bone" : Name;
    }

    public class BoneData
    {
        public long Pos { get; private set; }
        public int OldSize { get; set; }
        public List<Bone> Bones { get; set; }

        public BoneData()
        {
            Pos = 0;
            OldSize = 0;
            Bones = new List<Bone>();
        }

        public int GetBoneID(string bone)
        {
            for (int i = 0; i < Bones.Count; i++)
            {
                if (Bones[i].Name == bone)
                    return i;
            }
            return -1;
        }

        public string GetBoneName(int bone)
        {
            if (Bones.Count > bone)
                return Bones[bone].Name;

            return string.Empty;
        }

        public void RemoveBone(int bone)
        {
            Bones.RemoveAt(bone);
        }

        public void RecalcChilds()
        {
            for (int i = 0; i < Bones.Count; i++)
            {
                Bones[i].ChildsId.Clear();
                for (int j = 0; j < Bones.Count; j++)
                {
                    if (Bones[j].ParentName == Bones[i].Name)
                        Bones[i].ChildsId.Add(j);
                }
            }
        }

        public void Load(XRayLoader xr_loader)
        {
            Pos = xr_loader.ChunkPos;

            uint count = xr_loader.ReadUInt32();

            for (; count != 0; count--)
            {
                Bone bone = new Bone
                {
                    Name = xr_loader.ReadStringZ(),
                    ParentName = xr_loader.ReadStringZ(),
                    Fobb = xr_loader.ReadBytes(60)
                };
                Bones.Add(bone);
            }

            RecalcChilds();

            OldSize = Data(false).Length;
        }

        public byte[] Data(bool repair)
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(BitConverter.GetBytes(Bones.Count));

            for (int i = 0; i < Bones.Count; i++)
            {
                temp.AddRange(Encoding.Default.GetBytes(Bones[i].Name));
                temp.Add(0);
                temp.AddRange(Encoding.Default.GetBytes(Bones[i].ParentName));
                temp.Add(0);

                if (repair)
                {
                    for (int j = 0; j < 60; j++)
                        temp.Add(0);
                }
                else
                    temp.AddRange(Bones[i].Fobb);
            }

            return temp.ToArray();
        }
    }

    public struct BoneRenderTransform
    {
        public float PosX { get; private set; }
        public float PosY { get; private set; }
        public float PosZ { get; private set; }

        public float RotX { get; private set; }
        public float RotY { get; private set; }
        public float RotZ { get; private set; }

        public float OutPosX { get; private set; }
        public float OutPosY { get; private set; }
        public float OutPosZ { get; private set; }

        public float OutRotX { get; private set; }
        public float OutRotY { get; private set; }
        public float OutRotZ { get; private set; }

        public float[] OutPos()
        {
            return new float[3] { OutPosX, OutPosY, OutPosZ };
        }

        public float[] OutRot()
        {
            return new float[3] { OutRotX, OutRotY, OutRotZ };
        }

        public static BoneRenderTransform[] Setup(XRayModel Model, out string child_list)
        {
            BoneRenderTransform[] transforms = new BoneRenderTransform[Model.BoneData.Bones.Count];
            child_list = string.Empty;

            for (int i = 0; i < Model.BoneData.Bones.Count; i++)
            {
                float[] pos, rot; 

                if (Model.IkData.ChunkVersion == 2)
                {
                    pos = Model.IkData.Bones[i].FixedPosition;
                    rot = Model.IkData.Bones[i].FixedRotation;
                }
                else
                {
                    pos = Model.IkData.Bones[i].Position;
                    rot = Model.IkData.Bones[i].Rotation;
                }

                transforms[i].PosX = pos[0];
                transforms[i].PosY = pos[1];
                transforms[i].PosZ = pos[2];

                transforms[i].RotX = rot[0];
                transforms[i].RotY = rot[1];
                transforms[i].RotZ = rot[2];

                if (i != 0)
                    child_list += "-";

                for (int j = 0; j < Model.BoneData.Bones[i].ChildsId.Count; j++)
                    child_list += $"{Model.BoneData.Bones[i].ChildsId[j]},";

                if (!string.IsNullOrEmpty(Model.BoneData.Bones[i].ParentName))
                    child_list += $"{Model.BoneData.GetBoneID(Model.BoneData.Bones[i].ParentName)}";
                else
                    child_list += "9999";
            }

            child_list += "-";

            return transforms;
        }
    }
}
