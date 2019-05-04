using System;
using System.Threading;
using System.Linq;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using radio_app.Modes;

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
                    Console.WriteLine(inputReportBuffer[1]);
                    ModeTable.CurrentMode.InputM(inputReportBuffer[1]);
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
                    ModeTable.CurrentMode.InputQ(inputReportBuffer[3]);
                }
            }
        }
    }
}
