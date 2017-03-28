// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;
using System;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableSwell : DrawableTaikoHitObject
    {
        private const float target_ring_thick_border = 4f;
        private const float target_ring_thin_border = 1f;
        private const float target_ring_scale = 5f;
        private const float inner_ring_alpha = 0.35f;

        /// <summary>
        /// The amount of times the user has hit this swell.
        /// </summary>
        private int userHits;

        private readonly Swell swell;

        private readonly Container bodyContainer;
        private readonly CircularContainer targetRing;
        private readonly CircularContainer innerRing;

        public DrawableSwell(Swell swell)
            : base(swell)
        {
            this.swell = swell;

            Children = new Framework.Graphics.Drawable[]
            {
                bodyContainer = new Container
                {
                    Children = new Framework.Graphics.Drawable[]
                    {
                        innerRing = new CircularContainer
                        {
                            Name = "Inner ring",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2),
                            Masking = true,
                            Children = new []
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = inner_ring_alpha,
                                }
                            }
                        },
                        targetRing = new CircularContainer
                        {
                            Name = "Target ring (thick border)",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2),
                            Masking = true,
                            BorderThickness = target_ring_thick_border,
                            Children = new Framework.Graphics.Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    AlwaysPresent = true
                                },
                                new CircularContainer
                                {
                                    Name = "Target ring (thin border)",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    BorderThickness = target_ring_thin_border,
                                    BorderColour = Color4.White,
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
                            }
                        },
                        new SwellCirclePiece(new CirclePiece())
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            innerRing.Colour = colours.YellowDark;
            targetRing.BorderColour = colours.YellowDark.Opacity(0.25f);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LifetimeEnd = double.MaxValue;

            targetRing.Delay(HitObject.StartTime - Time.Current).ScaleTo(target_ring_scale, 600, EasingTypes.OutQuint);
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
            {
                if (Time.Current < HitObject.StartTime)
                    return;

                userHits++;

                innerRing.FadeTo(1);
                innerRing.FadeTo(inner_ring_alpha, 500, EasingTypes.OutQuint);
                innerRing.ScaleTo(1f + (target_ring_scale - 1) * userHits / swell.RequiredHits, 1200, EasingTypes.OutElastic);

                if (userHits == swell.RequiredHits)
                {
                    Judgement.Result = HitResult.Hit;
                    Judgement.TaikoResult = TaikoHitResult.Great;
                }
            }
            else
            {
                if (Judgement.TimeOffset < 0)
                    return;

                if (userHits > swell.RequiredHits / 2)
                {
                    Judgement.Result = HitResult.Hit;
                    Judgement.TaikoResult = TaikoHitResult.Good;
                }
                else
                    Judgement.Result = HitResult.Miss;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    FadeOut(100);
                    Expire();
                    break;
                case ArmedState.Hit:
                    bodyContainer.ScaleTo(1.2f, 400, EasingTypes.OutQuad);

                    FadeOut(600);
                    Expire();
                    break;
            }
        }

        protected override void UpdateScrollPosition(double time)
        {
            base.UpdateScrollPosition(Math.Min(time, HitObject.StartTime));
        }

        protected override bool HandleKeyPress(Key key)
        {
            if (Judgement.Result.HasValue)
                return false;

            UpdateJudgement(true);

            return true;
        }
    }
}
