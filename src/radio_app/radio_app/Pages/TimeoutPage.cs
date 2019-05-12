using System;
namespace radio_app.Pages
{
    public class TimeoutPage : ScreenPage
    {
        public int Duration = 3;
        internal int d_cnt = 3;

        public TimeoutPage()
        {
        }

        public override void Enter()
        {
            d_cnt = Duration;
            base.Enter();
        }

        public override int Draw()
        {
            if (base.Draw() != 0) return -1;

            if (d_cnt-- == 0)
            {
                pPage.Enter();
                return -1;
            }
            return 0;
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
                case 36:
                    {
                        pPage.Enter();
                        break;
                    }
                default:
                    {
                        base.InputM(key);
                        break;
                    }
            }
        }
    }
}
