using System;
using System.Collections.Generic;

namespace OgfTool
{
    public class BBox
    {
        public float[] Min { get; set; }
        public float[] Max { get; set; }

        public BBox()
        {
            Identity();
        }

        public void Load(XRayLoader xr_loader)
        {
            Min = xr_loader.ReadVector();
            Max = xr_loader.ReadVector();
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(FVec.GetBytes(Min));
            temp.AddRange(FVec.GetBytes(Max));

            return temp.ToArray();
        }

        public void Identity()
        {
            Min = new float[3];
            Max = new float[3];
        }

        public void Invalidate()
        {
            Min = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue };
            Max = new float[3] { float.MinValue, float.MinValue, float.MinValue };
        }

        public void Set(float[] min, float[] max)
        {
            Min = min;
            Max = max;
        }

        public void Modify(float[] vec)
        {
            Min = FVec.Min(Min, vec);
            Min = FVec.Min(Max, vec);
        }

        public void Merge(BBox box)
        {
            Modify(box.Min);
            Modify(box.Max);
        }

        public float[] GetCenter()
        {
            float[] C = new float[3];
            for (int i = 0; i < 3; i++)
                C[i] = (Min[i] + Max[i]) * 0.5f;

            return C;
        }

        public void CreateBox(List<SSkelVert> Vertices)
        {
            Invalidate();
            for (int k = 0; k < Vertices.Count; k++)
                Modify(Vertices[k].Offset());
        }

        public List<SSkelVert> GetVisualVerts()
        {
            List<SSkelVert> verts = new List<SSkelVert>();

            float x_diff = Max[0] - Min[0];
            float y_diff = Max[1] - Min[1];
            float z_diff = Max[2] - Min[2];

            // Back start
            {
                SSkelVert vert1 = new SSkelVert(); // 1 Vert
                vert1.Offs = new float[3] { Min[0], Min[1], Min[2] };
                vert1.norm = new float[3] { 0.0f, 0.0f, -1.0f };
                vert1.Uv = new float[2] { 0.0f, 1.0f };

                SSkelVert vert2 = new SSkelVert(); // 2 Vert
                vert2.Offs = new float[3] { Min[0] + x_diff, Min[1], Min[2] };
                vert2.norm = new float[3] { 0.0f, 0.0f, -1.0f };
                vert2.Uv = new float[2] { 1.0f, 1.0f };

                SSkelVert vert3 = new SSkelVert(); // 3 Vert
                vert3.Offs = new float[3] { Min[0], Min[1] + y_diff, Min[2] };
                vert3.norm = new float[3] { 0.0f, 0.0f, -1.0f };
                vert3.Uv = new float[2] { 0.0f, 0.0f };

                SSkelVert vert4 = new SSkelVert(); // 4 Vert
                vert4.Offs = new float[3] { Min[0] + x_diff, Min[1] + y_diff, Min[2] };
                vert4.norm = new float[3] { 0.0f, 0.0f, -1.0f };
                vert4.Uv = new float[2] { 1.0f, 0.0f };

                verts.Add(vert1);
                verts.Add(vert2);
                verts.Add(vert3);
                verts.Add(vert4);
            }
            // Back end 

            // Left start
            {
                SSkelVert vert1 = new SSkelVert(); // 5 Vert
                vert1.Offs = new float[3] { Min[0], Min[1], Min[2] };
                vert1.norm = new float[3] { -1.0f, 0.0f, 0.0f };
                vert1.Uv = new float[2] { 1.0f, 1.0f };

                SSkelVert vert2 = new SSkelVert(); // 6 Vert
                vert2.Offs = new float[3] { Min[0], Min[1] + y_diff, Min[2] };
                vert2.norm = new float[3] { -1.0f, 0.0f, 0.0f };
                vert2.Uv = new float[2] { 1.0f, 0.0f };

                SSkelVert vert3 = new SSkelVert(); // 7 Vert
                vert3.Offs = new float[3] { Min[0], Min[1], Min[2] + z_diff };
                vert3.norm = new float[3] { -1.0f, 0.0f, 0.0f };
                vert3.Uv = new float[2] { 0.0f, 1.0f };

                SSkelVert vert4 = new SSkelVert(); // 8 Vert
                vert4.Offs = new float[3] { Min[0], Min[1] + y_diff, Min[2] + z_diff };
                vert4.norm = new float[3] { -1.0f, 0.0f, 0.0f };
                vert4.Uv = new float[2] { 0.0f, 0.0f };

                verts.Add(vert1);
                verts.Add(vert2);
                verts.Add(vert3);
                verts.Add(vert4);
            }
            // Left end

            // Right start
            {
                SSkelVert vert1 = new SSkelVert(); // 9 Vert
                vert1.Offs = new float[3] { Min[0] + x_diff, Min[1], Min[2] };
                vert1.norm = new float[3] { 1.0f, 0.0f, 0.0f };
                vert1.Uv = new float[2] { 0.0f, 1.0f };

                SSkelVert vert2 = new SSkelVert(); // 10 Vert
                vert2.Offs = new float[3] { Min[0] + x_diff, Min[1], Min[2] + z_diff };
                vert2.norm = new float[3] { 1.0f, 0.0f, 0.0f };
                vert2.Uv = new float[2] { 1.0f, 1.0f };

                SSkelVert vert3 = new SSkelVert(); // 11 Vert
                vert3.Offs = new float[3] { Min[0] + x_diff, Min[1] + y_diff, Min[2] };
                vert3.norm = new float[3] { 1.0f, 0.0f, 0.0f };
                vert3.Uv = new float[2] { 0.0f, 0.0f };

                SSkelVert vert4 = new SSkelVert(); // 12 Vert
                vert4.Offs = new float[3] { Max[0], Max[1], Max[2] };
                vert4.norm = new float[3] { 1.0f, 0.0f, 0.0f };
                vert4.Uv = new float[2] { 1.0f, 0.0f };

                verts.Add(vert1);
                verts.Add(vert2);
                verts.Add(vert3);
                verts.Add(vert4);
            }
            // Right end

            // Front start
            {
                SSkelVert vert1 = new SSkelVert(); // 13 Vert
                vert1.Offs = new float[3] { Min[0], Min[1], Min[2] + z_diff };
                vert1.norm = new float[3] { 0.0f, 0.0f, 1.0f };
                vert1.Uv = new float[2] { 1.0f, 1.0f };

                SSkelVert vert2 = new SSkelVert(); // 14 Vert
                vert2.Offs = new float[3] { Min[0] + x_diff, Min[1], Min[2] + z_diff };
                vert2.norm = new float[3] { 0.0f, 0.0f, 1.0f };
                vert2.Uv = new float[2] { 0.0f, 1.0f };

                SSkelVert vert3 = new SSkelVert(); // 15 Vert
                vert3.Offs = new float[3] { Min[0], Min[1] + y_diff, Min[2] + z_diff };
                vert3.norm = new float[3] { 0.0f, 0.0f, 1.0f };
                vert3.Uv = new float[2] { 1.0f, 0.0f };

                SSkelVert vert4 = new SSkelVert(); // 16 Vert
                vert4.Offs = new float[3] { Max[0], Max[1], Max[2] };
                vert4.norm = new float[3] { 0.0f, 0.0f, 1.0f };
                vert4.Uv = new float[2] { 0.0f, 0.0f };

                verts.Add(vert1);
                verts.Add(vert2);
                verts.Add(vert3);
                verts.Add(vert4);
            }
            // Front end 

            // Up start
            {
                SSkelVert vert1 = new SSkelVert(); // 17 Vert
                vert1.Offs = new float[3] { Min[0], Min[1] + y_diff, Min[2] };
                vert1.norm = new float[3] { 0.0f, 1.0f, 0.0f };
                vert1.Uv = new float[2] { 0.0f, 1.0f };

                SSkelVert vert2 = new SSkelVert(); // 18 Vert
                vert2.Offs = new float[3] { Min[0] + x_diff, Min[1] + y_diff, Min[2] };
                vert2.norm = new float[3] { 0.0f, 1.0f, 0.0f };
                vert2.Uv = new float[2] { 1.0f, 1.0f };

                SSkelVert vert3 = new SSkelVert(); // 19 Vert
                vert3.Offs = new float[3] { Min[0], Min[1] + y_diff, Min[2] + z_diff };
                vert3.norm = new float[3] { 0.0f, 1.0f, 0.0f };
                vert3.Uv = new float[2] { 0.0f, 0.0f };

                SSkelVert vert4 = new SSkelVert(); // 20 Vert
                vert4.Offs = new float[3] { Max[0], Max[1], Max[2] };
                vert4.norm = new float[3] { 0.0f, 1.0f, 0.0f };
                vert4.Uv = new float[2] { 1.0f, 0.0f };

                verts.Add(vert1);
                verts.Add(vert2);
                verts.Add(vert3);
                verts.Add(vert4);
            }
            // Up end 

            // Down start
            {
                SSkelVert vert1 = new SSkelVert(); // 21 Vert
                vert1.Offs = new float[3] { Min[0], Min[1], Min[2] };
                vert1.norm = new float[3] { 0.0f, -1.0f, 0.0f };
                vert1.Uv = new float[2] { 1.0f, 1.0f };

                SSkelVert vert2 = new SSkelVert(); // 22 Vert
                vert2.Offs = new float[3] { Min[0] + x_diff, Min[1], Min[2] };
                vert2.norm = new float[3] { 0.0f, -1.0f, 0.0f };
                vert2.Uv = new float[2] { 0.0f, 1.0f };

                SSkelVert vert3 = new SSkelVert(); // 23 Vert
                vert3.Offs = new float[3] { Min[0], Min[1], Min[2] + z_diff };
                vert3.norm = new float[3] { 0.0f, -1.0f, 0.0f };
                vert3.Uv = new float[2] { 1.0f, 0.0f };

                SSkelVert vert4 = new SSkelVert(); // 24 Vert
                vert4.Offs = new float[3] { Min[0] + x_diff, Min[1], Min[2] + z_diff };
                vert4.norm = new float[3] { 0.0f, -1.0f, 0.0f };
                vert4.Uv = new float[2] { 0.0f, 0.0f };

                verts.Add(vert1);
                verts.Add(vert2);
                verts.Add(vert3);
                verts.Add(vert4);
            }
            // Down end 

            return verts;
        }

        public List<SSkelFace> GetVisualFaces(List<SSkelVert> Verts)
        {
            List<SSkelFace> faces = new List<SSkelFace>();
 
            int VertsCount = GetVisualVerts().Count; // 24
            int[,] FaceVertList = new int[,]
            {
                { 1, 3, 4 },
                { 4, 2, 1 },
                { 7, 8, 6 },
                { 6, 5, 7 },
                { 9, 11, 12 },
                { 12, 10, 9 },
                { 16, 15, 13 },
                { 13, 14, 16 },
                { 17, 19, 20 },
                { 20, 18, 17 },
                { 24, 23, 21 },
                { 21, 22, 24 }
            };

            for (int i = 0; i < FaceVertList.Length / 3; i++)
            {
                SSkelFace face = new SSkelFace();

                for (int j = 0; j < 3; j++)
                {
                    int vert_idx = VertsCount - FaceVertList[i, j] + 1;
                    face.Vertex[j] = (ushort)(Verts.Count - vert_idx);
                }

                faces.Add(face);
            }

            return faces;
        }
    };

    public class BSphere
    {
        public float[] Center { get; set; }
        public float Radius { get; set; }

        public BSphere()
        {
            Identity();
        }

        public void Identity()
        {
            Center = new float[3];
            Radius = 0.0f;
        }

        public void Load(XRayLoader xr_loader)
        {
            Center = xr_loader.ReadVector();
            Radius = xr_loader.ReadFloat();
        }

        public void CreateSphere(BBox box)
        {
            Center = box.GetCenter();
            Radius = FVec.DistanceTo(Center, box.Max);
        }

        public byte[] Data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(FVec.GetBytes(Center));
            temp.AddRange(BitConverter.GetBytes(Radius));

            return temp.ToArray();
        }
    };
}
