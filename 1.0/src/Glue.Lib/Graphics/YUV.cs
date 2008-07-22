using System;
using System.Drawing;

namespace Glue.Lib.Graphics
{
    public struct YUV
    {
        public byte Y;
        public byte U;
        public byte V;

        public YUV(byte Y, byte U, byte V)
        {
            this.Y = Y;
            this.U = U;
            this.V = V;
        }

        public static YUV FromRGB(Color c)
        {
            byte Y, U, V;

            Y = (byte)(((  66 * c.R + 129 * c.G +  25 * c.B + 128) >> 8) +  16);
            U = (byte)((( -38 * c.R -  74 * c.G + 112 * c.B + 128) >> 8) + 128);
            V = (byte)((( 112 * c.R -  94 * c.G -  18 * c.B + 128) >> 8) + 128);
			
            return new YUV(Y,U,V);
        }

        public Color RGB
        {
            get
            {
                int C = Y - 16;
                int D = U - 128;
                int E = V - 128;

                byte R, G, B;

                R = (byte)Math.Max(Math.Min((( 298 * C           + 409 * E + 128) >> 8), 255), 0);
                G = (byte)Math.Max(Math.Min((( 298 * C - 100 * D - 208 * E + 128) >> 8), 255), 0);
                B = (byte)Math.Max(Math.Min((( 298 * C + 516 * D           + 128) >> 8), 255), 0);

                return Color.FromArgb(R,G,B);
            }
        }
    }
}
