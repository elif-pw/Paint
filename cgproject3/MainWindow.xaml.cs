using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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



namespace cgproject3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point? initialPoint = null;
        private Point? finalPoint=null;
        private Point? thirdPoint = null;
        DrawingLogic logic;


        public MainWindow()
        {
            InitializeComponent();
            logic = new DrawingLogic((int)this.Width, (int)this.Height);
            this.DataContext = logic;
           
        }

        private void DrawingArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this.AreaLabel);
            if (logic.MoveObjectMode)
            {
                if (logic.LinesMode)
                    logic.move_item_line((int)p.X, (int)p.Y);
                else if (logic.PolyMode)
                    logic.move_item_poly((int)p.X, (int)p.Y);
                else if (logic.RecMode)
                    logic.move_item_rec((int)p.X, (int)p.Y);
            }
            else if (logic.MoveVertexMode)
            {
                if (logic.LinesMode)
                    logic.move_vertex_line((int)p.X, (int)p.Y);
                else if (logic.PolyMode)
                    logic.move_vertex_poly((int)p.X, (int)p.Y);
                else if (logic.RecMode)
                    logic.move_vertex_rec((int)p.X, (int)p.Y);
            }
            else if (logic.MoveEdgeMode)
            {
                if (logic.RecMode)
                    logic.move_edge_rec((int)p.X, (int)p.Y);
                else if (logic.PolyMode)
                    logic.move_edge_poly((int)p.X, (int)p.Y);
               
            }
            else if (initialPoint == null)
            {
                initialPoint = p;
            }
            else
            {
               if (logic.RecMode)
               {
                    if (finalPoint == null)
                    {
                        finalPoint = p;
                        logic.drawRectangle((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);
                        logic.shapes_recs.Add(new MyRectangle((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y, logic.Thickness, false, logic.colorR, logic.colorG, logic.colorB));
                        initialPoint = null;
                        finalPoint = null;
                        thirdPoint = null;
                    }
               }

                else if (logic.CapMode)
                {
                    if (finalPoint == null)
                    {
                        finalPoint = p;
                    }
                    else if (finalPoint != null && thirdPoint == null)
                    {
                        thirdPoint = p;
                        logic.drawCapsule((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y, (int)thirdPoint?.X, (int)thirdPoint?.Y);
                        initialPoint = null;
                        finalPoint = null;
                        thirdPoint = null;
                    }

                }

                else if (logic.LinesMode && logic.AntiAliased == false)
                {
                    finalPoint = p;
                    logic.DrawLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);
                    logic.shapes_lines.Add(new MyLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y, logic.Thickness, false, logic.colorR, logic.colorG, logic.colorB));

                    initialPoint = null;
                    finalPoint = null;
                }
                else if (logic.LinesMode && logic.AntiAliased)
                {
                    finalPoint = p;
                    logic.DrawAntiAliasedLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);
                    logic.shapes_lines.Add(new MyLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y, logic.Thickness, true, logic.colorR, logic.colorG, logic.colorB));

                    initialPoint = null;
                    finalPoint = null;
                }
                else if (logic.CirclesMode)
                {
                    finalPoint = p;
                    logic.DrawCircle((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);
                    initialPoint = null;
                    finalPoint = null;
                }

                else if (logic.PolyMode)
                {
                    finalPoint = p;
                    if (logic.PolyFirstX == null)
                    {
                        logic.shapes_polygons.Add(new Polygon((int)initialPoint?.X, (int)initialPoint?.Y, logic.Thickness, logic.AntiAliased, logic.colorR, logic.colorG, logic.colorB));
                        logic.DrawLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);

                        logic.PolyFirstX = (int)initialPoint?.X;
                        logic.PolyFirstY = (int)initialPoint?.Y;
                        initialPoint = finalPoint;
                        finalPoint = null;
                    }
                    else if (logic.PolyFirstX + 15 > (int)finalPoint?.X
                        && (int)finalPoint?.X > logic.PolyFirstX - 15 && logic.PolyFirstY + 15 > (int)finalPoint?.Y
                        && (int)finalPoint?.Y > logic.PolyFirstY - 15)
                    {

                        logic.DrawLine((int)initialPoint?.X, (int)initialPoint?.Y, logic.PolyFirstX.Value, logic.PolyFirstY.Value);
                        logic.shapes_polygons[logic.shapes_polygons.Count - 1].add((int)initialPoint?.X, (int)initialPoint?.Y);
                        logic.PolyFirstX = null;
                        logic.PolyFirstY = null;
                        initialPoint = null;
                        finalPoint = null;

                    }
                    else
                    {
                        logic.DrawLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);
                        logic.shapes_polygons[logic.shapes_polygons.Count - 1].add((int)initialPoint?.X, (int)initialPoint?.Y);
                        initialPoint = finalPoint;
                        finalPoint = null;
                    }

                }
                else if (logic.ClipPolyMode)
                {
                    finalPoint = p;
                    if (logic.PolyFirstX == null) // start
                    {
                        logic.clippingPolygon = new Polygon((int)initialPoint?.X, (int)initialPoint?.Y, logic.Thickness, logic.AntiAliased, logic.colorR, logic.colorG, logic.colorB);
                        logic.DrawLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);

                        logic.PolyFirstX = (int)initialPoint?.X;
                        logic.PolyFirstY = (int)initialPoint?.Y;
                        initialPoint = finalPoint;
                        finalPoint = null;
                    }
                    else if (logic.PolyFirstX + 15 > (int)finalPoint?.X
                        && (int)finalPoint?.X > logic.PolyFirstX - 15 && logic.PolyFirstY + 15 > (int)finalPoint?.Y
                        && (int)finalPoint?.Y > logic.PolyFirstY - 15)
                    {
                        //final point
                        logic.DrawLine((int)initialPoint?.X, (int)initialPoint?.Y, logic.PolyFirstX.Value, logic.PolyFirstY.Value);
                        logic.clippingPolygon.add((int)initialPoint?.X, (int)initialPoint?.Y);
                        logic.PolyFirstX = null;
                        logic.PolyFirstY = null;
                        initialPoint = null;
                        finalPoint = null;
                        if (logic.PolygonIsConvex() && logic.shapes_polygons.Count != 0)
                            logic.clip();
                        else if (logic.shapes_polygons.Count == 0)
                        {
                            MessageBox.Show("Before clipping draw a ploygon to clip!");
                        }
                        else
                            MessageBox.Show("Clipping polygon must be convex!");

                    }
                    else //middle edges
                    {
                        logic.DrawLine((int)initialPoint?.X, (int)initialPoint?.Y, (int)finalPoint?.X, (int)finalPoint?.Y);
                        logic.clippingPolygon.add((int)initialPoint?.X, (int)initialPoint?.Y);
                        initialPoint = finalPoint;
                        finalPoint = null;
                    }

                }
            }

        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Regex regex = new Regex("[^0-9]{1,3}");
            //e.Handled = regex.IsMatch(e.Text);
            int output;
            if (int.TryParse(e.Text, out output))
            {
                if (int.Parse(e.Text) > 255 || int.Parse(e.Text) < 0)
                    MessageBox.Show("Enter only values 0-255!");

            }
            else
                MessageBox.Show("Enter only values 0-255!");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            logic = new DrawingLogic((int)this.Width, (int)this.Height);
            logic.shapes_circles = new List<Circle>();
            logic.shapes_polygons = new List<Polygon>();
            logic.shapes_lines = new List<MyLine>();
           // logic.shapes = new List<Shape>();
            this.DataContext = logic;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }


        //saving all shapes
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < logic.shapes_lines.Count; i++)
            {

                sb.Append(JsonConvert.SerializeObject(logic.shapes_lines[i]));
                sb.Append(", ");
            }
            for (int i = 0; i < logic.shapes_circles.Count; i++)
            {

                sb.Append(JsonConvert.SerializeObject(logic.shapes_circles[i]));
                sb.Append(", ");
            }
            for (int i = 0; i < logic.shapes_polygons.Count; i++)
            {

                sb.Append(JsonConvert.SerializeObject(logic.shapes_polygons[i]));
                sb.Append(", ");
            }
            for (int i = 0; i < logic.shapes_recs.Count; i++)
            {

                sb.Append(JsonConvert.SerializeObject(logic.shapes_recs[i]));
                sb.Append(", ");
            }
            sb.Append("]");
            string path = @"C:\Users\Elif\source\repos\lab4\cgproject4\cgproject3\file1.json";

            using (var tw = new StreamWriter(path))
            {
                tw.WriteLine(sb.ToString());
                tw.Close();
            }


        }

        // deleting lines
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            logic.delete_lines();
        }
        //deleting circles
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            logic.delete_cicles();
        }

        //deleting polygons
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            logic.delete_polys();
        }
        //loading
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            logic.load_shapes();
        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("1. To create, edit vertex/edge, move, remove an object choose appropriate mode in the configurations(right side of the window)." +
                "\n2. To choose editing a vertex, moving or removing object on right click context menu right click near a vertex of the object. After that choose the desired option and left click to the new place. " +
                "\n3. For editing edge right click near an edge of the object and choose 'edit edge' option. Click one more time for the desired new position of the edge."+
                "\n4. Before choosing 'change thickness' or 'fill polygon' on the right click set it up in the configurations. "
                +"\n5. For filling a polygon right click near an edge or vertex of the polygon and choose filling way in the context menu."
                +"\n6. Clipping region automatically clips the last drawn polygon." +
                "\n7. The image for filling a polygon should be as big as the drawing area(all the bitmap).", "Instructions", MessageBoxButton.OK);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (logic.AntiAliased)
            {
                logic.apply_antial();
            }
        
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if(logic.AntiAliased==false)
                logic.remove_antial();
        }

        private void AreaLabel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this.AreaLabel);
            logic.EditPoint = new System.Drawing.Point((int)p.X,(int) p.Y);
          
        }

        private void ChanegeThickness(object sender, RoutedEventArgs e)
        {
            if (logic.LinesMode)
                logic.changeThicknessLine();
            else if (logic.RecMode)
                logic.changeThicknessRec();
            else if (logic.PolyMode)
                logic.changeThicknessPoly();
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (logic.LinesMode)
                logic.remove_item_line();
            else if (logic.PolyMode)
                logic.remove_item_poly();
            else if (logic.RecMode)
                logic.remove_item_rec();
        }
        private void MoveObject_Click(object sender, RoutedEventArgs e)
        {
            logic.MoveObjectMode = true;
         
        }
        private void MoveVertex_Click(object sender, RoutedEventArgs e)
        {
            logic.MoveVertexMode = true;

        }
        private void MoveEdge_Click(object sender, RoutedEventArgs e)
        {
            logic.MoveEdgeMode = true;

        }
        private void FillObject_Click(object sender, RoutedEventArgs e)
        {
            logic.fill_polygon();
        }

        private void FloodFillObject_Click(object sender, RoutedEventArgs e)
        {
            logic.flood_fill();
        }

        private void ImageObject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                logic.fill_image_polygon(op.FileName);
            }
            
        }
      
        //private void AreaLabel_DragEnter(object sender, DragEventArgs e)
        //{
        //    Point p = e.GetPosition(this.AreaLabel);
        //    logic.EditPoint = new System.Drawing.Point((int)p.X, (int)p.Y);
        //    logic.edit_line();
        //}

        //private void AreaLabel_DragLeave(object sender, DragEventArgs e)
        //{
        //    Point p = e.GetPosition(this.AreaLabel);
        //    logic.NewPoint= new System.Drawing.Point((int)p.X, (int)p.Y);
        //    logic.moved_line();
        //}

        //private void AreaLabel_Drop(object sender, DragEventArgs e)
        //{
        //    Point p = e.GetPosition(this.AreaLabel);
        //    logic.NewPoint = new System.Drawing.Point((int)p.X, (int)p.Y);
        //    logic.moved_line();
        //}
    }
}
