﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.IO;

namespace osu.Framework.Platform.MacOS
{
    public class MacOSStorage : DesktopStorage
    {
        public MacOSStorage(string baseName)
            : base(baseName)
        {
        }

        protected override string LocateBasePath()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            string[] paths =
            {
                xdg ?? Path.Combine(home, ".local", "share"),
                Path.Combine(home)
            };

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                    return path;
            }

            return paths[0];
        }
    }
}
