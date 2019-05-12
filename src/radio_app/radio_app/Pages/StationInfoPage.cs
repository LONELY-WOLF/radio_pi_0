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

        public override void Draw()
        {
            base.Draw();

            if (d_cnt <= 0) return;

            int l = Station.Length * ScreenManager.mainFont.Size.Width;
            int off = (l > 128) ? 0 : (128 - l) / 2;

            Display.ClearFB();
            Display.DrawText(Station, off, 26, ScreenManager.mainFont);
            Display.FlushBuffer();
        }
    }
}
