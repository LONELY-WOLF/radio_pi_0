using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace radio_app
{
    public static class Player
    {
        static List<PlayItem> playlist = new List<PlayItem>();
        static PlayItem _current;
        static Process ffplay;
        const string ff_args = "-loglevel quiet -nodisp -reconnect 1 -reconnect_at_eof 1 -reconnect_streamed 1 -reconnect_delay_max 300 ";

        public static void LoadXML(string path)
        {
            XDocument config = XDocument.Load(path);
            XElement xplaylist = config.Root.Element(XName.Get("playlist"));
            XName x_name = XName.Get("name");
            XName x_path = XName.Get("path");
            foreach (XElement item in xplaylist.Elements())
            {
                playlist.Add(new PlayItem { Name = item.Attribute(x_name).Value, Path = item.Attribute(x_path).Value });
            }
            _current = playlist[0];
        }

        static void ChangeItem()
        {
            if (IsPlaying)
            {
                PlayItem();
            }
        }

        static void PlayItem()
        {
            if (ffplay != null)
            {
                ffplay.Refresh();
                if (!ffplay.HasExited)
                {
                    ffplay.Kill();
                    ffplay.WaitForExit();
                }
            }
            ffplay = Process.Start("ffplay", ff_args + _current.Path);
            IsPlaying = true;
        }

        public static void PlayStop()
        {
            if (ffplay != null)
            {
                ffplay.Refresh();
                if (ffplay.HasExited)
                {
                    ffplay = Process.Start("ffplay", ff_args + _current.Path);
                    IsPlaying = true;
                }
                else
                {
                    ffplay.Kill();
                    IsPlaying = false;
                }
            }
            else
            {
                ffplay = Process.Start("ffplay", ff_args + _current.Path);
                IsPlaying = true;
            }
        }

        public static void Next()
        {
            _current = NextStation;
            ChangeItem();
        }

        public static void Prev()
        {
            _current = PrevStation;
            ChangeItem();
        }

        public static bool IsPlaying { get; private set; } = false;

        public static PlayItem CurrentStation
        {
            get => _current;
        }

        public static PlayItem NextStation
        {
            get
            {
                int i = playlist.IndexOf(_current);
                if (i < playlist.Count - 1)
                {
                    i++;
                }
                else
                {
                    i = 0;
                }
                return playlist[i];
            }
        }

        public static PlayItem PrevStation
        {
            get
            {
                int i = playlist.IndexOf(_current);
                if (i > 0)
                {
                    i--;
                }
                else
                {
                    i = playlist.Count - 1;
                }
                return playlist[i];
            }
        }

        public static bool PlayStation(int n)
        {
            if(n >= playlist.Count)
            {
                return false;
            }
            _current = playlist[n];
            PlayItem();
            return true;
        }
    }

    public struct PlayItem
    {
        public string Name, Path;
    }
}
