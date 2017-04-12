using GrowthModelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Visualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Random rand = new Random();
        Infection i = new Infection(new ModelParameter() { BacteriaDoublingTime = 1 });
        Dictionary<Bacteria, Ellipse> mDrawList = new Dictionary<Bacteria, Ellipse>();
        int bactCount = 0;

        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }


        void CompositionTarget_Rendering(object sender, System.EventArgs e)
        {
            //theCanvas.Children.Clear();

            i.Grow();
            if(i.Count > bactCount)
            {                
                foreach (Bacteria b in i)
                {
                    if (!mDrawList.ContainsKey(b))
                    {
                        var myEllipse = new Ellipse();
                        myEllipse.Width = 5;
                        myEllipse.Height = 5;
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush();
                        mySolidColorBrush.Color = Colors.Black;
                        myEllipse.Fill = mySolidColorBrush;
                        this.mDrawList.Add(b, myEllipse);
                        theCanvas.Children.Add(myEllipse);
                    }
                }
            }
            foreach (KeyValuePair<Bacteria, Ellipse> entry in mDrawList)
            {
                Canvas.SetLeft(entry.Value, entry.Key.X + entry.Key.Dimension.Xmax);
                Canvas.SetTop(entry.Value, entry.Key.Y + entry.Key.Dimension.Ymax);
            }
        }
    }
}

