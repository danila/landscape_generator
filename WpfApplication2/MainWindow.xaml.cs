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

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            device.Clear(0, 0, 0, 255);

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
        public static int FILTER = 2;
        public static int DATA_SIZE = 257;
        public static int MAX_HEIGHT = 32;
        public static double ROUGHNESS = 1;
        public double[,] map = new double[DATA_SIZE-1, DATA_SIZE-1];

        public double prevX;
        public double prevY;

        public double maxY = -1e32;
        public double minY = 1e32;

        Random rand = new Random();
        private int RAND_MAX = 0;


        private Device device;
        Mesh mesh = new Mesh(DATA_SIZE-1);
        Camera cam = new Camera();

        private Heightmap heightmap;
        
       
        private void GraphicImage_Loaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bmp = new WriteableBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96, 96, PixelFormats.Bgra32, null);

            device = new Device(bmp);

            GraphicImage.Source = bmp;

            heightmap = new Heightmap(SEED, MAX_HEIGHT, (int)Math.Pow(2, FILTER) + 1);
            map = heightmap.Generate(ROUGHNESS);
            mesh.GetVertices(map, DATA_SIZE);
            mesh.Rotation = new Vector3(3.14159265f, 0, 0);
            cam.Position = new Vector3(-550.0f, 500f, 0f);
            Vector3 target = new Vector3();
            target.X = mesh.Vertices[(int)Math.Pow(2, SEED) / 2, (int)Math.Pow(2, SEED) / 2].X;
            target.Y = 0;
            target.Z = -mesh.Vertices[(int)Math.Pow(2, SEED) / 2, (int)Math.Pow(2, SEED) / 2].Z;
            cam.Target = target;

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ROUGHNESS += 0.05;
                heightmap = new Heightmap(SEED, DATA_SIZE, (int)Math.Pow(2, FILTER) + 1);
                map = heightmap.Generate(ROUGHNESS);
                mesh.GetVertices(map, DATA_SIZE);

            }
            if (e.Key == Key.Down)
            {
                ROUGHNESS -= 0.05;
                heightmap = new Heightmap(SEED, DATA_SIZE, (int)Math.Pow(2, FILTER) + 1);
                map = heightmap.Generate(ROUGHNESS);
                mesh.GetVertices(map, DATA_SIZE);
            }

            if (e.Key == Key.W)
                cam.Position = new Vector3(cam.Position.X + 30.0f, cam.Position.Y, cam.Position.Z);
            if (e.Key == Key.S)
                cam.Position = new Vector3(cam.Position.X - 30.0f, cam.Position.Y, cam.Position.Z);


        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                prevX = e.GetPosition(GraphicImage).X;
                prevY = e.GetPosition(GraphicImage).Y;
            }
        }


    }
}
