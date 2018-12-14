// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Android;
using osu.Framework.Platform;
using osu.Game;
using System;
using System.IO;

namespace osu.Android
{
    internal class OsuGameAndroid : OsuGame
    {
        public OsuGameAndroid() : base()
        {

        }
        public override Storage GetStorageForStableInstall()
        {
            return new OpsuStorage();
        }

        // For better migration from opsu! to osu!lazer (WIP)
        private class OpsuStorage : AndroidStorage
        {
            bool checkExists(string p) => Directory.Exists(Path.Combine(p, "Songs"));

            protected override string LocateBasePath()
            {
                BasePath = base.LocateBasePath();
                string opsuInstallPath = Path.Combine(BasePath, "opsu");
                Console.WriteLine(opsuInstallPath);
                if (checkExists(opsuInstallPath))
                    return opsuInstallPath;
                return null;
            }

            public OpsuStorage() : base(string.Empty, null)
            {
                BasePath = LocateBasePath();
            }
        }
    }
}
