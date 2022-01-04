// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;

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
        private Func<double?> holdStartTime;

        private Container glowContainer;

        public DrawableHoldNoteTick()
            : this(null)
        {
        }

        public DrawableHoldNoteTick(HoldNoteTick hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(glowContainer = new CircularContainer
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
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccentColour.BindValueChanged(colour =>
            {
                glowContainer.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 2f,
                    Roundness = 15f,
                    Colour = colour.NewValue.Opacity(0.3f)
                };
            }, true);
        }

        protected override void OnApply()
        {
            base.OnApply();

            Debug.Assert(ParentHitObject != null);

            var holdNote = (DrawableHoldNote)ParentHitObject;
            holdStartTime = () => holdNote.HoldStartTime;
        }

        protected override void OnFree()
        {
            base.OnFree();

            holdStartTime = null;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < HitObject.StartTime)
                return;

            double? startTime = holdStartTime?.Invoke();

            if (startTime == null || startTime > HitObject.StartTime)
                ApplyResult(r => r.Type = r.Judgement.MinResult);
            else
                ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }
    }
}
