// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModRelax : ModRelax
    {
        public override LocalisableString Description => @"No ninja-like spinners, demanding drumrolls or unexpected katus.";

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(TaikoModSingleTap) }).ToArray();
    }
}
