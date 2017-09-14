// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Desktop.Platform;
using SQLite;

namespace osu.Desktop.Tests.Platform
{
    public class TestStorage : DesktopStorage
    {
        public TestStorage(string baseName) : base(baseName)
        {
        }

        public override SQLiteConnection GetDatabase(string name) => new SQLiteConnection(@":memory:");
    }
}