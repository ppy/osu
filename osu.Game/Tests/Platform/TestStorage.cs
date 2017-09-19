﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Platform;
using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;
using SQLite.Net.Platform.Win32;

namespace osu.Game.Tests.Platform
{
    public class TestStorage : DesktopStorage
    {
        public TestStorage(string baseName) : base(baseName)
        {
        }

        public override SQLiteConnection GetDatabase(string name)
        {
            ISQLitePlatform platform;
            if (RuntimeInfo.IsWindows)
                platform = new SQLitePlatformWin32();
            else
                platform = new SQLitePlatformGeneric();
            return new SQLiteConnection(platform, @":memory:");
        }
    }
}