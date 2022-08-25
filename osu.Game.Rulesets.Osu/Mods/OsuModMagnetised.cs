// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModMagnetised : CursorBasedHitObjectPositionMod, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Magnetised";
        public override string Acronym => "MG";
        public override IconUsage? Icon => FontAwesome.Solid.Magnet;
        public override LocalisableString Description => "No need to chase the circles – your cursor is a magnet!";
        public override double ScoreMultiplier => 0.5;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModRelax), typeof(OsuModRepel) }).ToArray();

        [SettingSource("Attraction strength", "How strong the pull is.", 0)]
        public BindableFloat AttractionStrength { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        protected override double GetDampLength(DrawableHitObject hitObject, Vector2 cursorPos)
        {
            return Interpolation.Lerp(3000, 40, AttractionStrength.Value);
        }

        protected override Vector2 GetBaseDestination(DrawableHitObject drawable, Vector2 cursorPos)
        {
            return cursorPos;
        }
    }
}
