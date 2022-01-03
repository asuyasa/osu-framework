﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Platform;
using osu.Framework.Platform.Linux;
using osu.Framework.Platform.MacOS;
using osu.Framework.Platform.Windows;

namespace osu.Framework
{
    public static class Host
    {
        [Obsolete("Use GetSuitableHost(HostConfig) instead.")]
        public static DesktopGameHost GetSuitableHost(string gameName, bool bindIPC = false, bool portableInstallation = false)
        {
            return GetSuitableHost(new HostConfig
            {
                Name = gameName,
                BindIPC = bindIPC,
                PortableInstallation = portableInstallation,
            });
        }

        public static DesktopGameHost GetSuitableHost(HostConfig hostConfig)
        {
            switch (RuntimeInfo.OS)
            {
                case RuntimeInfo.Platform.Windows:
                    return new WindowsGameHost(hostConfig);

                case RuntimeInfo.Platform.Linux:
                    return new LinuxGameHost(hostConfig);

                case RuntimeInfo.Platform.macOS:
                    return new MacOSGameHost(hostConfig);

                default:
                    throw new InvalidOperationException($"Could not find a suitable host for the selected operating system ({RuntimeInfo.OS}).");
            }
        }
    }
}
