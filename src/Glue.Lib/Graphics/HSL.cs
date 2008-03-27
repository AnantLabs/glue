using System;
using System.Drawing;

namespace Glue.Lib.Graphics
{
    public struct HSL
    {
        private float h;
        private float s;
        private float l;

        public float Hue
        {
            get { return h; }
            set { h = (float)(Math.Abs(value) % 360); }
        }

        public float Saturation
        {
            get { return s; }
            set { s = (float)Math.Max(Math.Min(1.0, value), 0.0); }
        }

        public float Luminance
        {
            get { return l; }
            set { l = (float)Math.Max(Math.Min(1.0, value), 0.0); }
        }

        public HSL(float hue, float saturation, float luminance)
        {
            this.h = (float)(Math.Abs(hue) % 360);
            this.s = (float)Math.Max(Math.Min(1.0, saturation), 0.0);
            this.l = (float)Math.Max(Math.Min(1.0, luminance), 0.0);
        }

        public Color RGB
        {
            get
            {
                double r=0,g=0,b=0;

                if (l==0) 
                { 
                    // Luminance=0 => black
                    r=g=b=0; 
                } 
                else if (s==0) 
                { 
                    // Saturation=0 => gray level=l
                    r=g=b=l;
                } 
                else 
                { 
                    double temp2 = (l<=0.5) ? l*(1.0+s) : l+s-(l*s);
                    double temp1 = 2.0*l-temp2; 

                    double[] t3 = new double[] { h/360.0 + 1.0/3.0,
                                                 h/360.0,
                                                 h/360.0 - 1.0/3.0 };

                    double[] clr = new double[] {0, 0, 0};

                    for (int i=0; i<3; i++)
                    { 
                        if (t3[i]<0) 
                            t3[i]+=1.0; 

                        if (t3[i]>1) 
                            t3[i]-=1.0; 

                        if (6.0*t3[i] < 1.0) 
                            clr[i] = temp1+(temp2-temp1)*t3[i]*6.0; 
                        else if (2.0*t3[i] < 1.0) 
                            clr[i] = temp2; 
                        else if (3.0*t3[i] < 2.0) 
                            clr[i] = (temp1+(temp2-temp1)*((2.0/3.0)-t3[i])*6.0); 
                        else 
                            clr[i] = temp1; 
                    }

                    r=clr[0]; 
                    g=clr[1]; 
                    b=clr[2]; 
                }
                return Color.FromArgb((int)(255*r), (int)(255*g), (int)(255*b));
            }
        }

        public static HSL FromRGB(byte red, byte green, byte blue)
        {
            return FromRGB(Color.FromArgb(red, green, blue));
        }

        public static HSL FromRGB(Color c)
        {
            return new HSL(c.GetHue(), c.GetSaturation(), c.GetBrightness());
        }
    }
}
