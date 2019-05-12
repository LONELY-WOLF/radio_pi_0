﻿using System;
using System.Diagnostics;

namespace radio_app.Pages
{
    public class ScreenPage
    {
        protected ScreenPage pPage = null;

        public ScreenPage()
        {
        }

        public virtual void Enter()
        {
            PageTable.CurrentPage.Exit();
            pPage = PageTable.CurrentPage;
            PageTable.CurrentPage = this;
        }

        public virtual void Exit()
        {

        }

        public virtual int Draw()
        {
            if (PageTable.CurrentPage != this) return -1;
            return 0;
        }

        public virtual void InputM(byte key)
        {
            switch (key)
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

        public virtual void InputQ(byte key)
        {
            switch (key)
            {
                case 40: //OK
                    {
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
                case 79: //Right
                    {
                        break;
                    }
                case 80: //Left
                    {
                        break;
                    }
                case 81: //Down
                    {
                        break;
                    }
                case 82: //Up
                    {
                        break;
                    }
                case 101: //Menu
                    {
                        break;
                    }
            }
        }
    }
}
