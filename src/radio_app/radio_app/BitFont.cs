using System;
using System.Drawing;
using System.Xml.Linq;

namespace radio_app
{
    public class BitFont
    {
        public Bitmap Image;
        Size size = new Size();
        string[] charset;

        public BitFont(XElement config, string[] charset)
        {
            this.charset = charset;
            Image = new Bitmap(config.Value);
            size.Width = Image.Width / charset[0].Length;
            size.Height = Image.Height / charset.Length;
        }

        public Size Size { get => size; }

        public Rectangle GetChar(char c)
        {
            for (int cy = 0; cy < charset.Length; cy++)
            {
                int cx = charset[cy].IndexOf(c);
                if (cx == -1) continue;
                return new Rectangle(new Point(cx * size.Width, cy * size.Height), size);
            }
            return Rectangle.Empty;
        }
    }
}
