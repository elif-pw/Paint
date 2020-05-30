using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgproject3
{
    public class Shape
    {
        public int x1 { get; set; }
        public int y1 { get; set; }
        public int x2 { get; set; }
        public int y2 { get; set; }
        public int colorR{ get; set; }
        public int colorG { get; set; }
        public int colorB { get; set; }
        public List<Point> vertices { get; set; }
        public int thickness { get; set; }
        public bool antialiased { get; set; }
        public string type { get; set; }

        public int rad { get; set; }

        public int fillcolorR { get; set; }
        public int fillcolorG { get; set; }
        public int fillcolorB { get; set; }

        public bool filled { get; set; }
        public bool filledImage { get; set; }
        public string imagePath { get; set; }

        public Shape(int colorR, int colorG, int colorB)
        {
            this.colorR = colorR;
            this.colorG = colorG;
            this.colorB = colorB;
        }

        //public Shape(int x1, int y1, int x2, int y2, int colorR, int colorG, int colorB, int thickness, bool antialiased, string type) : this(x1, y1, x2)
        //{ //line
        //    this.y2 = y2;
        //    this.colorR = colorR;
        //    this.colorG = colorG;
        //    this.colorB = colorB;
        //    this.thickness = thickness;
        //    this.antialiased = antialiased;
        //    this.type = type;
        //}

        //public Shape(int x1, int y1, int colorR, int colorG, int colorB, int thickness, string type, int rad) : this(x1, y1, colorR)
        //{//circ
        //    this.colorG = colorG;
        //    this.colorB = colorB;
        //    this.thickness = thickness;
        //    this.type = type;
        //    this.rad = rad;
        //}

        //public Shape(int colorR, int colorG, int colorB,  List<Point> vertices, int thickness, bool antialiased, string type ) : this(colorR, colorG, colorB)
        //{//poly
        //    this.vertices = vertices;
        //    this.thickness = thickness;
        //    this.antialiased = antialiased;
        //    this.type = type;

        //}
        //public Shape(int colorR, int colorG, int colorB, int fillR, int fillG, int fillB, List<Point> vertices, int thickness, bool antialiased, string type ) : this(colorR, colorG, colorB)
        //{//poly filled
        //    this.vertices = vertices;
        //    this.thickness = thickness;
        //    this.antialiased = antialiased;
        //    this.type = type;
        //    this.fillcolorB = fillB;
        //    this.fillcolorG = fillG;
        //    this.fillcolorR = fillR;

        //}

        public Shape()
        {
            
        }
        public void add(int x, int y)
        {
            vertices.Add(new Point(x, y));
        }
    }
}
