using System;
using System.Drawing;

namespace radio_app.Modes
{
    public class HomeMode : ScreenMode
    {
        Point pos = new Point(0, 0);
        Point vel = new Point(1, 1);
        DateTime dt_prev = DateTime.UtcNow.AddHours(-10);
        Bitmap timeFont;
        int tf_w = 0, tf_h = 0, tf_dot_w = 0;
        int tf_space = 4;

        public HomeMode(Bitmap timeFont, int space_width)
        {
            this.timeFont = timeFont;
            tf_h = timeFont.Height;
            tf_w = (timeFont.Width - 10) / 10;
            tf_dot_w = (timeFont.Width - 10) % 10;

        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {

        }

        public override void Draw()
        {
            if (ModeTable.CurrentMode != this) return;

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
        }

        public override void InputQ(byte key)
        {
            switch (key)
            {
                case 40: //OK
                    {
                        ModeTable.TorchMode.Enter();
                        break;
                    }
                case 75: //PgUp
                    {
                        Player.Prev();
                        ModeTable.StationSelectMode.Enter();
                        break;
                    }
                case 78: //PgDn
                    {
                        Player.Next();
                        ModeTable.StationSelectMode.Enter();
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
