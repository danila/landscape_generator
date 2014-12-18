using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct3D11;

namespace WpfApplication2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void RefreshScene()
        {
            device.Clear(70, 70, 70, 255);
            device.Render(cam, lightPos, mesh);
            device.Present();

            LblPolygonsNum.Content = mesh.Polygons.Length.ToString();
            LblVerticiesNum.Content = mesh.Vertices.Length.ToString();
        }
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            device.Clear(70, 70, 70, 255);
            // rotating slightly the cube during each frame rendered
            //mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.05f, mesh.Rotation.Z);
            //if (mesh.Rotation.Y > 2*3.14159265)
            //    mesh.Rotation = new Vector3(mesh.Rotation.X, 0.0f, mesh.Rotation.Z);

            //mesh.Rotation = new Vector3(0.5f, 0.5f, mesh.Rotation.Z);
            //cam.Position = new Vector3(cam.Position.X + 1f, cam.Position.Y + 1f, cam.Position.Z);

            // Doing the various matrix operations
            device.Render(cam, lightPos, mesh);

            // Flushing the back buffer into the front buffer
            device.Present();
        }

        Stopwatch Timer = new Stopwatch();

        public Vector3 lightPos = new Vector3(1500, -700, -1500);
        public static int SEED = 8;
        public static int FILTER = 3;
        public static int DATA_SIZE = 257;  
        public static int MAX_HEIGHT = 128;
        public static double WATER_FACTOR = 0.1;
        public static double ROUGHNESS = 0.6;
        public static bool WATER_FILL = false;
        public double[,] map = new double[DATA_SIZE-1, DATA_SIZE-1];

        public double prevX;
        public double prevY;

        public double maxY = -1e32;
        public double minY = 1e32;

        Random rand = new Random();
        private int RAND_MAX = 0;


        private Engine device;
        Terra mesh;
        Camera cam;

        private Heightmap heightmap;
        
       
        private void GraphicImage_Loaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap((int)1024, (int)700, 96, 96, PixelFormats.Bgra32, null);


            device = new Engine(bmp);
            mesh = new Terra(DATA_SIZE-1, MAX_HEIGHT, WATER_FACTOR);
            cam = new Camera();
            GraphicImage.Source = bmp;

            Timer.Restart();
            
            heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER, WATER_FACTOR, WATER_FILL);
            map = heightmap.Generate(ROUGHNESS);
            mesh.GetVertices(map, DATA_SIZE);
            CameraAdjust();
            Timer.Stop();
            LblTimeGen.Content = Timer.Elapsed.Seconds.ToString() + ":" + Timer.Elapsed.Milliseconds.ToString();

            Timer.Restart();
            RefreshScene();
            Timer.Stop();

            LblTimeDraw.Content = Timer.Elapsed.Seconds.ToString() + ":" + Timer.Elapsed.Milliseconds.ToString();
        }

        private void CameraAdjust()
        {
            int middle = (int)((DATA_SIZE - 1) / 2);
            mesh.Rotation = new Vector3(3.14159265f, 0, 0);
            //cam.Position = new Vector3(mesh.Vertices[middle, middle].Coordinates.X - DATA_SIZE * 2, mesh.Vertices[middle / 2, middle].Coordinates.X + DATA_SIZE * 2, 0f);
            cam.Position = new Vector3(-700.0f, 600f, 0f);
            Vector3 target = new Vector3();
            target.X = mesh.Vertices[middle, middle].Coordinates.X;
            target.Y = -(int)(MAX_HEIGHT / 2);
            target.Z = -mesh.Vertices[middle, middle].Coordinates.Z;
            cam.Target = target;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ROUGHNESS += 0.05;
                heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER, WATER_FACTOR, WATER_FILL);
                map = heightmap.Generate(ROUGHNESS);
                mesh.GetVertices(map, DATA_SIZE);
                RefreshScene();


            }
            if (e.Key == Key.Down)
            {
                ROUGHNESS -= 0.05;
                heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER, WATER_FACTOR, WATER_FILL);
                map = heightmap.Generate(ROUGHNESS);
                mesh.GetVertices(map, DATA_SIZE);
                RefreshScene();

            }
            if (e.Key == Key.Z)
            {
                lightPos.Z += 50;
                RefreshScene();
            }
            if (e.Key == Key.X)
            {
                lightPos.X -= 50;
                RefreshScene();
            }
            if (e.Key == Key.C)
            {
                lightPos.Y += 50;
                RefreshScene();
            }



            if (e.Key == Key.W)
            {
                cam.Position = new Vector3(cam.Position.X + 30.0f, cam.Position.Y, cam.Position.Z);
                RefreshScene();
            }
            if (e.Key == Key.S)
            {
                cam.Position = new Vector3(cam.Position.X - 30.0f, cam.Position.Y, cam.Position.Z);
                RefreshScene();
            }

            if (e.Key == Key.A)
            {
                cam.Position = new Vector3(cam.Position.X, cam.Position.Y, cam.Position.Z - 30.0f);
                RefreshScene();
            }

            if (e.Key == Key.D)
            {
                cam.Position = new Vector3(cam.Position.X, cam.Position.Y, cam.Position.Z + 30.0f);
                RefreshScene();
            }


        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                prevX = e.GetPosition(GraphicImage).X;
                prevY = e.GetPosition(GraphicImage).Y;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var size_index = ComboSize.SelectedIndex;
            if (size_index == 0)
            {
                SEED = 5;
                DATA_SIZE = 33;
            } else if (size_index == 1) {
                SEED = 6;
                DATA_SIZE = 65;
            } else if (size_index == 2) {
                SEED = 7;
                DATA_SIZE = 129;
            } else if (size_index == 3) {
                SEED = 8;
                DATA_SIZE = 257;
            } else if (size_index == 4) {
                SEED = 9;
                DATA_SIZE = 513;
            }

            var aa_index = ComboAntialiasing.SelectedIndex;
            if (aa_index == 0)
                FILTER = -1;
            else
                FILTER = aa_index;

            var rough_index = ComboRoughness.SelectedIndex;
            if (rough_index == 0) {
                ROUGHNESS = 0.8;
            } else if (rough_index == 1) {
                ROUGHNESS = 0.5;
            } else if (rough_index == 2) {
                ROUGHNESS = 0.8;
            }

            var water_level = ComboWaterlevel.SelectedIndex;
            if (water_level == 0) {
                WATER_FACTOR = 0.1;
            } else if (water_level == 1) {
                WATER_FACTOR = 0.3;
            } else if (water_level == 2) {
                WATER_FACTOR = 0.6;
            }

            if (CheckFill.IsChecked == true)
                WATER_FILL = true;
            else
                WATER_FILL = false;


            mesh = new Terra(DATA_SIZE - 1, MAX_HEIGHT, WATER_FACTOR);

            Timer.Restart();
            heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER, WATER_FACTOR, WATER_FILL);
            map = heightmap.Generate(ROUGHNESS);
            mesh.GetVertices(map, DATA_SIZE);
            CameraAdjust();
            Timer.Stop();
            LblTimeGen.Content = Timer.Elapsed.Seconds.ToString() + ":" + Timer.Elapsed.Milliseconds.ToString();

            Timer.Restart();
            RefreshScene();
            Timer.Stop();
            LblTimeDraw.Content = Timer.Elapsed.Seconds.ToString() + ":" + Timer.Elapsed.Milliseconds.ToString();

        }





    }
}
