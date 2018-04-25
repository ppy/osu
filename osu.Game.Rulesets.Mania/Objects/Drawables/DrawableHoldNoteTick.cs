// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
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

            InternalChildren = new[]
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
            };
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
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

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
                return;

            if (Time.Current < HitObject.StartTime)
                return;

            if (HoldStartTime?.Invoke() > HitObject.StartTime)
                return;

            AddJudgement(new HoldNoteTickJudgement { Result = HitResult.Perfect });
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (State.Value)
            {
                case ArmedState.Hit:
                    AccentColour = Color4.Green;
                    break;
            }
        }

        protected override void Update()
        {
            if (AllJudged)
                return;

            if (HoldStartTime?.Invoke() == null)
                return;

            UpdateJudgement(true);
        }
    }
}
