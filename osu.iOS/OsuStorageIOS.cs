// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.iOS;
using osu.Framework.Platform;
using osu.Game.IO;

namespace osu.iOS
{
    public class OsuStorageIOS : OsuStorage
    {
        private readonly IOSGameHost host;

        public OsuStorageIOS(IOSGameHost host, Storage defaultStorage)
            : base(host, defaultStorage)
        {
            this.host = host;
        }

        public override Storage GetExportStorage() => new IOSStorage(Path.GetTempPath(), host);
    }
}
