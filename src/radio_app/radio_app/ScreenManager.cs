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
        static int tf_w = 0, tf_h = 0, tf_dot_w = 0;
        static int tf_space = 4;
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
        static Point pos = new Point(0, 0);
        static Point vel = new Point(1, 1);
        static DateTime dt_prev = DateTime.UtcNow.AddHours(-10);

        public static void Init(string config_path)
        {
            Display.Init();

            XDocument config = XDocument.Load(config_path);
            XElement xfonts = config.Root.Element(XName.Get("fonts"));
            timeFont = new Bitmap(xfonts.Element(XName.Get("time")).Value);
            tf_space = int.Parse(xfonts.Element(XName.Get("time")).Attribute(XName.Get("space")).Value);
            tf_h = timeFont.Height;
            tf_w = (timeFont.Width - 10) / 10;
            tf_dot_w = (timeFont.Width - 10) % 10;
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
            if (ShowStation > 0)
            {
                Mode = ScreenMode.StationSelect;
                ShowStation--;
            }
            else if(Mode == ScreenMode.StationSelect)
            {
                Mode = ScreenMode.Home;
            }
            switch (Mode)
            {
                case ScreenMode.Home:
                    {
                        DrawHomeScreen();
                        break;
                    }
                case ScreenMode.StationSelect:
                    {
                        DrawStationList();
                        break;
                    }
            }
        }

        static void Torch_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetTorchMode(false);
        }


        public static void DrawHomeScreen()
        {
            if (Mode != ScreenMode.Home) return;

            DateTime dt = DateTime.UtcNow.AddHours(3);
            string tm = dt.ToString("HH:mm");
            int off = (tm[0] == '0') ? 0 : tf_w + tf_space;
            #region Screensaver
            if ((dt - dt_prev).TotalSeconds > 0.9)
            {
                dt_prev = dt;
                pos.X = pos.X + vel.X;
                pos.Y = pos.Y + vel.Y;

                if(vel.X > 0)
                {
                    int t_w = ((tf_w + tf_space) * 3) + tf_dot_w + off;
                    if(pos.X + t_w > 127)
                    {
                        vel.X = -1;
                        if(pos.X + t_w > 128)
                        {
                            pos.X = 128 - t_w;
                        }
                    }
                }
                else
                {
                    if (pos.X < 1)
                    {
                        vel.X = 1;
                    }
                }
                if(vel.Y > 0)
                {
                    if (pos.Y + tf_h > 63)
                    {
                        vel.Y = -1;
                    }
                }
                else
                {
                    if(pos.Y < 1)
                    {
                        vel.Y = 1;
                    }
                }

            }
            #endregion

            Display.ClearFB();
            int x = 0;
            if (off != 0)
            {
                DrawTimeChar(tm[0], pos.X, pos.Y);
            }
            x += off;
            DrawTimeChar(tm[1], pos.X + x, pos.Y);
            x += tf_w + tf_space;
            if (DateTime.UtcNow.Second % 2 == 0)
            {
                DrawTimeChar(' ', pos.X + x, pos.Y);
            }
            else
            {
                DrawTimeChar(':', pos.X + x, pos.Y);
            }
            x += tf_dot_w + tf_space;
            DrawTimeChar(tm[3], pos.X + x, pos.Y);
            x += tf_w + tf_space;
            DrawTimeChar(tm[4], pos.X + x, pos.Y);
            Display.FlushBuffer();
        }

        public static void DrawStationList()
        {
            Display.ClearFB();
            DrawText(Player.PrevStation.Name, 8, 10);
            DrawText(Player.CurrentStation.Name, 8, 26);
            DrawText(Player.NextStation.Name, 8, 42);
            DrawText("[", 0, 26);
            DrawText("]", 122, 26);
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
                Display.g_fb.DrawImage(timeFont, new Rectangle(x, y, tf_dot_w, tf_h), new Rectangle((tf_w + 1) * 10, 0, tf_dot_w, tf_h), GraphicsUnit.Pixel);
                return;
            }
            if (c == ' ')
            {
                //g_fb.FillRectangle(new SolidBrush(Color.White), new Rectangle(x, y + 4, 4, 12));
                return;
            }
            Display.g_fb.DrawImage(timeFont, new Rectangle(x, y, tf_w, tf_h), new Rectangle((c - '0') * (tf_w + 1), 0, tf_w, tf_h), GraphicsUnit.Pixel);
        }

        public enum ScreenMode
        {
            Bootlogo,
            Home,
            StationSelect,
            Menu,
            Torch,
            None
        }
    }
}
