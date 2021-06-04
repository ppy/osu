// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Framework.Testing;

namespace osu.Game.Configuration
{
    [ExcludeFromDynamicCompile]
    public class DevelopmentOsuConfigManager : OsuConfigManager
    {
        protected override string Filename => base.Filename.Replace(".ini", ".dev.ini");

        public DevelopmentOsuConfigManager(Storage storage)
            : base(storage)
        {
        }
    }
}
