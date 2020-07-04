using System;
using System.Drawing;

namespace radio_app.Pages
{
    public class HomePage : ScreenPage
    {
        Point pos = new Point(0, 0);
        Point vel = new Point(1, 1);
        DateTime dt_prev = DateTime.UtcNow.AddHours(-10);
        Bitmap timeFont;
        int tf_w = 0, tf_h = 0, tf_dot_w = 0;
        int tf_space = 4;

        public HomePage(Bitmap timeFont, int space_width)
        {
            this.timeFont = timeFont;
            tf_h = timeFont.Height;
            tf_w = (timeFont.Width - 10) / 10;
            tf_dot_w = (timeFont.Width - 10) % 10;

        }

        public override void Enter()
        {
            base.Enter();
            Draw();
        }

        public override void Exit()
        {

        }

        public override int Draw()
        {
            if (base.Draw() != 0) return -1;

            DateTime dt = DateTime.UtcNow.AddHours(3);
            string tm = dt.ToString("HH:mm");
            int off = (tm[0] == '0') ? 0 : tf_w + tf_space;
            #region Screensaver
            if ((dt - dt_prev).TotalSeconds > 0.9)
            {
                dt_prev = dt;
                pos.X = pos.X + vel.X;
                pos.Y = pos.Y + vel.Y;

                if (vel.X > 0)
                {
                    int t_w = ((tf_w + tf_space) * 3) + tf_dot_w + off;
                    if (pos.X + t_w > 127)
                    {
                        vel.X = -1;
                        if (pos.X + t_w > 128)
                        {
                            pos.X = 128 - t_w;
                        }
                    }
                }
                else
                {
                    if (pos.X < 1)
                    {
                        vel.X = 1;
                    }
                }
                if (vel.Y > 0)
                {
                    if (pos.Y + tf_h > 63)
                    {
                        vel.Y = -1;
                    }
                }
                else
                {
                    if (pos.Y < 1)
                    {
                        vel.Y = 1;
                    }
                }

            }
            #endregion

            Display.ClearFB();
            int x = 0;
            if (off != 0)
            {
                DrawTimeChar(tm[0], pos.X, pos.Y);
            }
            x += off;
            DrawTimeChar(tm[1], pos.X + x, pos.Y);
            x += tf_w + tf_space;
            if (DateTime.UtcNow.Second % 2 == 0)
            {
                DrawTimeChar(' ', pos.X + x, pos.Y);
            }
            else
            {
                DrawTimeChar(':', pos.X + x, pos.Y);
            }
            x += tf_dot_w + tf_space;
            DrawTimeChar(tm[3], pos.X + x, pos.Y);
            x += tf_w + tf_space;
            DrawTimeChar(tm[4], pos.X + x, pos.Y);
            Display.FlushBuffer();
            return 0;
        }

        public override void InputM(byte key)
        {
            switch (key)
            {
                case 205:
                    {
                        if(Player.IsPlaying)
                        {
                            PageTable.StationInfoPage.Station = "Radio: off";
                        }
                        else
                        {
                            PageTable.StationInfoPage.Station = Player.CurrentStation.Name;
                        }
                        PageTable.StationInfoPage.Enter();
                        break;
                    }
            }
            base.InputM(key);
        }

        public override void InputQ(byte key)
        {
            if (key >= 30 && key <= 39)
            {
                int n = key - 30;
                if (Player.PlayStation(n))
                {
                    PageTable.StationInfoPage.Station = Player.CurrentStation.Name;
                    PageTable.StationInfoPage.Enter();
                }
            }
            switch (key)
            {
                case 40: //OK
                    {
                        PageTable.TorchPage.Enter();
                        break;
                    }
                case 75: //PgUp
                    {
                        Player.Prev();
                        PageTable.StationSelectPage.Enter();
                        break;
                    }
                case 78: //PgDn
                    {
                        Player.Next();
                        PageTable.StationSelectPage.Enter();
                        break;
                    }
                default:
                    {
                        base.InputQ(key);
                        break;
                    }
            }
        }

        private void DrawTimeChar(char c, int x, int y)
        {
            if (c == ':')
            {
                Display.DrawImage(timeFont, new Rectangle(x, y, tf_dot_w, tf_h), new Rectangle((tf_w + 1) * 10, 0, tf_dot_w, tf_h), GraphicsUnit.Pixel);
                return;
            }
            if (c == ' ')
            {
                //g_fb.FillRectangle(new SolidBrush(Color.White), new Rectangle(x, y + 4, 4, 12));
                return;
            }
            Display.DrawImage(timeFont, new Rectangle(x, y, tf_w, tf_h), new Rectangle((c - '0') * (tf_w + 1), 0, tf_w, tf_h), GraphicsUnit.Pixel);
        }
    }
}
