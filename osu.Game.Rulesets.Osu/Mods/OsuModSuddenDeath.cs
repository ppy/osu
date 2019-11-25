// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSuddenDeath : ModSuddenDeath
    {
        public override string Description => @"不全连，便重试";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModAutopilot)).ToArray();
    }
}
