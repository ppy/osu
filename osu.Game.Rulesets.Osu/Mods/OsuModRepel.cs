// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModRepel : CursorBasedHitObjectPositionMod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Repel";
        public override string Acronym => "RP";
        public override LocalisableString Description => "Hit objects run away!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModMagnetised) }).ToArray();

        [SettingSource("Repulsion strength", "How strong the repulsion is.", 0)]
        public BindableFloat RepulsionStrength { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        protected override double GetDampLength(DrawableHitObject hitObject, Vector2 cursorPos)
        {
            return Vector2.Distance(hitObject.Position, cursorPos) / (0.04 * RepulsionStrength.Value + 0.04);
        }

        protected override Vector2 GetBaseDestination(DrawableHitObject drawable, Vector2 cursorPos)
        {
            var destination = Vector2.Clamp(2 * drawable.Position - cursorPos, Vector2.Zero, OsuPlayfield.BASE_SIZE);

            switch (drawable.HitObject)
            {
                case Slider slider:
                    var possibleMovementBounds = OsuHitObjectGenerationUtils.CalculatePossibleMovementBounds(slider);

                    destination = Vector2.Clamp(
                        destination,
                        new Vector2(possibleMovementBounds.Left, possibleMovementBounds.Top),
                        new Vector2(possibleMovementBounds.Right, possibleMovementBounds.Bottom)
                    );

                    break;
            }

            return destination;
        }
    }
}
