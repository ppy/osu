// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModGrow : OsuModObjectScaleTween
    {
        public override string Name => "Grow";

        public override string Acronym => "GR";

        public override IconUsage? Icon => OsuIcon.ModGrow;

        public override LocalisableString Description => ModSelectOverlayStrings.OsuModGrowDescription;

        public override BindableNumber<float> StartScale { get; } = new BindableFloat(0.5f)
        {
            MinValue = 0f,
            MaxValue = 0.99f,
            Precision = 0.01f,
        };
    }
}
