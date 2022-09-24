// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModGrow : OsuModObjectScaleTween
    {
        public override string Name => "生长";

        public override string Acronym => "GR";

        public override IconUsage? Icon => FontAwesome.Solid.ArrowsAltV;

        public override LocalisableString Description => "在正确的大小击打物件!";

        [SettingSource("Starting Size", "The initial size multiplier applied to all objects.")]
        public override BindableNumber<float> StartScale { get; } = new BindableFloat
        {
            MinValue = 0f,
            MaxValue = 0.99f,
            Default = 0.5f,
            Value = 0.5f,
            Precision = 0.01f,
        };
    }
}
