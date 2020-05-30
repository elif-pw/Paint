using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace cgproject3
{
    class DrawingLogic : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public BitmapSource drawingAreaBitmapSource { get; set; }
        private Bitmap drawingAreaBitmap { get; set; }

        public bool LinesMode { get; set; }
        public bool CirclesMode { get; set; }

        public bool PolyMode { get; set; }
        public bool CapMode { get; set; }
        public bool RecMode { get; set; } 
        
        public bool ClipPolyMode { get; set; }

        public bool MoveObjectMode { get; set; } = false;
        public bool MoveVertexMode { get; set; } = false;
        public bool MoveEdgeMode { get; set; } = false;
        public int? PolyFirstX { get; set; } = null;
        public int? PolyFirstY { get; set; } = null;

        // public List<Shape> shapes;

        public List<MyLine> shapes_lines;
        public List<Circle> shapes_circles;
        public List<Polygon> shapes_polygons;
        public List<MyRectangle> shapes_recs;
        public Polygon clippingPolygon;

        public int colorR { get; set; } = 0;
        public int colorG { get; set; } = 0;
        public int colorB { get; set; } = 0;

        public int Thickness { get; set; } = 1;

        public bool AntiAliased { get; set; } = false;
        public int AreaWidth;
        public int AreaHeight;

        public Point EditPoint;
        public Point NewPoint;
        public Point OldPoint;

        BitmapData outbmpData;

        public BitmapData uploadedBitmapData;
        public Bitmap uploadedBitmap;

        public DrawingLogic(int width, int height)
        {
            AreaWidth = width;
            AreaHeight = height;
            drawingAreaBitmap = new Bitmap(width, height);
            using (Graphics graph = Graphics.FromImage(drawingAreaBitmap))
            {
                graph.FillRectangle(System.Drawing.Brushes.White, new Rectangle(0, 0, width, height));
            }
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);

            //  shapes = new List<Shape>();
            shapes_circles = new List<Circle>();
            shapes_lines = new List<MyLine>();
            shapes_polygons = new List<Polygon>();
            shapes_recs = new List<MyRectangle>();
        }

        private void InvokePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void whiten_page()
        {
            drawingAreaBitmap = new Bitmap(AreaWidth, AreaHeight);
            using (Graphics graph = Graphics.FromImage(drawingAreaBitmap))
            {
                graph.FillRectangle(System.Drawing.Brushes.White, new Rectangle(0, 0, AreaWidth, AreaHeight));
            }
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
        }


        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          bitmap.GetHbitmap(),
                          IntPtr.Zero,
                          System.Windows.Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
            return bitmapSource;
        }


        public bool inRange(int x, int y)
        {
            if (x > 0 && x < AreaWidth && y > 0 && y < AreaHeight)
                return true;
            return false;
        }

        public unsafe void DrawLine(int x1, int y1, int x2, int y2, bool deleting = false, int? thick = null)
        {
            Rectangle rect = new Rectangle(0, 0, drawingAreaBitmap.Width, drawingAreaBitmap.Height);
            outbmpData =
                   drawingAreaBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            if (x1 > x2)
            {
                int tempx = x1;
                int tempy = y1;
                x1 = x2;
                x2 = tempx;
                y1 = y2;
                y2 = tempy;
            }

            if (y2 >= y1)
            {
                if ((y2 - y1) > (x2 - x1))
                {
                    MidpointLineNNE(x1, x2, y1, y2, deleting, thick);
                }
                else
                {
                    MidpointLineENE(x1, x2, y1, y2, deleting, thick);
                }
            }
            else
            {
                if ((y2 - y1) > -(x2 - x1))
                {
                    MidpointLineESE(x1, x2, y1, y2, deleting, thick);
                }
                else
                {
                    MidpointLineSSE(x1, x2, y1, y2, deleting, thick);
                }
            }
           
            drawingAreaBitmap.UnlockBits(outbmpData);
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");

        }

        private unsafe void  drawthick(int x, int y, bool deleting = false, int? thick = null)
        {
            int R, B, G, thickness_level;
            if (deleting == true)
            {
                B = 255;
                G = 255;
                R = 255;
            }
            else
            {
                R = colorB;
                B = colorR;
                G = colorG;

            }
            if (ClipPolyMode)
            {
                B = 255;
            }
            if (thick != null)
            {
                thickness_level = (int)thick;
            }
            else
            {
                thickness_level = Thickness;
            }
       
          
            for (int i = 0; i < thickness_level / 2 + 1; i++)
                {
                    for (int j = 0; j < thickness_level / 2 + 1; j++)
                    {
                        if (j * j + i * i < (thickness_level / 2 + 1) * (thickness_level / 2 + 1))
                        {
                            byte* p;
                            if (inRange(x + j, y + i))
                            {
                                p = (byte*)outbmpData.Scan0 + ((y + i) * outbmpData.Stride) + ((x + j) * 3);
                                p[0] = (byte)R;
                                p[1] = (byte)G;
                                p[2] = (byte)B;
                            }
                            if (inRange(x - j, y + i))
                            {
                                p = (byte*)outbmpData.Scan0 + ((y + i) * outbmpData.Stride) + ((x - j) * 3);
                                p[0] = (byte)R;
                                p[1] = (byte)G;
                                p[2] = (byte)B;
                            }
                            if (inRange(x + j, y - i))
                            {
                                p = (byte*)outbmpData.Scan0 + ((y - i) * outbmpData.Stride) + ((x + j) * 3);
                                p[0] = (byte)R;
                                p[1] = (byte)G;
                                p[2] = (byte)B;
                            }
                            if (inRange(x - j, y - i))
                            {
                                p = (byte*)outbmpData.Scan0 + ((y - i) * outbmpData.Stride) + ((x - j) * 3);
                                p[0] = (byte)R;
                                p[1] = (byte)G;
                                p[2] = (byte)B;
                            }
                        }
                    }
                }

         
        }
        private void MidpointLineENE(int x1, int x2, int y1, int y2, bool deleting = false, int? thick = null)
        {

            int dx = x2 - x1;
            int dy = y2 - y1;
            int d = 2 * dy - dx;
            int dE = 2 * dy;             // increment used when moving to E
            int dNE = 2 * (dy - dx);     // increment used when movint to NE
            int x = x1; int y = y1;

            drawthick(x, y, deleting, thick);
            while (x < x2)
            {
                if (d < 0) // move to E
                {
                    d += dE;
                    x++;
                }
                else // move to NE
                {
                    d += dNE;
                    ++x;
                    ++y;
                }

                drawthick(x, y, deleting, thick);
            }
        }

        private void MidpointLineNNE(int x1, int x2, int y1, int y2, bool deleting = false, int? thick = null)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int d = 2 * dx - dy;
            int dN = 2 * dx;             // increment used when moving to N
            int dNE = 2 * (dx - dy);     // increment used when moving to NE
            int x = x1;
            int y = y1;
            drawthick(x, y, deleting, thick);
            while (y < y2)
            {
                if (d < 0) // move to N
                {
                    d += dN;
                    y++;
                }
                else // move to NE
                {
                    d += dNE;
                    ++x;
                    ++y;
                }
                drawthick(x, y, deleting, thick);
            }

        }
        private void MidpointLineSSE(int x1, int x2, int y1, int y2, bool deleting = false, int? thick = null)
        {

            int dx = x2 - x1;
            int dy = y2 - y1;
            int d = 2 * dx + dy;
            int dS = 2 * dx;                // increment used when moving to S
            int dSE = 2 * (dy + dx);        // increment used when moving to SE
            int x = x1;
            int y = y1;
            drawthick(x, y, deleting, thick);
            while (y > y2)
            {
                if (d < 0) // move to S
                {
                    d += dS;
                    --y;
                }
                else // move to SE
                {
                    d += dSE;
                    ++x;
                    --y;
                }
                drawthick(x, y, deleting, thick);
            }

        }

        private void MidpointLineESE(int x1, int x2, int y1, int y2, bool deleting = false, int? thick = null)
        {

            int dx = x2 - x1;
            int dy = y2 - y1;
            int d = 2 * dy + dx;
            int dE = 2 * dy;             // increment used when moving to E
            int dSE = 2 * (dy + dx);     // increment used when moving to SE
            int x = x1;
            int y = y1;
            drawthick(x, y, deleting, thick);
            while (x < x2)
            {
                if (d > 0) // move to E
                {
                    d += dE;
                    ++x;
                }
                else // move to SE
                {
                    d += dSE;
                    ++x;
                    --y;
                }
                drawthick(x, y, deleting, thick);
            }

        }

        public  void DrawCircle(int x1, int y1, int x2, int y2, bool deleting = false, int? thick = null)
        {
            var r = (int)System.Windows.Point.Subtract(new System.Windows.Point(x1, y1), new System.Windows.Point(x2, y2)).Length;
            MidpointCircle(x1, y1, r, deleting);
            shapes_circles.Add(new Circle(r, x1, y1, Thickness, colorR, colorG, colorB));
            
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
        }
        public unsafe void MidpointCircle(int x1, int y1, int R, bool deleting = false, int? thick = null)
        {
            Rectangle rect = new Rectangle(0, 0, drawingAreaBitmap.Width, drawingAreaBitmap.Height);
            outbmpData =
                   drawingAreaBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);


            int d = 1 - R;
            int x = 0;
            int y = R;
            drawthick(x1 + x, y1 + y, deleting, thick);
            drawthick(x1 + x, y1 - y, deleting, thick);
            drawthick(x1 - x, y1 + y, deleting, thick);
            drawthick(x1 - x, y1 - y, deleting, thick);


            while (y > x)
            {
                if (d < 0) //move to E
                    d += 2 * x + 3;
                else //move to SE
                {
                    d += 2 * x - 2 * y + 5;
                    --y;
                }
                ++x;
                drawthick(x1 + x, y1 + y, deleting, thick);
                drawthick(x1 + x, y1 - y, deleting, thick);
                drawthick(x1 - x, y1 + y, deleting, thick);
                drawthick(x1 - x, y1 - y, deleting, thick);

            }

            d = 1 - R;
            x = R;
            y = 0;
            drawthick(x1 + x, y1 + y, deleting, thick);
            drawthick(x1 + x, y1 - y, deleting, thick);
            drawthick(x1 - x, y1 + y, deleting, thick);
            drawthick(x1 - x, y1 - y, deleting, thick);

            while (y < x)
            {
                if (d < 0) //move to E
                    d += 2 * y + 3;
                else //move to SE
                {
                    d += 2 * y - 2 * x + 5;
                    --x;
                }
                ++y;
                drawthick(x1 + x, y1 + y, deleting, thick);
                drawthick(x1 + x, y1 - y, deleting, thick);
                drawthick(x1 - x, y1 + y, deleting, thick);
                drawthick(x1 - x, y1 - y, deleting, thick);

            }
            drawingAreaBitmap.UnlockBits(outbmpData);
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
        }



        //Gupta-Sproull’s Algorithm (Lines)
        public void DrawAntiAliasedLine(int x1, int y1, int x2, int y2, bool deleting = false, int? thick = null)
        {
                int thickness_level;

            if (x1 > x2)
            {
                int tempx = x1;
                int tempy = y1;
                x1 = x2;
                x2 = tempx;
                y1 = y2;
                y2 = tempy;
            }
            if (thick != null)
            {
                thickness_level = (int)thick;
            }
            else
            {
                thickness_level = Thickness;
            }


            if (y2 >= y1)
            {
                if ((y2 - y1) > (x2 - x1))
                {
                    ThickAntialiasedLineNNE(x1, y1, x2, y2, thickness_level, deleting);
                }
                else
                {
                    ThickAntialiasedLineENE(x1, y1, x2, y2, thickness_level, deleting);

                }


            }
            else
            {
                if ((y2 - y1) > -(x2 - x1))
                {
                    ThickAntialiasedLineESE(x1, x2, y1, y2, thickness_level, deleting);
                }
                else
                {
                    ThickAntialiasedLineSSE(x1, x2, y1, y2, thickness_level, deleting);
                }
            }

            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");

        }


        public float Cov(float d, float r)
        {
            if (d < r)
            {
                return (float)((Math.Acos(d / r) - d / (r * r) * Math.Sqrt(r * r - d * d)) / Math.PI);
            }
            return 0;
        }

        public float coverage(float thickness, float D, float r)
        {
            float w = thickness / 2;
            if (w >= r)
            {
                if (D >= w)
                {
                    return Cov(D - w, r);
                }
                else
                {
                    return 1 - Cov(w - D, r);
                }
            }
            else
            {
                if (D >= 0 && w >= D)
                {
                    return 1 - Cov(w - D, r) - Cov(w + D, r);
                }
                else if (D >= w && r - w >= D)
                {
                    return Cov(D - w, r) - Cov(D + w, r);
                }
                else
                {
                    return Cov(D - w, r);
                }
            }
        }

        public float IntensifyPixel(int x, int y, float thickness, float distance, bool deleting = false)
        {
         
            float r = 0.5f;
            float cov = coverage(thickness, distance, r);

            if (cov > 0)
            {
                if (inRange(x, y))
                {
                    if (deleting)
                    {
                        drawingAreaBitmap.SetPixel(x, y, Color.White);
                    }
                    else
                        drawingAreaBitmap.SetPixel(x, y, Color.FromArgb((int)(255f * (1 - cov)), (int)(255f * (1 - cov)), (int)(255f * (1 - cov))));


                }
            }
            return cov;
        }
        public void ThickAntialiasedLineENE(int x1, int y1, int x2, int y2, float thickness, bool deleting = false)
        {

            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int dE = 2 * dy, dNE = 2 * (dy - dx);
            int d = 2 * dy - dx;
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float)(1f / (2f * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2f * dx * invDenom; //precomputed constant
            int x = x1, y = y1;
            int i;
            IntensifyPixel(x, y, thickness, 0, deleting);
            for (i = 1; 0 < IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom, deleting); ++i) ;
            for (i = 1; 0 < IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom, deleting); ++i) ;

            while (x < x2)
            {
                ++x;
                if (d < 0) // move to E
                {
                    two_v_dx = d + dx;
                    d += dE;
                }
                else // move to NE
                {
                    two_v_dx = d - dx;
                    d += dNE;
                    ++y;
                }
                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, deleting);
                for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom, deleting) != 0; ++i) ;
                for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom, deleting) != 0; ++i) ;
            }

        }

        public void ThickAntialiasedLineNNE(int x1, int y1, int x2, int y2, float thickness, bool deleting = false)
        {

            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int dN = 2 * dx, dNE = 2 * (dx - dy);
            int d = 2 * dx - dy;
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float)(1f / (2f * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2f * dx * invDenom; //precomputed constant
            int x = x1, y = y1;
            int i;
            IntensifyPixel(x, y, thickness, 0, deleting);
            for (i = 1; 0 < IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom, deleting); ++i) ;
            for (i = 1; 0 < IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom, deleting); ++i) ;

            while (x < x2)
            {
                ++y;
                if (d < 0) // move to N
                {
                    two_v_dx = d + dx;
                    d += dN;
                }
                else // move to NE
                {
                    two_v_dx = d - dx;
                    d += dNE;
                    ++x;
                }
                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, deleting);
                for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom, deleting) != 0; ++i) ;
                for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom, deleting) != 0; ++i) ;
            }

        }

        public void ThickAntialiasedLineESE(int x1, int y1, int x2, int y2, float thickness, bool deleting = false)
        {

            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int dE = 2 * dy, dSE = 2 * (dy + dx);
            int d = 2 * dy + dx;
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float)(1f / (2f * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2f * dx * invDenom; //precomputed constant
            int x = x1, y = y1;
            int i;
            IntensifyPixel(x, y, thickness, 0, deleting);
            for (i = 1; 0 < IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom, deleting); ++i) ;
            for (i = 1; 0 < IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom, deleting); ++i) ;


            while (x < x2)
            {
                ++x;
                if (d > 0) // move to E
                {
                    two_v_dx = d + dy;
                    d += dE;
                }
                else // move to SE
                {
                    two_v_dx = d - dy;
                    d += dSE;
                    --y;
                }
                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, deleting);
                for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom, deleting) != 0; ++i) ;
                for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom, deleting) != 0; ++i) ;
            }

        }

        public void ThickAntialiasedLineSSE(int x1, int y1, int x2, int y2, float thickness, bool deleting = false)
        {

            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int dS = 2 * dx, dSE = 2 * (dy + dx);
            int d = 2 * dx + dy;
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float)(1f / (2f * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2f * dx * invDenom; //precomputed constant
            int x = x1, y = y1;
            int i;
            IntensifyPixel(x, y, thickness, 0, deleting);
            for (i = 1; 0 < IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom, deleting); ++i) ;
            for (i = 1; 0 < IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom, deleting); ++i) ;


            while (x < x2)
            {
                --y;
                if (d < 0) // move to S
                {
                    two_v_dx = d + dy;
                    d += dS;
                }
                else // move to SE
                {
                    two_v_dx = d - dy;
                    d += dSE;
                    ++x;
                }
                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom, deleting);
                for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom, deleting) != 0; ++i) ;
                for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom, deleting) != 0; ++i) ;
            }

        }



        public void delete_lines()
        {
            whiten_page();
            shapes_lines = new List<MyLine>();
            draw_everything();
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
            
        }

        public void delete_cicles()
        {
            whiten_page();
            shapes_circles = new List<Circle>();
            draw_everything();
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
            
        }
        public void delete_polys()
        {
            whiten_page();
            shapes_polygons = new List<Polygon>();
            draw_everything();
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
        }

        public void draw_everything(bool loading=false)
        {
            for (int i=0; i<shapes_circles.Count; i++)
            {
                var obj = shapes_circles[i];
                MidpointCircle(obj.x1, obj.y1, obj.rad, false, obj.thickness);
            }
            for (int i=0; i<shapes_lines.Count; i++)
            {
                var obj = shapes_lines[i];
                DrawLine(obj.x1, obj.y1, obj.x2, obj.y2, false, obj.thickness);
            }
            for (int i=0; i<shapes_polygons.Count; i++)
            {
                var obj = shapes_polygons[i];
                for (int j = 0; j < obj.vertices.Count; j++)
                {
                    if (j == obj.vertices.Count - 1)
                    {
                        DrawLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[0].X, obj.vertices[0].Y, false, obj.thickness);
                    }
                    else
                        DrawLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[j + 1].X, obj.vertices[j + 1].Y, false, obj.thickness);
                }
                if (obj.filled)
                {
                    fill_polygon(obj, loading);
                }
                else if(obj.filledImage)
                {
                    fill_image_polygon(obj.imagePath, obj);
                }
            }
            for (int i=0; i<shapes_recs.Count; i++)
            {
                var obj = shapes_recs[i];
                drawRectangle(obj.x1, obj.y1, obj.x2, obj.y2, false, obj.thickness);
            }
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");

        }

        public void load_shapes()
        {
            List<Shape> listshapes = new List<Shape>();
          //  string path = @"C:\Users\Elif\source\repos\lab4\cgproject4\cgproject3\file1.json";
            using (StreamReader r = new StreamReader(@"C:\Users\Elif\source\repos\lab4\cgproject4\cgproject3\file1.json"))
            {
                string json = r.ReadToEnd();
                listshapes = JsonConvert.DeserializeObject<List<Shape>>(json);

            }
            for (int i = 0; i < listshapes.Count; i++)
            {
                if (listshapes[i].type == "line")
                {
                    var obj = listshapes[i];
                    shapes_lines.Add(new MyLine(obj.x1, obj.y1, obj.x2, obj.y2, obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                   
                }
                else if (listshapes[i].type == "circle")
                {
                    var obj = listshapes[i];
                    shapes_circles.Add(new Circle(obj.rad, obj.x1, obj.y1, obj.thickness, obj.colorR, obj.colorG, obj.colorB));
                  
                }
                else if (listshapes[i].type == "poly")
                {
                    var obj = listshapes[i];
                    if (obj.filled)
                    {
                        shapes_polygons.Add(new Polygon(obj.fillcolorR, obj.fillcolorG, obj.fillcolorB , obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                    }
                    else if (obj.filledImage)
                    {
                        shapes_polygons.Add(new Polygon(obj.imagePath, obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                    }
                    else
                        shapes_polygons.Add(new Polygon(obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                    var poly = shapes_polygons[shapes_polygons.Count - 1];
                    for (int j = 0; j < obj.vertices.Count; j++)
                    {
                        poly.add(obj.vertices[j].X, obj.vertices[j].Y);
                    }
               
                }
                else if (listshapes[i].type == "rec")
                {
                    var obj = listshapes[i];
                    shapes_recs.Add(new MyRectangle(obj.x1, obj.y1, obj.x2, obj.y2, obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                  
                }

            }
            draw_everything(true);

        }

        public void apply_antial()
        {
            for (int i = 0; i < shapes_lines.Count; i++)
            {
                var lin = shapes_lines[i];
                if (lin.antialiased == false)
                {
                    DrawLine(lin.x1, lin.y1, lin.x2, lin.y2, true, lin.thickness);
                    DrawAntiAliasedLine(lin.x1, lin.y1, lin.x2, lin.y2, false, lin.thickness);
                    lin.antialiased = true;
                }
            }
            for (int i = 0; i < shapes_polygons.Count; i++)
            {
                var obj = shapes_polygons[i];
                if (obj.antialiased == false)
                {
                    for (int j = 0; j < obj.vertices.Count; j++)
                    {
                        if (j == obj.vertices.Count - 1)
                        {
                            DrawLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[0].X, obj.vertices[0].Y, true, obj.thickness);
                            DrawAntiAliasedLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[0].X, obj.vertices[0].Y, false, obj.thickness);

                        }
                        else
                        {
                            DrawLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[j + 1].X, obj.vertices[j + 1].Y, true, obj.thickness);
                            DrawAntiAliasedLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[j + 1].X, obj.vertices[j + 1].Y, false, obj.thickness);
                        }

                    }
                    obj.antialiased = true;
                }
            }
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");

        }
        public void remove_antial()
        {
            for (int i = 0; i < shapes_lines.Count; i++)
            {
                var lin = shapes_lines[i];
                if (lin.antialiased)
                {
                    DrawAntiAliasedLine(lin.x1, lin.y1, lin.x2, lin.y2, true, lin.thickness);
                    DrawLine(lin.x1, lin.y1, lin.x2, lin.y2, false, lin.thickness);
                    lin.antialiased = false;
                }
            }
            for (int i = 0; i < shapes_polygons.Count; i++)
            {
                var obj = shapes_polygons[i];
                if (obj.antialiased)
                {
                    for (int j = 0; j < obj.vertices.Count; j++)
                    {
                        if (j == obj.vertices.Count - 1)
                        {
                            DrawAntiAliasedLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[0].X, obj.vertices[0].Y, true, obj.thickness);
                            DrawLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[0].X, obj.vertices[0].Y, false, obj.thickness);

                        }
                        else
                        {
                            DrawAntiAliasedLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[j + 1].X, obj.vertices[j + 1].Y, true, obj.thickness);
                            DrawLine(obj.vertices[j].X, obj.vertices[j].Y, obj.vertices[j + 1].X, obj.vertices[j + 1].Y, false, obj.thickness);
                        }

                    }
                    obj.antialiased = false;
                }
            }
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");

        }


        public void changeThicknessLine()
        {
            for (int i = 0; i < shapes_lines.Count; i++)
            {
                var lin = shapes_lines[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10) ||
                    (lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {

                    DrawLine(lin.x1, lin.y1, lin.x2, lin.y2, true, lin.thickness);
                    shapes_lines.RemoveAt(i);
                    whiten_page();
                    shapes_lines.Add(new MyLine(lin.x1, lin.y1, lin.x2, lin.y2, Thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    draw_everything();
                    return;
                    
                }
            }
        }
        public void changeThicknessRec()
        {

        }
        public void changeThicknessPoly()
        {
            for (int i = 0; i < shapes_polygons.Count; i++)
            {

                var obj = shapes_polygons[i];
                for (int j = 0; j < obj.vertices.Count; j++)
                {
                    var point = obj.vertices[j];
                    if (point.X < EditPoint.X + 10 && point.X > EditPoint.X - 10 && point.Y < EditPoint.Y + 10 && EditPoint.Y - 10 < point.Y)
                    {
                        shapes_polygons[i].thickness = Thickness;
                        whiten_page();
                        draw_everything();
                        return;
                    }
                }

            }
        }


        public void remove_item_line()
        {
            for (int i = 0; i < shapes_lines.Count; i++)
            {
                var lin = shapes_lines[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10) ||
                    (lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    DrawLine(lin.x1, lin.y1, lin.x2, lin.y2, true, lin.thickness);
                    shapes_lines.RemoveAt(i);
                    draw_everything();
                    return;
                }
            }
        }
        public void remove_item_poly()
        {
            for (int i = 0; i < shapes_polygons.Count; i++)
            {

                var obj = shapes_polygons[i];
                for (int j=0; j<obj.vertices.Count; j++)
                {
                    var point = obj.vertices[j];
                    if (point.X < EditPoint.X + 10 && point.X > EditPoint.X - 10 && point.Y < EditPoint.Y + 10 && EditPoint.Y-10<point.Y)
                    {
                        shapes_polygons.RemoveAt(i);
                        whiten_page();
                        draw_everything();
                        return;
                    }
                }
              
            }
        }
        public void remove_item_rec()
        {

            for (int i = 0; i < shapes_recs.Count; i++)
            {
                var lin = shapes_recs[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10) ||
                    (lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10) ||
                     (lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10) ||
                      (lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    draw_everything();
                    return;
                }
            }

        }

        public void move_item_line(int x, int y)
        {
           
            for (int i = 0; i < shapes_lines.Count; i++)
            {
                var lin = shapes_lines[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10) )
                {

                    whiten_page();
                    shapes_lines.RemoveAt(i);
                    shapes_lines.Add(new MyLine(x, y, lin.x2 - lin.x1 + x, lin.y2 - lin.y1 + y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveObjectMode = false;
                    draw_everything();
                    return;
                }
                else if ((lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_lines.RemoveAt(i);
                    shapes_lines.Add(new MyLine(lin.x1 - lin.x2 + x, lin.y1 - lin.y2 + y, x, y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveObjectMode = false;
                    draw_everything();
                    return;

                }
            }
            MoveObjectMode = false;
        }

        public void move_item_poly(int x, int y)
        {
            //move polyy to do
            for (int i = 0; i < shapes_polygons.Count; i++)
            {

                var obj = shapes_polygons[i];
                for (int j = 0; j < obj.vertices.Count; j++)
                {
                    var point = obj.vertices[j];
                    if (point.X < EditPoint.X + 10 && point.X > EditPoint.X - 10 && point.Y < EditPoint.Y + 10 && EditPoint.Y - 10 < point.Y)
                    {
                        shapes_polygons.Add(new Polygon(obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                        Polygon newpoly = shapes_polygons[shapes_polygons.Count - 1];
                        for (int z=0; z<obj.vertices.Count; z++)
                        {
                            if (z == j)
                            {
                                newpoly.add(x, y);
                            }
                            else
                                newpoly.add(obj.vertices[z].X - obj.vertices[j].X + x, obj.vertices[z].Y - obj.vertices[j].Y + y);
                        }
                        if (obj.filled)
                        {
                            newpoly.fill(obj.fillcolorR, obj.fillcolorG, obj.fillcolorB);
                        }
                        else if (obj.filledImage)
                        {
                            newpoly.fillwimage(obj.imagePath);
                        }
                        shapes_polygons.RemoveAt(i);
                        whiten_page();
                        draw_everything();
                        MoveObjectMode = false;
                        return;
                    }
                }

            }
            MoveObjectMode = false;
        }
        public void move_item_rec(int x, int y)
        {
            //move rectangle
            for (int i = 0; i < shapes_recs.Count; i++)
            {
                var lin = shapes_recs[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10))

                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(x, y, lin.x2 - lin.x1 + x, lin.y2 - lin.y1 + y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    draw_everything();
                    MoveObjectMode = false;
                    return;
                }
                else if ((lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(lin.x1 - lin.x2 + x, lin.y1 - lin.y2 + y, x, y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    draw_everything();
                    MoveObjectMode = false;
                    return;
                }
                else if ((lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(lin.x1 - lin.x2 + x, y, x, lin.y2 - lin.y1 + y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    draw_everything();
                    MoveObjectMode = false;
                    return;
                }
                else if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(x, lin.y1 - lin.y2 + y, lin.x2 - lin.x1 + x, y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    draw_everything();
                    MoveObjectMode = false;
                    return;
                }
            }
            MoveObjectMode = false;
        }

        public void move_vertex_line(int x, int y)
        {

            for (int i = 0; i < shapes_lines.Count; i++)
            {
                var lin = shapes_lines[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_lines.RemoveAt(i);
                    shapes_lines.Add(new MyLine(x, y, lin.x2, lin.y2, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    draw_everything();
                    MoveVertexMode = false;
                    return;
                }
                else if ((lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_lines.RemoveAt(i);
                    shapes_lines.Add(new MyLine(lin.x1 , lin.y1, x, y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    draw_everything();
                    MoveVertexMode = false;
                    return;

                }
            }
         
            MoveVertexMode = false;

        }
        public void move_vertex_poly(int x, int y)
        {
            for (int i = 0; i < shapes_polygons.Count; i++)
            {
                var obj = shapes_polygons[i];
                for (int j = 0; j < obj.vertices.Count; j++)
                {
                    var point = obj.vertices[j];
                    if (point.X < EditPoint.X + 10 && point.X > EditPoint.X - 10 && point.Y < EditPoint.Y + 10 && EditPoint.Y - 10 < point.Y)
                    {
                        shapes_polygons.Add(new Polygon(obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                        Polygon newpoly = shapes_polygons[shapes_polygons.Count - 1];
                        for (int z = 0; z < obj.vertices.Count; z++)
                        {
                            if (z == j)
                            {
                                newpoly.add(x, y);
                            }
                            else
                                newpoly.add( obj.vertices[z].X ,obj.vertices[z].Y);
                        }
                        if (obj.filled)
                        {
                            newpoly.fill(obj.fillcolorR, obj.fillcolorG, obj.fillcolorB);
                        }
                        else if (obj.filledImage)
                        {
                            newpoly.fillwimage(obj.imagePath);
                        }
                     
                        shapes_polygons.RemoveAt(i);
                        whiten_page();
                        draw_everything();
                        MoveVertexMode = false;
                        return;
                    }
                }

            }
            MoveVertexMode = false;
        }
        public void move_vertex_rec(int x, int y)
        {
            //move rectangle vertex
            for (int i = 0; i < shapes_recs.Count; i++)
            {
                var lin = shapes_recs[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10))

                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(x, y, lin.x2, lin.y2, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveVertexMode = false;
                    draw_everything();
                    return;
                }
                else if ((lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(lin.x1, lin.y1, x, y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveVertexMode = false;
                    draw_everything();
                    return;
                }
                else if ((lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(lin.x1, y, x, lin.y2, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveVertexMode = false;
                    draw_everything();
                    return;
                }
                else if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(x, lin.y1, lin.x2, y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveVertexMode = false;
                    draw_everything();
                    return;
                }
            }

            MoveVertexMode = false;
        }

        public void move_edge_rec(int x, int y)
        {
            //move rectangle vertex
            for (int i = 0; i < shapes_recs.Count; i++)
            {
                var lin = shapes_recs[i];
                if ((lin.x1 <= EditPoint.X + 10 && lin.x1 >= EditPoint.X - 10 && lin.y1 <= EditPoint.Y  && lin.y2 >= EditPoint.Y ))

                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(x, lin.y1, lin.x2, lin.y2, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveEdgeMode = false;
                    draw_everything();
                    return;
                }
                else if ((lin.x2 <= EditPoint.X + 10 && lin.x2 >= EditPoint.X - 10 && lin.y1 <= EditPoint.Y  && lin.y2 >= EditPoint.Y ))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(lin.x1, lin.y1, x, lin.y2, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveEdgeMode = false;
                    draw_everything();
                    return;
                }
                else if ((lin.x1 <= EditPoint.X  && lin.x2 >= EditPoint.X  && lin.y1 <= EditPoint.Y + 10 && lin.y1 >= EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(lin.x1, y, lin.x2, lin.y2, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveEdgeMode = false;
                    draw_everything();
                    return;
                }
                else if ((lin.x1 <= EditPoint.X  && lin.x2 >= EditPoint.X && lin.y2 <= EditPoint.Y + 10 && lin.y2 >= EditPoint.Y - 10))
                {
                    whiten_page();
                    shapes_recs.RemoveAt(i);
                    shapes_recs.Add(new MyRectangle(lin.x1, lin.y1, lin.x2, y, lin.thickness, lin.antialiased, lin.colorR, lin.colorG, lin.colorB));
                    MoveEdgeMode = false;
                    draw_everything();
                    return;
                }
            }

            MoveEdgeMode = false;
        }
        public void move_edge_poly(int x, int y)
        {
            for (int i = 0; i < shapes_polygons.Count; i++)
            {

                var obj = shapes_polygons[i];
                for (int j = 0; j < obj.vertices.Count ; j++)
                {
                   
                    int second;
                    Point nextpoint;
                    if (j == (obj.vertices.Count - 1))
                    {
                        nextpoint = obj.vertices[0];
                        second = 0;
                    }
                    else
                    {
                        nextpoint = obj.vertices[j + 1];
                        second = j + 1;
                    }

                    var point = obj.vertices[j];

                    if ((point.X-10 <= EditPoint.X && EditPoint.X <= nextpoint.X+10 && point.Y-10 <= EditPoint.Y && EditPoint.Y <= nextpoint.Y+10) ||
                            (point.X+10 >= EditPoint.X && EditPoint.X >= nextpoint.X-10 && point.Y-10 <= EditPoint.Y && EditPoint.Y <= nextpoint.Y+10) ||
                            (point.X+10 >= EditPoint.X && EditPoint.X >= nextpoint.X-10 && point.Y+10 >= EditPoint.Y && EditPoint.Y >= nextpoint.Y-10) ||
                             (point.X-10 <= EditPoint.X && EditPoint.X <= nextpoint.X+10 && point.Y+10 >= EditPoint.Y && EditPoint.Y >= nextpoint.Y-10))

                    {
                        shapes_polygons.Add(new Polygon(obj.thickness, obj.antialiased, obj.colorR, obj.colorG, obj.colorB));
                        Polygon newpoly = shapes_polygons[shapes_polygons.Count - 1];
                        for (int z = 0; z < obj.vertices.Count; z++)
                        {
                            if (z == j)
                            {
                                newpoly.add(obj.vertices[j].X - EditPoint.X + x, obj.vertices[j].Y - EditPoint.Y + y);
                            }
                            else if (z == second)
                            {
                                newpoly.add(obj.vertices[second].X - EditPoint.X + x, obj.vertices[second].Y - EditPoint.Y + y);
                            }
                            else
                                newpoly.add(obj.vertices[z].X, obj.vertices[z].Y);
                        }
                        if (obj.filled)
                        {
                            newpoly.fill(obj.fillcolorR, obj.fillcolorG, obj.fillcolorB);
                        }
                        else if (obj.filledImage)
                        {
                            newpoly.fillwimage(obj.imagePath);
                        }
                        shapes_polygons.RemoveAt(i);
                        whiten_page();
                        draw_everything();
                        MoveEdgeMode = false;
                        return;
                    }

                }

            }
            MoveEdgeMode = false;
        }
        public void edit_line()
        {
            for (int i = 0; i < shapes_lines.Count; i++)
            {
                var lin = shapes_lines[i];
                if ((lin.x1 < EditPoint.X + 10 && lin.x1 > EditPoint.X - 10 && lin.y1 < EditPoint.Y + 10 && lin.y1 > EditPoint.Y - 10))

                {
                    OldPoint = new Point(lin.x2, lin.y2);
                    DrawLine(lin.x1, lin.y1, lin.x2, lin.y2, true, lin.thickness);
                }
                else if ((lin.x2 < EditPoint.X + 10 && lin.x2 > EditPoint.X - 10 && lin.y2 < EditPoint.Y + 10 && lin.y2 > EditPoint.Y - 10))
                {
                    OldPoint = new Point(lin.x1, lin.y1);
                    DrawLine(lin.x1, lin.y1, lin.x2, lin.y2, true, lin.thickness);
                }
            }
        }

        public void moved_line()
        {
            DrawLine(OldPoint.X, OldPoint.Y, NewPoint.X, NewPoint.Y);
        }


        private int checksign(int Ex, int Ey, int Dx, int Dy, int Fx, int Fy)
        {
            return ((Ex - Dx) * (Fy - Dy) - (Ey - Dy) * (Fx - Dx));
        }


        private void drawsemicircle(int Ax, int Ay, int v1, int v2, int r, bool negative)
        {
            if (negative)
            {
                int d = 1 - r;
                int x = 0;
                int y = r;
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) < 0)
                {
                    drawthick(Ax + x, Ay + y);
                }
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) < 0)
                    drawthick(Ax + x, Ay - y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) < 0)
                    drawthick(Ax - x, Ay + y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) < 0)
                    drawthick(Ax - x, Ay - y);


                while (y > x)
                {
                    if (d < 0)
                        d += 2 * x + 3;
                    else
                    {
                        d += 2 * x - 2 * y + 5;
                        --y;
                    }
                    ++x;
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) < 0)
                    {
                        drawthick(Ax + x, Ay + y);
                    }
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) < 0)
                        drawthick(Ax + x, Ay - y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) < 0)
                        drawthick(Ax - x, Ay + y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) < 0)
                        drawthick(Ax - x, Ay - y);



                }

                d = 1 - r;
                x = r;
                y = 0;
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) < 0)
                {
                    drawthick(Ax + x, Ay + y);
                }
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) < 0)
                    drawthick(Ax + x, Ay - y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) < 0)
                    drawthick(Ax - x, Ay + y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) < 0)
                    drawthick(Ax - x, Ay - y);


                while (y < x)
                {
                    if (d < 0)
                        d += 2 * y + 3;
                    else
                    {
                        d += 2 * y - 2 * x + 5;
                        --x;
                    }
                    ++y;
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) < 0)
                    {
                        drawthick(Ax + x, Ay + y);
                    }
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) < 0)
                        drawthick(Ax + x, Ay - y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) < 0)
                        drawthick(Ax - x, Ay + y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) < 0)
                        drawthick(Ax - x, Ay - y);


                }

            }
            else
            {
                int d = 1 - r;
                int x = 0;
                int y = r;
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) > 0)
                {
                    drawthick(Ax + x, Ay + y);
                }
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) > 0)
                    drawthick(Ax + x, Ay - y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) > 0)
                    drawthick(Ax - x, Ay + y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) > 0)
                    drawthick(Ax - x, Ay - y);


                while (y > x)
                {
                    if (d < 0)
                        d += 2 * x + 3;
                    else
                    {
                        d += 2 * x - 2 * y + 5;
                        --y;
                    }
                    ++x;
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) > 0)
                    {
                        drawthick(Ax + x, Ay + y);
                    }
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) > 0)
                        drawthick(Ax + x, Ay - y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) > 0)
                        drawthick(Ax - x, Ay + y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) > 0)
                        drawthick(Ax - x, Ay - y);




                }

                d = 1 - r;
                x = r;
                y = 0;
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) > 0)
                {
                    drawthick(Ax + x, Ay + y);
                }
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) > 0)
                    drawthick(Ax + x, Ay - y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) > 0)
                    drawthick(Ax - x, Ay + y);
                if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) > 0)
                    drawthick(Ax - x, Ay - y);



                while (y < x)
                {
                    if (d < 0)
                        d += 2 * y + 3;
                    else
                    {
                        d += 2 * y - 2 * x + 5;
                        --x;
                    }
                    ++y;
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay + y) > 0)
                    {
                        drawthick(Ax + x, Ay + y);
                    }
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax + x, Ay - y) > 0)
                        drawthick(Ax + x, Ay - y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay + y) > 0)
                        drawthick(Ax - x, Ay + y);
                    if (checksign(Ax, Ay, Ax + v1, Ay + v2, Ax - x, Ay - y) > 0)
                        drawthick(Ax - x, Ay - y);



                }
            }

        }

        public unsafe void drawCapsule(int Ax, int Ay, int Bx, int By, int Cx, int Cy)
        {
            Rectangle rect = new Rectangle(0, 0, drawingAreaBitmap.Width, drawingAreaBitmap.Height);
            outbmpData =
                   drawingAreaBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            drawthick(Ax, Ay);
            drawthick(Bx, By);
            drawthick(Cx, Cy);
            var lenthab = (int)System.Windows.Point.Subtract(new System.Windows.Point(Bx, By), new System.Windows.Point(Ax, Ay)).Length;

            var r = (int)System.Windows.Point.Subtract(new System.Windows.Point(Bx, By), new System.Windows.Point(Cx, Cy)).Length;
            int v2 = (Bx - Ax) / lenthab * r;
            int v1 = -(By - Ay) / lenthab * r;
            drawthick(Ax + v1, Ay + v2);
            drawthick(Ax - v1, Ay - v2);
            drawthick(Bx + v1, By + v2);
            drawthick(Bx - v1, By - v2);

            DrawLine(Ax + v1, Ay + v2, Bx + v1, By + v2);
            DrawLine(Ax - v1, Ay - v2, Bx - v1, By - v2);


            drawsemicircle(Ax, Ay, v1, v2, r, true);
            drawsemicircle(Bx, By, v1, v2, r, false);

            drawingAreaBitmap.UnlockBits(outbmpData);
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");

        }

        public void drawRectangle(int Ax, int Ay, int Bx, int By, bool deleting = false,  int? thick = null)
        {
          
            DrawLine(Ax, Ay, Bx, Ay, deleting, thick);
            DrawLine(Ax, Ay, Ax, By, deleting, thick);
            DrawLine(Bx, By, Ax, By, deleting, thick);
            DrawLine(Bx, By, Bx, Ay, deleting, thick);
        }

        public void clip()
        {
            Point[] subjectPoly = shapes_polygons[shapes_polygons.Count - 1].vertices.ToArray();
            Point[] clipPoly = clippingPolygon.vertices.ToArray();

            Point[] clipped = SutherlandHodgman(subjectPoly, clipPoly);
            int n = clipped.Count();
            Polygon clippedNewPolygon = new Polygon();

            for (int i = 0; i < n; i++)
            {
                DrawLine(clipped[i].X, clipped[i].Y, clipped[(i + 1) % n].X, clipped[(i + 1) % n].Y, false, 9);
                clippedNewPolygon.add(clipped[i].X, clipped[i].Y);
            }
            clippedNewPolygon.thickness = 5;
            clippedNewPolygon.colorB = 0;
            clippedNewPolygon.colorG = 0;
            clippedNewPolygon.colorR = 0;
            shapes_polygons.Add(clippedNewPolygon);
        }


        public bool PolygonIsConvex()
        {
            bool got_negative = false;
            bool got_positive = false;
            int num_points = clippingPolygon.vertices.Count;
            int B, C;
            for (int A = 0; A < num_points; A++)
            {
                B = (A + 1) % num_points;
                C = (B + 1) % num_points;

                float cross_product =
                    CrossProductLength(
                        clippingPolygon.vertices[A].X, clippingPolygon.vertices[A].Y,
                        clippingPolygon.vertices[B].X, clippingPolygon.vertices[B].Y,
                        clippingPolygon.vertices[C].X, clippingPolygon.vertices[C].Y);
                if (cross_product < 0)
                {
                    got_negative = true;
                }
                else if (cross_product > 0)
                {
                    got_positive = true;
                }
                if (got_negative && got_positive) return false;
            }
            return true;
        }
        public static float CrossProductLength(float Ax, float Ay,
          float Bx, float By, float Cx, float Cy)
        {
            // Get the vectors' coordinates.
            float BAx = Ax - Bx;
            float BAy = Ay - By;
            float BCx = Cx - Bx;
            float BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy - BAy * BCx);
        }
        private class Edge
        {
            public Edge(Point from, Point to)
            {
                this.From = from;
                this.To = to;
            }

            public readonly Point From;
            public readonly Point To;
        }

        public static Point[] SutherlandHodgman(Point[] inPoly, Point[] clipPoly)
        {
            if (inPoly.Length < 3 || clipPoly.Length < 3)
            {
                return null;
            }
            List<Point> outPoly = inPoly.ToList();
            if (!IsClockwise(inPoly))
            {
                outPoly.Reverse();
            }
            foreach (Edge clipEdge in iterateEdgesClockwise(clipPoly))
            {
                List<Point> inputList = outPoly.ToList();
                outPoly.Clear();
                if (inputList.Count == 0)
                {
                    break;
                }
                Point S = inputList[inputList.Count - 1];
                foreach (Point E in inputList)
                {
                    if (isInside(clipEdge.From, clipEdge.To, E))
                    {
                        if (!isInside(clipEdge.From, clipEdge.To, S))
                        {
                            Point? point = intersect(S, E, clipEdge.From, clipEdge.To);
                            if (point == null)
                            {
                                throw new ApplicationException("Line segments don't intersect");
                            }
                            else
                            {
                                outPoly.Add(point.Value);
                            }
                        }
                        outPoly.Add(E);
                    }
                    else if (isInside(clipEdge.From, clipEdge.To, S))
                    {
                        Point? point = intersect(S, E, clipEdge.From, clipEdge.To);
                        if (point == null)
                        {
                            throw new ApplicationException("Line segments don't intersect");
                        }
                        else
                        {
                            outPoly.Add(point.Value);
                        }
                    }

                    S = E;
                }
            }
            return outPoly.ToArray();
        }
        private static bool isInside(Point p, Point r1, Point r2)
        {
            bool? isLeft = IsLeftOf(p, r1, r2);
            if (isLeft == null)
            {
                return true;
            }

            return !isLeft.Value;
        }
        private static bool? IsLeftOf(Point p, Point r1, Point r2)
        {
            Point tmp1 = new Point(r2.X - r1.X, r2.Y - r1.Y);
            Point tmp2 = new Point(p.X - r2.X, p.Y - r2.Y);
            double x = (tmp1.X * tmp2.Y) - (tmp1.Y * tmp2.X);
            if (x < 0)
                return false;
            else if (x > 0)
                return true;
            else
                return null;
        }
        private static IEnumerable<Edge> iterateEdgesClockwise(Point[] polygon)
        {
            if (IsClockwise(polygon))
            {
                for (int cntr = 0; cntr < polygon.Length - 1; cntr++)
                {
                    yield return new Edge(polygon[cntr], polygon[cntr + 1]);
                }
                yield return new Edge(polygon[polygon.Length - 1], polygon[0]);
            }
            else
            {
                for (int cntr = polygon.Length - 1; cntr > 0; cntr--)
                {
                    yield return new Edge(polygon[cntr], polygon[cntr - 1]);
                }
                yield return new Edge(polygon[0], polygon[polygon.Length - 1]);
            }
        }
        private static bool IsClockwise(Point[] polygon)
        {
            for (int cntr = 2; cntr < polygon.Length; cntr++)
            {
                bool? isLeft = IsLeftOf(polygon[cntr], polygon[0], polygon[1]);
                if (isLeft != null)
                {
                    return !isLeft.Value;
                }
            }

            throw new ArgumentException("All the points in the polygon are colinear");
        }

        private static Point intersect(Point line1From, Point line1To, Point line2From, Point line2To)
        {
            Point direction1 = new Point(line1To.X - line1From.X, line1To.Y - line1From.Y);
            Point direction2 = new Point(line2To.X - line2From.X, line2To.Y - line2From.Y);
            double dotPerp = (direction1.X * direction2.Y) - (direction1.Y * direction2.X);

            Point c = new Point(line2From.X - line1From.X, line2From.Y - line1From.Y);
            double t = (c.X * direction2.Y - c.Y * direction2.X) / dotPerp;
            return new Point(line1From.X + (int)(t * direction1.X), line1From.Y + (int)(t * direction1.Y));
        }


        public class ActiveEdge
        {
            public int ymax;
            public double x;
            public double slopeinverse;
            public ActiveEdge(Point a, Point b)
            {
                ymax = b.Y;
                x = (double)a.X;
                slopeinverse = ((double)(b.X - a.X)) / ((double)(b.Y - a.Y));
            }

        }

        public Polygon findclosestpoly()
        {
            for (int i = 0; i < shapes_polygons.Count; i++)
            {

                var obj = shapes_polygons[i];
                for (int j = 0; j < obj.vertices.Count ; j++)
                {
                    Point nextpoint;
                    if (j == (obj.vertices.Count - 1))
                    {
                        nextpoint = obj.vertices[0];
                    }
                    else
                    {
                        nextpoint = obj.vertices[j + 1];
                    }
                    var point = obj.vertices[j];
                    if ((point.X - 10 <= EditPoint.X && EditPoint.X <= nextpoint.X + 10 && point.Y - 10 <= EditPoint.Y && EditPoint.Y <= nextpoint.Y + 10) ||
                            (point.X + 10 >= EditPoint.X && EditPoint.X >= nextpoint.X - 10 && point.Y - 10 <= EditPoint.Y && EditPoint.Y <= nextpoint.Y + 10) ||
                            (point.X + 10 >= EditPoint.X && EditPoint.X >= nextpoint.X - 10 && point.Y + 10 >= EditPoint.Y && EditPoint.Y >= nextpoint.Y - 10) ||
                             (point.X - 10 <= EditPoint.X && EditPoint.X <= nextpoint.X + 10 && point.Y + 10 >= EditPoint.Y && EditPoint.Y >= nextpoint.Y - 10) ||
                               (point.X < EditPoint.X + 10 && point.X > EditPoint.X - 10 && point.Y < EditPoint.Y + 10 && EditPoint.Y - 10 < point.Y))
                    {
                        return obj;
                    }

                }

            }
            return null;
        }




        public unsafe void fill_polygon(Polygon poly=null, bool loading=false)
        {
         
            List<ActiveEdge> ActiveEdgeTable = new List<ActiveEdge>();
            Polygon polygon;
            List<Point> polygon_vertices;
            if (poly != null)
            {
                polygon = poly;
                polygon_vertices = poly.vertices;
            }
            else
            {
                Polygon p = findclosestpoly();
                if (p==null)
                    return;
                polygon = p;
                polygon_vertices =p.vertices;
            }
            int r, g, b;          
            if (loading)
            { // get filling color if there is one
                r = polygon.fillcolorB;
                g = polygon.fillcolorG;
                b = polygon.fillcolorR;
            }
            else
            {
                //give a filling color to polygon object
                polygon.filled = true;
                polygon.fill(colorR, colorG, colorB);
                polygon.filledImage = false;
                r = colorB;
                g = colorG;
                b = colorR;
            }           

            //locking bitmap
            Rectangle rect = new Rectangle(0, 0, drawingAreaBitmap.Width, drawingAreaBitmap.Height);
            outbmpData =
                   drawingAreaBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //active edge algo
            int vertex_count = polygon_vertices.Count();
            List<int> ordered_list = new List<int>();
            for (int count=0; count<vertex_count; count++)
            {
                ordered_list.Add(count);
            }
            //sort according to y
            ordered_list = ordered_list.OrderBy(p => polygon_vertices[p].Y).ToList();
            int k = 0;
            int i = ordered_list[k];
            int y = polygon_vertices[ordered_list[0]].Y; //assign ymin 
            int y_max = polygon_vertices[ordered_list[vertex_count - 1]].Y;
            while (y < y_max)
            {
                if (polygon_vertices[i].Y == y) 
                {
                    if (polygon_vertices[(i - 1 + vertex_count) % vertex_count].Y > polygon_vertices[i].Y)
                    {
                        ActiveEdgeTable.Add(new ActiveEdge(polygon_vertices[i], polygon_vertices[(i - 1 + vertex_count) % vertex_count]));

                    }
                    if (polygon_vertices[(i + 1) % vertex_count].Y > polygon_vertices[i].Y)
                    {
                        ActiveEdgeTable.Add(new ActiveEdge(polygon_vertices[i], polygon_vertices[(i + 1 + vertex_count) % vertex_count]));
                    }
                    i = ordered_list[++k];
                }
                //sort by increasing x
                ActiveEdgeTable = ActiveEdgeTable.OrderBy(p => p.x).ToList();
                for (int j = 0; j < ActiveEdgeTable.Count() - 1; j = j + 2)
                {
                    for (int x = (int)(ActiveEdgeTable[j].x); x < (int)(ActiveEdgeTable[j + 1].x); x++)
                    {
                        fillpixel(x, y, r, g, b );
                    }
                }
                ++y;
                ActiveEdgeTable.RemoveAll(x => (x.ymax == y));
                for (int edge = 0; edge < ActiveEdgeTable.Count; edge++)
                {
                    ActiveEdgeTable[edge].x += ActiveEdgeTable[edge].slopeinverse;
                }
            }
            drawingAreaBitmap.UnlockBits(outbmpData);
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
        }


        private unsafe void fillpixel(int x, int y, int R, int G, int B)
        {          
            byte* p;
            if (inRange(x, y))
            {
                p = (byte*)outbmpData.Scan0 + ((y ) * outbmpData.Stride) + ((x ) * 3);
                p[0] = (byte)R;
                p[1] = (byte)G;
                p[2] = (byte)B;
            }
        }

        public unsafe void fill_image_polygon(string path,Polygon poly = null)
        {
            List<ActiveEdge> ActiveEdgeTable = new List<ActiveEdge>();
            Polygon polygon;
            List<Point> polygon_vertices;
            if (poly != null)
            {
                polygon = poly;
                polygon_vertices = poly.vertices;

            }
            else
            {
                Polygon p = findclosestpoly();
                if (p == null)
                    return;
                polygon = p;
                polygon_vertices = p.vertices;
                polygon.fillwimage(path);
                polygon.filled = false;
            }
          
           
            //locking bitmap of the drawing area
            Rectangle rect = new Rectangle(0, 0, drawingAreaBitmap.Width, drawingAreaBitmap.Height);
            outbmpData =
                   drawingAreaBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            //locking bits of the image only for reading
            uploadedBitmap = (Bitmap)Bitmap.FromFile(path, true);
            Rectangle uploadedrect = new Rectangle(0, 0, uploadedBitmap.Width, uploadedBitmap.Height);
            uploadedBitmapData =
                   uploadedBitmap.LockBits(uploadedrect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte* ptr = (byte*)uploadedBitmapData.Scan0.ToPointer();


            //active edge algo
            int vertex_count = polygon_vertices.Count();
            List<int> ordered_list = new List<int>();
            for (int count = 0; count < vertex_count; count++)
            {
                ordered_list.Add(count);
            }
            //sort according to y
            ordered_list = ordered_list.OrderBy(p => polygon_vertices[p].Y).ToList();
            int k = 0;
            int i = ordered_list[k];
            int y = polygon_vertices[ordered_list[0]].Y;//ymшin 
            int yMax = polygon_vertices[ordered_list[vertex_count - 1]].Y;
            while (y < yMax)
            {
                if (polygon_vertices[i].Y == y)
                {
                    if (polygon_vertices[(i - 1 + vertex_count) % vertex_count].Y > polygon_vertices[i].Y) 
                    {
                        ActiveEdgeTable.Add(new ActiveEdge(polygon_vertices[i], polygon_vertices[(i - 1 + vertex_count) % vertex_count]));
                    }
                    if (polygon_vertices[(i + 1) % vertex_count].Y > polygon_vertices[i].Y)
                    {
                        ActiveEdgeTable.Add(new ActiveEdge(polygon_vertices[i], polygon_vertices[(i + 1 + vertex_count) % vertex_count]));
                    }
                    ++k;
                    i = ordered_list[k];
                }
            
                ActiveEdgeTable = ActiveEdgeTable.OrderBy(p => p.x).ToList();
                for (int j = 0; j < ActiveEdgeTable.Count() - 1; j = j + 2)
                {

                    byte* tmp = ptr + (y * uploadedBitmapData.Stride);
                    for (int x = (int)(ActiveEdgeTable[j].x); x < (int)(ActiveEdgeTable[j + 1].x); x++)
                    {
                        if (tmp!=null)
                        {
                            fillpixel(x, y, tmp[x * 3], tmp[x * 3 + 1], tmp[x * 3 + 2]);
                        }
                    }
                }
                ++y;
                ActiveEdgeTable.RemoveAll(x => (x.ymax == y));
                for (int edge = 0; edge < ActiveEdgeTable.Count; edge++)
                {
                    ActiveEdgeTable[edge].x += ActiveEdgeTable[edge].slopeinverse;
                }
            }
            uploadedBitmap.UnlockBits(uploadedBitmapData);
            drawingAreaBitmap.UnlockBits(outbmpData);
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");



        }



        public unsafe void flood_fill()
        {
            Rectangle rect = new Rectangle(0, 0, drawingAreaBitmap.Width, drawingAreaBitmap.Height);
            outbmpData =
                   drawingAreaBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* firstpix = (byte*)outbmpData.Scan0;
            int h = drawingAreaBitmap.Height;
            int w = drawingAreaBitmap.Width;
            Point q = EditPoint;
            byte seed = (byte)(firstpix + (q.Y * outbmpData.Stride))[q.X*3];

            if (q.Y < 0 || q.Y > h - 1 || q.X < 0 || q.X > w - 1)
                return;

            Stack<Point> stack = new Stack<Point>();
            stack.Push(q);
            while (stack.Count > 0)
            {
                Point p = stack.Pop();
                int x = p.X;
                int y = p.Y;
                if (y < 0 || y > h - 1 || x < 0 || x > w - 1)
                    continue;
                byte val = (byte)(firstpix + (y * outbmpData.Stride))[x*3 ];
                if (val==seed)
                {
                    fillpixel(x, y, colorB, colorG, colorR);
                    stack.Push(new Point(x + 1, y));
                    stack.Push(new Point(x - 1, y));
                    stack.Push(new Point(x, y + 1));
                    stack.Push(new Point(x, y - 1));
                }
            }

            drawingAreaBitmap.UnlockBits(outbmpData);
            drawingAreaBitmapSource = BitmapToBitmapSource(drawingAreaBitmap);
            InvokePropertyChanged("DrawingAreaBitmapSource");
        }



    }
}
 