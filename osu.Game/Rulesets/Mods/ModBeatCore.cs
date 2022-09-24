// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModBeatCore : Mod
    {
        public override string Name => "节拍";
        public override string Acronym => "BC";
        public override double ScoreMultiplier => 1;
        public override LocalisableString Description => "在正常的速度动次打次动次打次...";
        public override ModType Type => ModType.Fun;
        public override Type[] IncompatibleMods => new[] { typeof(ModNightcore) };
        public override bool UserPlayable => false;
    }

    public abstract class ModBeatCore<TObject> : ModBeatCore, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new NightcoreBeatContainer());
        }
    }
}
