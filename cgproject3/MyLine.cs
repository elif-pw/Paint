using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgproject3
{
    class MyLine: Shape
    {
        public string type { get; set; } = "line";

        public MyLine(int x1, int y1, int x2, int y2,int thickness, bool antialiased, int r, int g, int b)
        {
          
            colorB = b;
            colorG = g;
            colorR = r;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.thickness = thickness;
            this.antialiased = antialiased;
        }

  
    }
}
