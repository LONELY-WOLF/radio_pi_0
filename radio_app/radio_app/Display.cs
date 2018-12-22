using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace radio_app
{
    public static class Display
    {
        static FileStream spi;
        static Bitmap fb, timeFont, textFont;
        static Graphics g_fb;
        static string charset = " !\"#$%&'()*+,-./" +
                                "0123456789:;<=>?" +
                                "@ABCDEFGHIJKLMNO" +
                                "PQRSTUVWXYZ[\\]^_" +
                                "`abcdefghijklmno" +
                                "pqrstuvwxyz{|}~ ";

        public static void Init()
        {
            fb = new Bitmap(128, 64);
            g_fb = Graphics.FromImage(fb);
            g_fb.InterpolationMode = InterpolationMode.NearestNeighbor;
            g_fb.SmoothingMode = SmoothingMode.None;
            g_fb.PixelOffsetMode = PixelOffsetMode.Half;
            g_fb.CompositingQuality = CompositingQuality.AssumeLinear;
            timeFont = new Bitmap("time_font.png");
            textFont = new Bitmap("Dangen_charset_6x12.png");

            //GPIO Init
            if (!File.Exists("/sys/class/gpio/gpio2/direction"))
            {
                File.WriteAllText("/sys/class/gpio/export", "2");
                File.WriteAllText("/sys/class/gpio/export", "10");
            }

            File.WriteAllText("/sys/class/gpio/gpio2/direction", "out");
            File.WriteAllText("/sys/class/gpio/gpio10/direction", "out");

            //SSD1309 Init
            spi = new FileStream("/dev/spidev1.0", FileMode.Open, FileAccess.ReadWrite);
            Reset = false;
            Thread.Sleep(10);
            Reset = true;
            Thread.Sleep(10);
            SendCmd(0xAE);
            SendCmd(0x2E);
            SendCmd(0xFD, 0x12);
            SendCmd(0x5D, 0x80);
            SendCmd(0xA8, 0x3F);
            SendCmd(0x40);
            SendCmd(0xD3, 0x00);
            SendCmd(0x20, 0x00);
            SendCmd(0xC0);
            SendCmd(0xA0);
            SendCmd(0xDA, 0x12);
            SendCmd(0xA6);
            SendCmd(0xA4);
            SendCmd(0x81, 0x00);
            ClearFB();
            FlushBuffer();
            SendCmd(0xAF);
        }

        public static bool Reset
        {
            set => File.WriteAllText("/sys/class/gpio/gpio10/value", value ? "1" : "0");
        }

        public static bool CommandMode
        {
            set
            {
                File.WriteAllText("/sys/class/gpio/gpio2/value", value ? "0" : "1");
                Thread.Sleep(1);
            }
        }

        public static void SendCmd(byte cmd)
        {
            CommandMode = true;
            spi.WriteByte(cmd);
            spi.Flush();
        }

        public static void SendCmd(byte cmd0, byte cmd1)
        {
            CommandMode = true;
            spi.WriteByte(cmd0);
            spi.WriteByte(cmd1);
            spi.Flush();
        }

        public static void SendCmd(byte cmd0, byte cmd1, byte cmd2)
        {
            CommandMode = true;
            spi.WriteByte(cmd0);
            spi.WriteByte(cmd1);
            spi.WriteByte(cmd2);
            spi.Flush();
        }

        public static void FlushBuffer()
        {
            SendCmd(0x21, 0, 127);
            SendCmd(0x22, 0, 7);

            CommandMode = false;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    byte b = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (fb.GetPixel(x, y * 8 + i).R < 128)
                        {
                            b |= (byte)(1 << i);
                        }
                    }
                    spi.WriteByte(b);
                }
            }
            spi.Flush();
        }

        public static void Clear()
        {
            CommandMode = false;
            for (int i = 0; i < 1024; i++)
            {
                spi.WriteByte(0);
            }
            spi.Flush();
        }

        public static void DrawTimeChar(char c, int x, int y)
        {
            if (c == ':')
            {
                g_fb.DrawImage(timeFont, new Rectangle(x, y, 4, 32), new Rectangle(170, 0, 4, 32), GraphicsUnit.Pixel);
                return;
            }
            if (c == ' ')
            {
                //g_fb.FillRectangle(new SolidBrush(Color.White), new Rectangle(x, y + 4, 4, 12));
                return;
            }
            g_fb.DrawImage(timeFont, new Rectangle(x, y, 16, 32), new Rectangle((c - '0') * 17, 0, 16, 32), GraphicsUnit.Pixel);
        }

        public static void ClearFB()
        {
            g_fb.Clear(Color.White);
        }

        public static void DrawText(string text, int x, int y)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int cx = charset.IndexOf(text[i]);
                if (cx == -1) continue;
                int cy = cx / 16;
                cx = cx % 16;
                g_fb.DrawImage(textFont, new Rectangle(x + (i * 6), y, 6, 12), new Rectangle(cx * 6, cy * 12, 6, 12), GraphicsUnit.Pixel);
            }
        }

        public static void DrawImage(Bitmap img, int x, int y)
        {
            g_fb.DrawImageUnscaled(img, x, y);
        }

        public static void DrawDirect(Bitmap img)
        {
            SendCmd(0x21, 0, 127);
            SendCmd(0x22, 0, 7);

            CommandMode = false;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    byte b = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (img.GetPixel(x, y * 8 + i).R < 128)
                        {
                            b |= (byte)(1 << i);
                        }
                    }
                    spi.WriteByte(b);
                }
            }
            spi.Flush();
        }
    }
}
