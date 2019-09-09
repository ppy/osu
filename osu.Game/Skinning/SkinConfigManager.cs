// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;

namespace osu.Game.Skinning
{
    public class SkinConfigManager<T> : ConfigManager<T> where T : struct
    {
        protected override void PerformLoad()
        {
        }

        protected override bool PerformSave() => false;
    }
}
