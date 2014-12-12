using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using SharpDX;
using Color = System.Drawing.Color;

namespace WpfApplication2
{

    public struct ScanLineData
    {
        public int currentY;
        public float ndotla;
        public float ndotlb;
        public float ndotlc;
        public float ndotld;
    }
    public class Device
    {
        private byte[] backBuffer;
        private readonly float[] depthBuffer;
        private WriteableBitmap bmp;
        private readonly int renderWidth;
        private readonly int renderHeight;

        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            renderWidth = bmp.PixelWidth;
            renderHeight = bmp.PixelHeight;
            // Размер заднего буфера равен количеству пикселей на
            // экране (width*height) * 4 (R,G,B & Alpha values). 
            backBuffer = new byte[bmp.PixelWidth*bmp.PixelHeight*4];
            depthBuffer = new float[bmp.PixelWidth * bmp.PixelHeight];

        }

        // Заливка заднего буфера заданным цветом
        public void Clear(byte r, byte g, byte b, byte a)
        {
            // Clearing Back Buffer
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                // BGRA is used by Windows instead by RGBA in HTML5
                backBuffer[index] = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }

            // Clearing Depth Buffer
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
            }
        }

        // Put the back buffer into the front buffer. 
        public void Present()
        {
            bmp.WritePixels(new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight), backBuffer, bmp.PixelWidth*4, 0);
        }


        // Put a pixel on screen at a specific X,Y coordinates
        public void PutPixel(int x, int y, float z, Color4 color)
        {
            // Задний буфер - одноразмерный массив
            // Экран - двуразмерный, поэтому нужно посчитать
            // Индекс элемента в одноразмерном массиве
            var index = (x + y * renderWidth);
            var index4 = index * 4;

            if (depthBuffer[index] < z)
            {
                return; // Discard
            }

            depthBuffer[index] = z;

            backBuffer[index4] = (byte)(color.Blue * 255);
            backBuffer[index4 + 1] = (byte)(color.Green * 255);
            backBuffer[index4 + 2] = (byte)(color.Red * 255);
            backBuffer[index4 + 3] = (byte)(color.Alpha * 255);
        }

        //public void DrawLine(Vector2 point0, Vector2 point1, SharpDX.Color color)
        //{
        //    var x0 = (int) point0.X;
        //    var y0 = (int) point0.Y;
        //    var x1 = (int) point1.X;
        //    var y1 = (int) point1.Y;

        //    int dx = Math.Abs(x1 - x0);
        //    int dy = Math.Abs(y1 - y0);
        //    int sx = (x0 < x1) ? 1 : -1;
        //    int sy = (y0 < y1) ? 1 : -1;
        //    int err = dx - dy;

        //    while (true)
        //    {
        //        DrawPoint(new Vector2(x0, y0), color);

        //        if ((x0 == x1) && (y0 == y1)) break;
        //        int e2 = 2*err;
        //        if (e2 > -dy)
        //        {
        //            err -= dy;
        //            x0 += sx;
        //        }
        //        if (e2 < dx)
        //        {
        //            err += dx;
        //            y0 += sy;
        //        }
        //    }
        //}

        // Project takes some 3D coordinates and transform them
        // in 2D coordinates using the transformation matrix

        public Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            // transforming the coordinates into 2D space
            var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transMat);
            // transforming the coordinates & the normal to the vertex in the 3D world
            var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, world);
            var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, world);

            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point2d.X * renderWidth + renderWidth / 2.0f;
            var y = -point2d.Y * renderHeight + renderHeight / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point2d.Z),
                Normal = normal3dWorld,
                WorldCoordinates = point3dWorld
            };
        }


        // Высвечивание пикселя
        public void DrawPoint(Vector3 point, Color4 color)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
            {
                // Drawing a point
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        //public void DrawTriangle(Polygon triangle, Matrix transformMatrix, SharpDX.Color color)
        //{
        //    Vector2 pointA = Project(triangle.A, transformMatrix);
        //    Vector2 pointB = Project(triangle.B, transformMatrix);
        //    Vector2 pointC = Project(triangle.C, transformMatrix);


        //    DrawLine(pointA, pointB, color);
        //    DrawLine(pointB, pointC, color);
        //    DrawLine(pointC, pointA, color);
        //}

        
        // Clamping values to keep them between 0 and 1
        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color4 color)
        {
            Vector3 pa = va.Coordinates;
            Vector3 pb = vb.Coordinates;
            Vector3 pc = vc.Coordinates;
            Vector3 pd = vd.Coordinates;

            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var ndotl = data.ndotla;
                // changing the color value using the cosine of the angle
                // between the light vector and the normal vector
                Color4 setColor = new Color4();
                setColor.Alpha = color.Alpha;
                setColor.Red = color.Red * ndotl;
                setColor.Green = color.Green * ndotl;
                setColor.Blue = color.Blue * ndotl;
                DrawPoint(new Vector3(x, data.currentY, z), setColor);
            }
        }

        float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }

        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Color4 color, Vector3 lightPos)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            Vector3 p1 = v1.Coordinates;
            Vector3 p2 = v2.Coordinates;
            Vector3 p3 = v3.Coordinates;

            // normal face's vector is the average normal between each vertex's normal
            // computing also the center point of the face
            Vector3 vnFace = (v1.Normal + v2.Normal + v3.Normal) / 3;
            Vector3 centerPoint = (v1.WorldCoordinates + v2.WorldCoordinates + v3.WorldCoordinates) / 3;
            // Light position 
            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            float ndotl = ComputeNDotL(centerPoint, vnFace, lightPos);

            var data = new ScanLineData { ndotla = ndotl };

            // computing lines' directions
            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            // Computing slopes
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        ProcessScanLine(data, v1, v3, v1, v2, color);
                    }
                    else
                    {
                        ProcessScanLine(data, v1, v3, v2, v3, color);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        ProcessScanLine(data, v1, v2, v1, v3, color);
                    }
                    else
                    {
                        ProcessScanLine(data, v2, v3, v1, v3, color);
                    }
                }
            }
        }

        // The main method of the engine that re-compute each vertex projection
        // during each frame
        public void Render(Camera camera, Vector3 lightPos, params Mesh[] meshes)
        {
            // To understand this part, please read the prerequisites resources
            Matrix viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            Matrix projectionMatrix = Matrix.PerspectiveFovRH(0.78f,
                (float) bmp.PixelWidth/bmp.PixelHeight,
                0.01f, 1.0f);

            foreach (Mesh mesh in meshes)
            {
                // Beware to apply rotation before translation 
                Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y,
                    mesh.Rotation.X, mesh.Rotation.Z)*
                                     Matrix.Translation(mesh.Position);

                Matrix transformMatrix = worldMatrix*viewMatrix*projectionMatrix;

                SharpDX.Color terrain = new SharpDX.Color();
                var triangleIndex = 0;


                foreach (var triangle in mesh.Polygons)
                {
                    var vertexA = triangle.A;
                    var vertexB = triangle.B;
                    var vertexC = triangle.C;

                    var pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    var pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    var pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    if (triangleIndex == 70000) 
                    { };

                    if (triangle.A.Coordinates.Y <= 10)
                        terrain = SharpDX.Color.Blue;
                    else if (triangle.A.Coordinates.Y < 40)
                        terrain = SharpDX.Color.RoyalBlue;
                    else if (triangle.A.Coordinates.Y < 50)
                        terrain = SharpDX.Color.Olive;
                    else if (triangle.A.Coordinates.Y < 80)
                        terrain = SharpDX.Color.DarkGreen;
                    else if (triangle.A.Coordinates.Y < 120)
                        terrain = SharpDX.Color.Chocolate;
                    else if (triangle.A.Coordinates.Y < 240)
                        terrain = SharpDX.Color.Moccasin;

                    else
                        terrain = SharpDX.Color.Green;

                    //var color = 0.25f + (triangleIndex % mesh.Polygons.Length) * 0.75f / mesh.Polygons.Length;

                        DrawTriangle(pixelA, pixelB, pixelC, terrain, lightPos);
                    
                        
                    triangleIndex++;
                }





                //SharpDX.Color terrain = new SharpDX.Color();
                //foreach (Polygon triangle in mesh.Polygons)
                //{
                //   if (triangle.A.Y <= 10)
                //        terrain = SharpDX.Color.Blue;
                //    else if (triangle.A.Y < 40)
                //        terrain = SharpDX.Color.RoyalBlue;
                //    else if (triangle.A.Y < 50)
                //        terrain = SharpDX.Color.Olive;
                //    else if (triangle.A.Y < 80)
                //        terrain = SharpDX.Color.DarkGreen;
                //    else if (triangle.A.Y < 120)
                //        terrain = SharpDX.Color.Moccasin;
                //    else if (triangle.A.Y < 240)
                //        terrain = SharpDX.Color.Chocolate;
                //    else
                //        terrain = SharpDX.Color.Green;


                //    DrawTriangle(triangle, transformMatrix, terrain);
                //}

                //foreach (Vector3 vertex in mesh.Vertices)
                //{
                //    // First, we project the 3D coordinates into the 2D space
                //    Vector2 point = Project(vertex, transformMatrix);
                //    SharpDX.Color terrain = new SharpDX.Color();

                //    if (vertex.Y <= 10)
                //        terrain = SharpDX.Color.Blue;
                //    else if (vertex.Y < 40)
                //        terrain = SharpDX.Color.RoyalBlue;
                //    else if (vertex.Y < 50)
                //        terrain = SharpDX.Color.Olive;
                //    else if (vertex.Y < 80)
                //        terrain = SharpDX.Color.DarkGreen;
                //    else if (vertex.Y < 120)
                //        terrain = SharpDX.Color.Moccasin;
                //    else if (vertex.Y < 240)
                //        terrain = SharpDX.Color.Chocolate;
                //    else
                //        terrain = SharpDX.Color.Green;
                //    // Then we can draw on screen
                //    DrawPoint(point, terrain);
                //}

                //for (var i = 1; i < mesh.Vertices.Length - 2; i++)
                //{
                //    var point0 = Project(mesh.Vertices[i], transformMatrix);
                //    var point1 = Project(mesh.Vertices[i + 1], transformMatrix);
                //    if (mesh.Vertices[i].X < 1 && mesh.Vertices[i].Y < 1 || mesh.Vertices[i + 1].X < 1 && mesh.Vertices[i + 1].Y < 1)
                //        continue;

                //    DrawLine(point0, point1);

                //}
            }
        }
    }
}