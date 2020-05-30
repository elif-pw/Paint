using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgproject3
{
    class Circle:Shape
    {
        public int rad;
        public int x1;
        public int y1;
        public int colorB;
        public int colorG;
        public int colorR;
        public int thickness;
        public string type = "circle";

        public Circle(int rad, int x1, int y1,int thickness, int r, int g, int b )
        {
            colorB = b;
            colorG = g;
            colorR = r;
            this.rad = rad;
            this.x1 = x1;
            this.y1 = y1;
            this.thickness = thickness;
        }
    }
}
