// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Localisation;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModRepel : Mod, IUpdatableByPlayfield, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Repel";
        public override string Acronym => "RP";
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Hit objects run away!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModAutopilot), typeof(OsuModWiggle), typeof(OsuModTransform), typeof(ModAutoplay), typeof(OsuModMagnetised), typeof(OsuModBubbles), typeof(OsuModDepth) };

        [SettingSource("Repulsion strength", "How strong the repulsion is.", 0)]
        public BindableFloat RepulsionStrength { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            // Hide follow points as they won't make any sense.
            // Judgements can potentially be turned on in a future where they display at a position relative to their drawable counterpart.
            (drawableRuleset.Playfield as OsuPlayfield)?.FollowPoints.Hide();
        }

        public void Update(Playfield playfield)
        {
            var cursorPos = playfield.Cursor.AsNonNull().ActiveCursor.DrawPosition;

            foreach (var entry in playfield.HitObjectContainer.AliveEntries)
            {
                var drawable = entry.Value;

                var destination = Vector2.Clamp(2 * drawable.Position - cursorPos, Vector2.Zero, OsuPlayfield.BASE_SIZE);

                if (drawable.HitObject is Slider thisSlider)
                {
                    var possibleMovementBounds = OsuHitObjectGenerationUtils.CalculatePossibleMovementBounds(thisSlider);

                    destination = Vector2.Clamp(
                        destination,
                        new Vector2(possibleMovementBounds.Left, possibleMovementBounds.Top),
                        new Vector2(possibleMovementBounds.Right, possibleMovementBounds.Bottom)
                    );
                }

                switch (drawable)
                {
                    case DrawableHitCircle circle:
                        easeTo(playfield.Clock, circle, destination, cursorPos);
                        break;

                    case DrawableSlider slider:

                        if (!slider.HeadCircle.Result.HasResult)
                            easeTo(playfield.Clock, slider, destination, cursorPos);
                        else
                            easeTo(playfield.Clock, slider, destination - slider.Ball.DrawPosition, cursorPos);

                        break;
                }
            }
        }

        private void easeTo(IFrameBasedClock clock, DrawableHitObject hitObject, Vector2 destination, Vector2 cursorPos)
        {
            double dampLength = Vector2.Distance(hitObject.Position, cursorPos) / (0.04 * RepulsionStrength.Value + 0.04);

            float x = (float)Interpolation.DampContinuously(hitObject.X, destination.X, dampLength, clock.ElapsedFrameTime);
            float y = (float)Interpolation.DampContinuously(hitObject.Y, destination.Y, dampLength, clock.ElapsedFrameTime);

            hitObject.Position = new Vector2(x, y);
        }
    }
}
