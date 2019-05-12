using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Timers;
using System.Xml.Linq;
using radio_app.Pages;

namespace radio_app
{
    public static class ScreenManager
    {
        //public static ScreenMode Mode = ScreenMode.None;
        static Bitmap bootlogo;
        public static BitFont mainFont;
        static int anim_pause = 100;
        static string[] mainCharset = { " !\"#$%&'()*+,-./",
                                        "0123456789:;<=>?",
                                        "@ABCDEFGHIJKLMNO",
                                        "PQRSTUVWXYZ[\\]^_",
                                        "`abcdefghijklmno",
                                        "pqrstuvwxyz{|}~ " };
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
            mainFont = new BitFont(xfonts.Element(XName.Get("main")), mainCharset);

            XElement bootgif = config.Root.Element(XName.Get("bootlogo"));
            bootlogo = new Bitmap(bootgif.Value);
            anim_pause = int.Parse(bootgif.Attribute(XName.Get("pause")).Value);

            XElement torch = config.Root.Element(XName.Get("torch"));
            byte torch_brightness = byte.Parse(torch.Attribute(XName.Get("brightness")).Value);
            int torch_duration = int.Parse(torch.Attribute(XName.Get("duration")).Value);

            home_timer = new Timer(1000);
            home_timer.Elapsed += Home_Timer_Elapsed;
            home_timer.Start();

            PageTable.HomePage = new HomePage(timeFont, tf_space);
            PageTable.TorchPage = new TorchPage(torch_duration, torch_brightness);
            PageTable.BootLogoPage = new BootLogoPage();
            PageTable.StationSelectPage = new StationSelectPage();
            PageTable.StationInfoPage = new StationInfoPage();
        }

        static void Home_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PageTable.CurrentPage.Draw();
        }

        public static void BeginAnimation()
        {
            SetPage(PageTable.BootLogoPage);
            anim_thread = new System.Threading.Thread(ShowAnimation);
            anim_thread.Start();
        }

        public static void EndAnimation()
        {
            anim_thread.Join();
            SetPage(PageTable.HomePage);
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

        static void SetPage(ScreenPage page)
        {
            PageTable.CurrentPage = page;
        }
    }
}
