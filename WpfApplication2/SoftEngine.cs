using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace WpfApplication2
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
    }

    public class Polygon
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public Vector3 Vertices { get; private set; }
        public Polygon() { }
        public Polygon(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a;
            B = b;
            C = c;
        }

    }

    public class Mesh
    {
        public Vector3[,] Vertices { get; private set; }
        public Polygon[] Polygons { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(int size)
        {
            Vertices = new Vector3[size, size];
            Polygons = new Polygon[(size-1) * (size-1) * 2];
        }

        internal void GetVertices(double[,] map, int size)
        {
            for (int x = 0; x < size - 1; ++x)
            {
                for (int y = 0; y < size - 1; ++y)
                {
                    //populate the point struct
                    Vertices[x, y].X = x;
                    Vertices[x, y].Y = (float)map[x, y];
                    Vertices[x, y].Z = y;
                }
            }

            GetTriangles(size);
        }

        internal void GetTriangles(int size)
        {
            int k = 0;
            for (int i = 0; i < size - 2; ++i)
            {
                for (int j = 0; j < size - 2; ++j)
                {
                    Polygon temp = new Polygon();
                    temp.A = Vertices[i, j];
                    temp.B = Vertices[i + 1, j];
                    temp.C = Vertices[i, j + 1];
                    Polygons[k] = temp;
                    ++k;

                    Polygon temp1 = new Polygon();
                    temp1.A = Vertices[i, j + 1];
                    temp1.B = Vertices[i + 1, j];
                    temp1.C = Vertices[i + 1, j + 1];
                    Polygons[k] = temp1;
                    ++k;
                }
            }
        }
    }

}
