using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace radio_app
{
    public static class Player
    {
        static List<PlayItem> playlist = new List<PlayItem>();
        static bool _isplaying = false;
        static PlayItem _current;
        static Process ffplay;
        const string ff_args = "-loglevel quiet -nodisp ";

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
            if (!_isplaying) return;
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
        }

        public static void PlayStop()
        {
            if (ffplay != null)
            {
                ffplay.Refresh();
                if (ffplay.HasExited)
                {
                    ffplay = Process.Start("ffplay", ff_args + _current.Path);
                    _isplaying = true;
                }
                else
                {
                    ffplay.Kill();
                    _isplaying = false;
                }
            }
            else
            {
                ffplay = Process.Start("ffplay", ff_args + _current.Path);
                _isplaying = true;
            }
        }

        public static void Next()
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
            _current = playlist[i];
            ChangeItem();
        }

        public static void Prev()
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
            _current = playlist[i];
            ChangeItem();
        }

        public static bool IsPlaying
        {
            get => _isplaying;
        }

        public static string Current
        {
            get => _current.Name;
        }
    }

    struct PlayItem
    {
        public string Name, Path;
    }
}
