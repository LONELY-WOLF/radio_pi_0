using System;
namespace radio_app.Pages
{
    public class StationSelectPage : ScreenPage
    {
        int duration;

        public StationSelectPage()
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
            if (PageTable.CurrentPage != this) return;

            if (duration-- == 0)
            {
                pPage.Enter();
                return;
            }
            Display.ClearFB();
            Display.DrawText(Player.PrevStation.Name, 8, 10, ScreenManager.mainFont);
            Display.DrawText(Player.CurrentStation.Name, 8, 26, ScreenManager.mainFont);
            Display.DrawText(Player.NextStation.Name, 8, 42, ScreenManager.mainFont);
            Display.DrawText("[", 0, 26, ScreenManager.mainFont);
            Display.DrawText("]", 122, 26, ScreenManager.mainFont);
            Display.FlushBuffer();
        }

        public override void InputM(byte key)
        {
            switch (key)
            {
                case 35:
                    {
                        PageTable.HomePage.Enter();
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
