// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModGrow : OsuModObjectScaleTween
    {
        public override string Name => "生长";

        public override string Acronym => "GR";

        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAltV;

        public override LocalisableString Description => "在正确的大小击打物件!";

        public override BindableNumber<float> StartScale { get; } = new BindableFloat(0.5f)
        {
            MinValue = 0f,
            MaxValue = 0.99f,
            Precision = 0.01f,
        };
    }
}
