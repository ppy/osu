// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="HoldNoteTick"/> hit object.
    /// </summary>
    public class DrawableHoldNoteTick : DrawableManiaHitObject<HoldNoteTick>
    {
        /// <summary>
        /// References the time at which the user started holding the hold note.
        /// </summary>
        public Func<double?> HoldStartTime;

        private readonly Container glowContainer;

        public DrawableHoldNoteTick(HoldNoteTick hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1);

            AddRangeInternal(new[]
            {
                glowContainer = new CircularContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                }
            });
        }

        public override Color4 AccentColour
        {
            get => base.AccentColour;
            set
            {
                base.AccentColour = value;

                glowContainer.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 2f,
                    Roundness = 15f,
                    Colour = value.Opacity(0.3f)
                };
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime)
                return;

            var startTime = HoldStartTime?.Invoke();

            if (startTime == null || startTime > HitObject.StartTime)
                ApplyResult(r => r.Type = HitResult.Miss);
            else
                ApplyResult(r => r.Type = HitResult.Perfect);
        }
    }
}
