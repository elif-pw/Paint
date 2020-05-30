using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgproject3
{
    class Polygon:Shape
    {
        public string type = "poly";
        public Polygon(int x1, int y1,int thickness, bool antialiased, int r, int g, int b)
        {
            colorB = b;
            colorG = g;
            colorR = r;
            vertices = new List<Point>();
            vertices.Add(new Point(x1, y1));
            this.thickness = thickness;
            this.antialiased = antialiased;
        }
        public Polygon()
        {
            vertices = new List<Point>();
        }
        public Polygon(int thickness, bool antialiased, int r, int g, int b)
        {
            colorB = b;
            colorG = g;
            colorR = r;
            vertices = new List<Point>();
            this.thickness = thickness;
            this.antialiased = antialiased;
        }
        public Polygon(int filr, int filg, int filb, int thickness, bool antialiased, int r, int g, int b)
        {
            filled = true;
            fillcolorR = filr;
            fillcolorG = filg;
            fillcolorB = filb;
            colorB = b;
            colorG = g;
            colorR = r;
            vertices = new List<Point>();
            this.thickness = thickness;
            this.antialiased = antialiased;
        }
        public Polygon( string path, int thickness, bool antialiased, int r, int g, int b)
        {
            imagePath = path;
            filledImage = true;
            colorB = b;
            colorG = g;
            colorR = r;
            vertices = new List<Point>();
            this.thickness = thickness;
            this.antialiased = antialiased;
        }

        public void add(int x, int y)
        {
            vertices.Add(new Point(x, y));
        }

        public void fillwimage(string path)
        {
            filledImage = true;
            imagePath = path;
        }
        public void fill(int r, int g, int b)
        {
            filled = true;
            fillcolorB = b;
            fillcolorG = g;
            fillcolorR = r;
        }
    }
}
