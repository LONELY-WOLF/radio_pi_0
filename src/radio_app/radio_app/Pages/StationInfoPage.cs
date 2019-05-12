using System;
namespace radio_app.Pages
{
    public class StationInfoPage : TimeoutPage
    {
        public string Station = "";

        public StationInfoPage()
        {
        }

        public override void Enter()
        {
            base.Enter();
            Draw();
        }

        public override int Draw()
        {
            if (base.Draw() != 0) return -1;

            int l = Station.Length * ScreenManager.mainFont.Size.Width;
            int off = (l > 128) ? 0 : (128 - l) / 2;

            Display.ClearFB();
            Display.DrawText(Station, off, 26, ScreenManager.mainFont);
            Display.FlushBuffer();
            return 0;
        }
    }
}
