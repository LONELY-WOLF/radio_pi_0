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
        public static void Main(string[] args)
        {
            ScreenManager.Init("config.xml");
            ScreenManager.BeginAnimation();

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

            ScreenManager.EndAnimation();

            while (true)
            {
                Thread.Sleep(100);
            }
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
                                ScreenManager.DrawHomeScreen();
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
                                ScreenManager.ShowStation = 3;
                                ScreenManager.DrawHomeScreen();
                                break;
                            }
                        case 78:
                            {
                                Console.WriteLine("PgDn");
                                Player.Prev();
                                ScreenManager.ShowStation = 3;
                                ScreenManager.DrawHomeScreen();
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
    }
}
