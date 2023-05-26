using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Utils
{
    internal static class Vector2D
    {
        public static double CalculateDistance(Single  X1,Single Y1, Single X2, Single Y2)
        {
            double x,y, result;
            x = X1 - X2;
            x = x * x;
            y = Y1 - Y2;
            y = y * y;
            result = Math.Sqrt(x + y);
            return result;
        }
    }
}
