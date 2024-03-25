// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTouchDevice : ModTouchDevice
    {
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot) }).ToArray();
        public override bool EligibleForPP => UsesDefaultConfiguration;
    }
}
