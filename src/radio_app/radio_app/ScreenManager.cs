using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Timers;
using System.Xml.Linq;

namespace radio_app
{
    public static class ScreenManager
    {
        public static ScreenMode Mode = ScreenMode.None;
        static Bitmap timeFont, mainFont, bootlogo;
        static int anim_pause = 100;
        static string mainCharset = " !\"#$%&'()*+,-./" +
                                    "0123456789:;<=>?" +
                                    "@ABCDEFGHIJKLMNO" +
                                    "PQRSTUVWXYZ[\\]^_" +
                                    "`abcdefghijklmno" +
                                    "pqrstuvwxyz{|}~ ";
        static Timer home_timer, torch_timer;
        public static byte ShowStation = 0;
        static System.Threading.Thread anim_thread;
        static int a_frame = 0;
        static byte torch_brightness = 128;

        public static void Init(string config_path)
        {
            Display.Init();

            XDocument config = XDocument.Load(config_path);
            XElement xfonts = config.Root.Element(XName.Get("fonts"));
            timeFont = new Bitmap(xfonts.Element(XName.Get("time")).Value);
            mainFont = new Bitmap(xfonts.Element(XName.Get("main")).Value);

            XElement bootgif = config.Root.Element(XName.Get("bootlogo"));
            bootlogo = new Bitmap(bootgif.Value);
            anim_pause = int.Parse(bootgif.Attribute(XName.Get("pause")).Value);

            XElement torch = config.Root.Element(XName.Get("torch"));
            torch_brightness = byte.Parse(torch.Attribute(XName.Get("brightness")).Value);
            int torch_duration = int.Parse(torch.Attribute(XName.Get("duration")).Value);

            home_timer = new Timer(1000);
            home_timer.Elapsed += Home_Timer_Elapsed;
            home_timer.Start();

            torch_timer = new Timer(torch_duration * 1000);
            torch_timer.AutoReset = false;
            torch_timer.Elapsed += Torch_Timer_Elapsed;
        }

        static void Home_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DrawHomeScreen();
        }

        static void Torch_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetTorchMode(false);
        }


        public static void DrawHomeScreen()
        {
            if (Mode != ScreenMode.Home) return;

            DateTime dt = DateTime.UtcNow.AddHours(3);
            #region Screensaver
            int x = 0, y = 0;
            int m = dt.Minute;
            if (m < 19)
            {
                x = 0;
                y = m;
            }
            else if (m < 30)
            {
                x = m - 19;
                y = 19;
            }
            else if (m < 49)
            {
                x = 11;
                y = 49 - m;
            }
            else if (m < 60)
            {
                x = 60 - m;
                y = 0;
            }
            #endregion
            string tm = dt.ToString("HH:mm");

            Display.ClearFB();
            DrawTimeChar(tm[0], x, y);
            DrawTimeChar(tm[1], x + 20, y);
            if (DateTime.UtcNow.Second % 2 == 0)
            {
                DrawTimeChar(' ', x + 40, y);
            }
            else
            {
                DrawTimeChar(':', x + 40, y);
            }
            DrawTimeChar(tm[3], x + 48, y);
            DrawTimeChar(tm[4], x + 68, y);
            DrawText(dt.ToString("dd/MM"), x + 88, y + 22);
            if (Player.IsPlaying || (ShowStation > 0))
            {
                DrawText(Player.Current, 0, 52);
                if (ShowStation > 0)
                {
                    ShowStation--;
                }
            }
            Display.FlushBuffer();
        }

        public static void BeginAnimation()
        {
            Mode = ScreenMode.Bootlogo;
            anim_thread = new System.Threading.Thread(ShowAnimation);
            anim_thread.Start();
        }

        public static void EndAnimation()
        {
            anim_thread.Join();
            Mode = ScreenMode.Home;
        }

        static void ShowAnimation()
        {
            if (Mode != ScreenMode.Bootlogo) return;

            int f_cnt = bootlogo.GetFrameCount(FrameDimension.Time);
            for (a_frame = 0; a_frame <= f_cnt; a_frame++)
            {
                bootlogo.SelectActiveFrame(FrameDimension.Time, a_frame);
                Display.DrawDirect(bootlogo);
                a_frame += 1;
                System.Threading.Thread.Sleep(anim_pause);
            }
        }

        static public void SetTorchMode(bool on)
        {
            if (on)
            {
                Mode = ScreenMode.Torch;
                Display.SetAllOn(true);
                Display.SetContrast(torch_brightness);
                torch_timer.Start();
            }
            else
            {
                torch_timer.Stop();
                Display.SetContrast(0x00);
                Display.SetAllOn(false);
                Mode = ScreenMode.Home;
            }
        }

        public static void DrawText(string text, int x, int y)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int cx = mainCharset.IndexOf(text[i]);
                if (cx == -1) continue;
                int cy = cx / 16;
                cx = cx % 16;
                Display.g_fb.DrawImage(mainFont, new Rectangle(x + (i * 6), y, 6, 12), new Rectangle(cx * 6, cy * 12, 6, 12), GraphicsUnit.Pixel);
            }
        }

        public static void DrawTimeChar(char c, int x, int y)
        {
            if (c == ':')
            {
                Display.g_fb.DrawImage(timeFont, new Rectangle(x, y, 4, 32), new Rectangle(170, 0, 4, 32), GraphicsUnit.Pixel);
                return;
            }
            if (c == ' ')
            {
                //g_fb.FillRectangle(new SolidBrush(Color.White), new Rectangle(x, y + 4, 4, 12));
                return;
            }
            Display.g_fb.DrawImage(timeFont, new Rectangle(x, y, 16, 32), new Rectangle((c - '0') * 17, 0, 16, 32), GraphicsUnit.Pixel);
        }

        public enum ScreenMode
        {
            Bootlogo,
            Home,
            Menu,
            Torch,
            None
        }
    }
}
