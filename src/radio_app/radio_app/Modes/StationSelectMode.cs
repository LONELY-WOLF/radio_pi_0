using System;
namespace radio_app.Modes
{
    public class StationSelectMode : ScreenMode
    {
        int duration;

        public StationSelectMode()
        {
        }

        public override void Enter()
        {
            duration = 3;
            base.Enter();
            Draw();
        }

        public override void Draw()
        {
            if (ModeTable.CurrentMode != this) return;

            if (duration-- == 0)
            {
                pMode.Enter();
                return;
            }
            Display.ClearFB();
            ScreenManager.DrawText(Player.PrevStation.Name, 8, 10);
            ScreenManager.DrawText(Player.CurrentStation.Name, 8, 26);
            ScreenManager.DrawText(Player.NextStation.Name, 8, 42);
            ScreenManager.DrawText("[", 0, 26);
            ScreenManager.DrawText("]", 122, 26);
            Display.FlushBuffer();
        }

        public override void InputM(byte key)
        {
            switch (key)
            {
                case 35:
                    {
                        ModeTable.HomeMode.Enter();
                        break;
                    }
                default:
                    {
                        base.InputM(key);
                        break;
                    }
            }
        }

        public override void InputQ(byte key)
        {
            switch (key)
            {
                case 75: //PgUp
                    {
                        Player.Prev();
                        duration = 3;
                        Draw();
                        break;
                    }
                case 78: //PgDn
                    {
                        Player.Next();
                        duration = 3;
                        Draw();
                        break;
                    }
            }
        }
    }
}
