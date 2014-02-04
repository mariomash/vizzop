using System;
using System.Drawing;

namespace vizzopWeb
{
    public class ThumbCreator
    {

        public enum VerticalAlign
        {
            Top,
            Middle,
            Bottom
        }

        public enum HorizontalAlign
        {
            Left,
            Middle,
            Right
        }

        public Bitmap Resize(Bitmap img, int height, int width, VerticalAlign valign, HorizontalAlign halign)
        {
            if (height == 0)
            {
                float ratio = (float)img.Height / (float)img.Width;
                height = Convert.ToInt16(width * ratio);
            }
            if (width == 0)
            {
                float ratio = (float)img.Width / (float)img.Height;
                width = Convert.ToInt16(height * ratio);
            }
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                float ratio = (float)height / (float)img.Height;
                int temp = (int)((float)img.Width * ratio);
                if (temp == width)
                {
                    //no corrections are needed!
                    g.DrawImage(img, 0, 0, width, height);
                    return result;
                }
                else if (temp > width)
                {
                    //den e för bred!
                    int overFlow = (temp - width);
                    if (halign == HorizontalAlign.Middle)
                    {
                        g.DrawImage(img, 0 - overFlow / 2, 0, temp, height);
                    }
                    else if (halign == HorizontalAlign.Left)
                    {
                        g.DrawImage(img, 0, 0, temp, height);
                    }
                    else if (halign == HorizontalAlign.Right)
                    {
                        g.DrawImage(img, -overFlow, 0, temp, height);
                    }
                }
                else
                {
                    //den e för hög!
                    ratio = (float)width / (float)img.Width;
                    temp = (int)((float)img.Height * ratio);
                    int overFlow = (temp - height);
                    if (valign == VerticalAlign.Top)
                    {
                        g.DrawImage(img, 0, 0, width, temp);
                    }
                    else if (valign == VerticalAlign.Middle)
                    {
                        g.DrawImage(img, 0, -overFlow / 2, width, temp);
                    }
                    else if (valign == VerticalAlign.Bottom)
                    {
                        g.DrawImage(img, 0, -overFlow, width, temp);
                    }
                }
            }
            return result;
        }
    }
}