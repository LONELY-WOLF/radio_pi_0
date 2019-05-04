using System;
using System.Timers;

namespace radio_app.Modes
{
    public class TorchMode : ScreenMode
    {
        byte torch_brightness;
        Timer torch_timer;

        public TorchMode(int torch_duration, byte torch_brightness)
        {
            this.torch_brightness = torch_brightness;
            torch_timer = new Timer(torch_duration * 1000);
            torch_timer.AutoReset = false;
            torch_timer.Elapsed += Torch_Timer_Elapsed;
        }

        public override void Enter()
        {
            base.Enter();

            Display.SetAllOn(true);
            Display.SetContrast(torch_brightness);
            torch_timer.Start();
        }

        public override void Exit()
        {
            torch_timer.Stop();
            Display.SetContrast(0x00);
            Display.SetAllOn(false);
        }

        public override void InputQ(byte key)
        {
            switch (key)
            {
                case 40: //OK
                    {
                        ModeTable.HomeMode.Enter();
                        break;
                    }
                case 75: //PgUp
                    {
                        Player.Prev();
                        break;
                    }
                case 78: //PgDn
                    {
                        Player.Next();
                        break;
                    }
                default:
                    {
                        base.InputQ(key);
                        break;
                    }
            }
        }

        void Torch_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ModeTable.HomeMode.Enter();
        }
    }
}
