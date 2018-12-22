using System;
using System.Threading;
using System.Linq;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace radio_app
{
    class MainClass
    {
        static int a_frame = 0;
        static Bitmap bootlogo;
        static bool showStation = false;

        public static void Main(string[] args)
        {
            bootlogo = new Bitmap("bootlogo.gif");
            Display.Init();

            Thread anim_thread = new Thread(ShowAnimation);
            anim_thread.Start();

            Player.LoadXML("config.xml");

            var hidDeviceList = DeviceList.Local.GetHidDevices(6421, 4136).ToArray();

            //Media
            HidDevice dev = hidDeviceList.First(d => d.GetMaxInputReportLength() == 5);
            {
                try
                {
                    var reportDescriptor = dev.GetReportDescriptor();
                    HidStream hidStream;
                    if (dev.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();

                        inputReceiver.Received += InputReceiverM_Received;
                        inputReceiver.Start(hidStream);
                    }
                    else
                    {
                        Console.WriteLine("Failed to open device.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            //Qwerty
            dev = hidDeviceList.First(d => d.GetMaxInputReportLength() == 9);
            {
                try
                {
                    var reportDescriptor = dev.GetReportDescriptor();
                    HidStream hidStream;
                    if (dev.TryOpen(out hidStream))
                    {
                        hidStream.ReadTimeout = Timeout.Infinite;

                        var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();

                        inputReceiver.Received += InputReceiverQ_Received;
                        inputReceiver.Start(hidStream);
                    }
                    else
                    {
                        Console.WriteLine("Failed to open device.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            anim_thread.Join();

            Timer t = new Timer(TimerCallback, null, 0, 1000);

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        private static void TimerCallback(Object o)
        {
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
            Display.DrawTimeChar(tm[0], x, y);
            Display.DrawTimeChar(tm[1], x + 20, y);
            if (DateTime.UtcNow.Second % 2 == 0)
            {
                Display.DrawTimeChar(' ', x + 40, y);
            }
            else
            {
                Display.DrawTimeChar(':', x + 40, y);
            }
            Display.DrawTimeChar(tm[3], x + 48, y);
            Display.DrawTimeChar(tm[4], x + 68, y);
            Display.DrawText(dt.ToString("dd/MM"), x + 88, y + 22);
            if (Player.IsPlaying || showStation)
            {
                Display.DrawText(Player.Current, 0, 52);
                showStation = false;
            }
            Display.FlushBuffer();
        }

        static void InputReceiverM_Received(object sender, EventArgs e)
        {
            Report report;
            HidDeviceInputReceiver inputReceiver = sender as HidDeviceInputReceiver;
            byte[] inputReportBuffer = new byte[5];
            while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
            {
                if (inputReportBuffer[1] != 0)
                {
                    switch (inputReportBuffer[1])
                    {
                        case 35:
                            {
                                Console.WriteLine("Home");
                                break;
                            }
                        case 36:
                            {
                                Console.WriteLine("Back");
                                break;
                            }
                        case 205:
                            {
                                Player.PlayStop();
                                break;
                            }
                        case 226:
                            {
                                Process.Start("amixer", "-q sset \"Line Out\" toggle");
                                break;
                            }
                        case 233:
                            {
                                Process.Start("amixer", "-q -M sset \"Line Out\" 1%+");
                                break;
                            }
                        case 234:
                            {
                                Process.Start("amixer", "-q -M sset \"Line Out\" 1%-");
                                break;
                            }
                    }
                }
            }
        }

        static void InputReceiverQ_Received(object sender, EventArgs e)
        {
            Report report;
            HidDeviceInputReceiver inputReceiver = sender as HidDeviceInputReceiver;
            byte[] inputReportBuffer = new byte[9];
            while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
            {
                if (inputReportBuffer[3] != 0)
                {
                    Console.WriteLine(inputReportBuffer[3]);
                    switch (inputReportBuffer[3])
                    {
                        case 40:
                            {
                                Console.WriteLine("Ok");
                                break;
                            }
                        case 75:
                            {
                                Console.WriteLine("PgUp");
                                Player.Next();
                                showStation = true;
                                break;
                            }
                        case 78:
                            {
                                Console.WriteLine("PgDn");
                                Player.Prev();
                                showStation = true;
                                break;
                            }
                        case 79:
                            {
                                Console.WriteLine("Right");
                                break;
                            }
                        case 80:
                            {
                                Console.WriteLine("Left");
                                break;
                            }
                        case 81:
                            {
                                Console.WriteLine("Down");
                                break;
                            }
                        case 82:
                            {
                                Console.WriteLine("Up");
                                break;
                            }
                        case 101:
                            {
                                Console.WriteLine("Menu");
                                break;
                            }
                    }
                }
            }
        }

        static void ShowAnimation()
        {
            for (a_frame = 0; a_frame <= 100; a_frame++)
            {
                bootlogo.SelectActiveFrame(FrameDimension.Time, a_frame);
                Display.DrawDirect(bootlogo);
                a_frame += 1;
                Thread.Sleep(100);
            }
        }
    }
}
