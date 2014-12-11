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
            device.Render(cam, mesh);
            device.Present();
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
            device.Render(cam, mesh);

            // Flushing the back buffer into the front buffer
            device.Present();
        }




        public static int SEED = 8;
        public static int FILTER = 3;
        public static int DATA_SIZE = 257;
        public static int MAX_HEIGHT = 128;
        public static double ROUGHNESS = 0.6;
        public double[,] map = new double[DATA_SIZE-1, DATA_SIZE-1];
        public static bool allowed = true;

        public double prevX;
        public double prevY;

        public double maxY = -1e32;
        public double minY = 1e32;

        Random rand = new Random();
        private int RAND_MAX = 0;


        private Device device;
        Mesh mesh;
        Camera cam;

        private Heightmap heightmap;
        
       
        private void GraphicImage_Loaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap((int)1024, (int)700, 96, 96, PixelFormats.Bgra32, null);


            device = new Device(bmp);
            mesh = new Mesh(DATA_SIZE-1);
            cam = new Camera();
            GraphicImage.Source = bmp;
            heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER);
            map = heightmap.Generate(ROUGHNESS);
            mesh.GetVertices(map, DATA_SIZE);
            CameraAdjust();


            //CompositionTarget.Rendering += CompositionTarget_Rendering;
            RefreshScene();
        }

        private void CameraAdjust()
        {
            mesh.Rotation = new Vector3(3.14159265f, 0, 0);
            cam.Position = new Vector3(-700.0f, 500f, 0f);
            Vector3 target = new Vector3();
            target.X = mesh.Vertices[(int)Math.Pow(2, SEED) / 2, (int)Math.Pow(2, SEED) / 2].X;
            target.Y = -(int)(MAX_HEIGHT / 2);
            target.Z = -mesh.Vertices[(int)Math.Pow(2, SEED) / 2, (int)Math.Pow(2, SEED) / 2].Z;
            cam.Target = target;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ROUGHNESS += 0.05;
                heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER);
                map = heightmap.Generate(ROUGHNESS);
                mesh.GetVertices(map, DATA_SIZE);
                RefreshScene();


            }
            if (e.Key == Key.Down)
            {
                ROUGHNESS -= 0.05;
                heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER);
                map = heightmap.Generate(ROUGHNESS);
                mesh.GetVertices(map, DATA_SIZE);
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
            if (rough_index == 0)
            {
                ROUGHNESS = 0.1;
            } else if (rough_index == 1) {
                ROUGHNESS = 0.6;
            } else if (rough_index == 2) {
                ROUGHNESS = 0.9;
            }
            
            mesh = new Mesh(DATA_SIZE - 1);
            heightmap = new Heightmap(SEED, MAX_HEIGHT, FILTER);
            map = heightmap.Generate(ROUGHNESS);
            mesh.GetVertices(map, DATA_SIZE);
            CameraAdjust();


            //CompositionTarget.Rendering += CompositionTarget_Rendering;
            RefreshScene();

        }


    }
}
