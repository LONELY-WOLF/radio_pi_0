using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Timers;
using System.Xml.Linq;
using radio_app.Modes;

namespace radio_app
{
    public static class ScreenManager
    {
        //public static ScreenMode Mode = ScreenMode.None;
        static Bitmap mainFont, bootlogo;
        static int anim_pause = 100;
        static string mainCharset = " !\"#$%&'()*+,-./" +
                                    "0123456789:;<=>?" +
                                    "@ABCDEFGHIJKLMNO" +
                                    "PQRSTUVWXYZ[\\]^_" +
                                    "`abcdefghijklmno" +
                                    "pqrstuvwxyz{|}~ ";
        static Timer home_timer;
        public static byte ShowStation = 0;
        static System.Threading.Thread anim_thread;
        static int a_frame = 0;

        public static void Init(string config_path)
        {
            Display.Init();

            XDocument config = XDocument.Load(config_path);
            XElement xfonts = config.Root.Element(XName.Get("fonts"));
            Bitmap timeFont = new Bitmap(xfonts.Element(XName.Get("time")).Value);
            int tf_space = int.Parse(xfonts.Element(XName.Get("time")).Attribute(XName.Get("space")).Value);
            mainFont = new Bitmap(xfonts.Element(XName.Get("main")).Value);

            XElement bootgif = config.Root.Element(XName.Get("bootlogo"));
            bootlogo = new Bitmap(bootgif.Value);
            anim_pause = int.Parse(bootgif.Attribute(XName.Get("pause")).Value);

            XElement torch = config.Root.Element(XName.Get("torch"));
            byte torch_brightness = byte.Parse(torch.Attribute(XName.Get("brightness")).Value);
            int torch_duration = int.Parse(torch.Attribute(XName.Get("duration")).Value);

            home_timer = new Timer(1000);
            home_timer.Elapsed += Home_Timer_Elapsed;
            home_timer.Start();

            ModeTable.HomeMode = new HomeMode(timeFont, tf_space);
            ModeTable.TorchMode = new TorchMode(torch_duration, torch_brightness);
            ModeTable.BootLogoMode = new BootLogoMode();
            ModeTable.StationSelectMode = new StationSelectMode();
        }

        static void Home_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ModeTable.CurrentMode.Draw();
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
            SetMode(ModeTable.BootLogoMode);
            anim_thread = new System.Threading.Thread(ShowAnimation);
            anim_thread.Start();
        }

        public static void EndAnimation()
        {
            anim_thread.Join();
            SetMode(ModeTable.HomeMode);
        }

        static void ShowAnimation()
        {
            //if (Mode != ScreenMode.Bootlogo) return;

            int f_cnt = bootlogo.GetFrameCount(FrameDimension.Time);
            for (a_frame = 0; a_frame <= f_cnt; a_frame++)
            {
                bootlogo.SelectActiveFrame(FrameDimension.Time, a_frame);
                Display.DrawDirect(bootlogo);
                a_frame += 1;
                System.Threading.Thread.Sleep(anim_pause);
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

        static void SetMode(ScreenMode mode)
        {
            ModeTable.CurrentMode = mode;
        }
    }
}
