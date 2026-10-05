// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDeflate : OsuModObjectScaleTween
    {
        public override string Name => "Deflate";

        public override string Acronym => "DF";

        public override IconUsage? Icon => OsuIcon.ModDeflate;

        public override LocalisableString Description => "Hit them at the right size!";

        public override BindableNumber<float> StartScale { get; } = new BindableFloat(2)
        {
            MinValue = 1f,
            MaxValue = 25f,
            Precision = 0.1f,
        };
    }
}
