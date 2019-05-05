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
        static Bitmap fb;
        static Graphics g_fb;
        static Mutex mtx = new Mutex();

        public static void Init()
        {
            mtx.WaitOne();

            fb = new Bitmap(128, 64);
            g_fb = Graphics.FromImage(fb);
            g_fb.InterpolationMode = InterpolationMode.NearestNeighbor;
            g_fb.SmoothingMode = SmoothingMode.None;
            g_fb.PixelOffsetMode = PixelOffsetMode.Half;
            g_fb.CompositingQuality = CompositingQuality.AssumeLinear;

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
            SendCmd(0x81, 0x00); // Contrast = 0
            SendCmd(0xD9, 0x27); // Pre-charge 2 up, 7 down
            SendCmd(0xDB, 0x00); // Vcomm = 0
            ClearFB();
            FlushBuffer();
            SendCmd(0xAF);

            mtx.ReleaseMutex();
        }

        static bool Reset
        {
            set => File.WriteAllText("/sys/class/gpio/gpio10/value", value ? "1" : "0");
        }

        static bool CommandMode
        {
            set
            {
                File.WriteAllText("/sys/class/gpio/gpio2/value", value ? "0" : "1");
                Thread.Sleep(1);
            }
        }

        static void SendCmd(byte cmd)
        {
            CommandMode = true;
            spi.WriteByte(cmd);
            spi.Flush();
        }

        static void SendCmd(byte cmd0, byte cmd1)
        {
            CommandMode = true;
            spi.WriteByte(cmd0);
            spi.WriteByte(cmd1);
            spi.Flush();
        }

        static void SendCmd(byte cmd0, byte cmd1, byte cmd2)
        {
            CommandMode = true;
            spi.WriteByte(cmd0);
            spi.WriteByte(cmd1);
            spi.WriteByte(cmd2);
            spi.Flush();
        }

        public static void FlushBuffer()
        {
            mtx.WaitOne();

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

            mtx.ReleaseMutex();
        }

        public static void Clear()
        {
            mtx.WaitOne();

            CommandMode = false;
            for (int i = 0; i < 1024; i++)
            {
                spi.WriteByte(0);
            }
            spi.Flush();

            mtx.ReleaseMutex();
        }

        public static void ClearFB()
        {
            mtx.WaitOne();

            g_fb.Clear(Color.White);

            mtx.ReleaseMutex();
        }

        public static void DrawImage(Bitmap img, int x, int y)
        {
            mtx.WaitOne();

            g_fb.DrawImageUnscaled(img, x, y);

            mtx.ReleaseMutex();
        }

        public static void DrawImage(Bitmap image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
        {
            mtx.WaitOne();

            g_fb.DrawImage(image, destRect, srcRect, srcUnit);

            mtx.ReleaseMutex();
        }

        public static void DrawText(string text, int x, int y, BitFont font)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Rectangle r = font.GetChar(text[i]);
                DrawImage(font.Image, new Rectangle(x + (i * r.Size.Width), y, r.Size.Width, r.Size.Height), r, GraphicsUnit.Pixel);
            }
        }

        public static void DrawDirect(Bitmap img)
        {
            mtx.WaitOne();

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

            mtx.ReleaseMutex();
        }

        public static void SetContrast(byte value)
        {
            mtx.WaitOne();

            SendCmd(0x81, value);

            mtx.ReleaseMutex();
        }

        public static void SetAllOn(bool on)
        {
            mtx.WaitOne();

            if(on)
            {
                SendCmd(0xA5);
            }
            else
            {
                SendCmd(0xA4);
            }

            mtx.ReleaseMutex();
        }
    }
}
