// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public sealed class MultiMod : Mod
    {
        public override string Name => string.Empty;
        public override string Acronym => string.Empty;
        public override LocalisableString Description => string.Empty;
        public override double ScoreMultiplier => 0;

        public Mod[] Mods { get; }

        public MultiMod(params Mod[] mods)
        {
            Mods = mods;
        }

        public override Mod DeepClone() => new MultiMod(Mods.Select(m => m.DeepClone()).ToArray());

        public override Type[] IncompatibleMods => Mods.SelectMany(m => m.IncompatibleMods).ToArray();
    }
}
