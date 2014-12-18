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

    public struct Vertex
    {
        public Vector3 Normal
        { get; set; }

        public Vector3 Coordinates;
        public Vector3 WorldCoordinates;
    }
    public class Polygon
    {
        public Vertex A;
        public Vertex B;
        public Vertex C;

        public Polygon() { }
        public Polygon(Vertex a, Vertex b, Vertex c)
        {
            A = a;
            B = b;
            C = c;
        }
    }

    public class Terra : Polygon
    {
        public int MaxHeight;
        public double WaterFactor;
        public Vertex[,] Vertices { get; private set; }
        public Polygon[] Polygons { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Terra(int size, int maxHeight, double waterFactor)
        {
            Vertices = new Vertex[size, size];
            Polygons = new Polygon[(size-1) * (size-1) * 2];
            MaxHeight = maxHeight;
            WaterFactor = waterFactor;
        }



        internal void GetVertices(double[,] map, int size)
        {
            for (int x = 0; x < size - 1; ++x)
            {
                for (int y = 0; y < size - 1; ++y)
                {
                    //populate the point struct
                    Vertices[x, y].Coordinates.X = x;
                    Vertices[x, y].Coordinates.Y = (float)map[x, y];
                    Vertices[x, y].Coordinates.Z = y;
                    Vertices[x, y].Normal = Vector3.Normalize(Vertices[x, y].Coordinates);
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
                    if (i == size / 2 && j == size / 2)
                    { }
                    Polygon temp = new Polygon();
                    temp.A = Vertices[i, j];
                    temp.B = Vertices[i, j + 1];
                    temp.C = Vertices[i + 1, j];
                    Polygons[k] = temp;
                    ++k;

                    Polygon temp1 = new Polygon();
                    temp1.A = Vertices[i, j + 1];
                    temp1.B = Vertices[i + 1, j + 1];
                    temp1.C = Vertices[i + 1, j];
                    Polygons[k] = temp1;
                    ++k;
                }
            }
        }
    }

}
