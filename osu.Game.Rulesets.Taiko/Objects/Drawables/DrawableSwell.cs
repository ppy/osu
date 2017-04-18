// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableSwell : DrawableTaikoHitObject<Swell>
    {
        /// <summary>
        /// Invoked when the swell has reached the hit target, i.e. when CurrentTime >= StartTime.
        /// This is only ever invoked once.
        /// </summary>
        public event Action OnStart;

        private const float target_ring_thick_border = 1.4f;
        private const float target_ring_thin_border = 1f;
        private const float target_ring_scale = 5f;
        private const float inner_ring_alpha = 0.65f;

        private readonly Container bodyContainer;
        private readonly CircularContainer targetRing;
        private readonly CircularContainer expandingRing;

        private readonly CirclePiece circlePiece;

        private readonly Key[] rimKeys = { Key.D, Key.K };
        private readonly Key[] centreKeys = { Key.F, Key.J };
        private Key[] lastKeySet;

        /// <summary>
        /// The amount of times the user has hit this swell.
        /// </summary>
        private int userHits;

        private bool hasStarted;
        private readonly SwellSymbolPiece symbol;

        public DrawableSwell(Swell swell)
            : base(swell)
        {
            Children = new Drawable[]
            {
                bodyContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        expandingRing = new CircularContainer
                        {
                            Name = "Expanding ring",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Size = new Vector2(TaikoHitObject.DEFAULT_CIRCLE_DIAMETER),
                            BlendingMode = BlendingMode.Additive,
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
                            Size = new Vector2(TaikoHitObject.DEFAULT_CIRCLE_DIAMETER),
                            Masking = true,
                            BorderThickness = target_ring_thick_border,
                            BlendingMode = BlendingMode.Additive,
                            Children = new Drawable[]
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
                        circlePiece = new CirclePiece
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new []
                            {
                                symbol = new SwellSymbolPiece()
                            }
                        }
                    }
                }
            };

            circlePiece.KiaiMode = HitObject.Kiai;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            circlePiece.AccentColour = colours.YellowDark;
            expandingRing.Colour = colours.YellowLight;
            targetRing.BorderColour = colours.YellowDark.Opacity(0.25f);
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (userTriggered)
            {
                userHits++;

                var completion = (float)userHits / HitObject.RequiredHits;

                expandingRing.FadeTo(expandingRing.Alpha + MathHelper.Clamp(completion / 16, 0.1f, 0.6f), 50);
                expandingRing.Delay(50);
                expandingRing.FadeTo(completion / 8, 2000, EasingTypes.OutQuint);
                expandingRing.DelayReset();

                symbol.RotateTo((float)(completion * HitObject.Duration / 8), 4000, EasingTypes.OutQuint);

                expandingRing.ScaleTo(1f + Math.Min(target_ring_scale - 1f, (target_ring_scale - 1f) * completion * 1.3f), 260, EasingTypes.OutQuint);

                if (userHits == HitObject.RequiredHits)
                {
                    Judgement.Result = HitResult.Hit;
                    Judgement.TaikoResult = TaikoHitResult.Great;
                }
            }
            else
            {
                if (Judgement.TimeOffset < 0)
                    return;

                //TODO: THIS IS SHIT AND CAN'T EXIST POST-TAIKO WORLD CUP
                if (userHits > HitObject.RequiredHits / 2)
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
            const float preempt = 100;

            Delay(HitObject.StartTime - Time.Current - preempt, true);

            targetRing.ScaleTo(target_ring_scale, preempt * 4, EasingTypes.OutQuint);

            Delay(preempt, true);

            Delay(Judgement.TimeOffset + HitObject.Duration, true);

            const float out_transition_time = 300;

            switch (state)
            {
                case ArmedState.Hit:
                    bodyContainer.ScaleTo(1.4f, out_transition_time);
                    break;
            }

            FadeOut(out_transition_time, EasingTypes.Out);

            Expire();
        }

        protected override void UpdateScrollPosition(double time)
        {
            // Make the swell stop at the hit target
            double t = Math.Min(HitObject.StartTime, time);

            if (t == HitObject.StartTime && !hasStarted)
            {
                OnStart?.Invoke();
                hasStarted = true;
            }

            base.UpdateScrollPosition(t);
        }

        protected override bool HandleKeyPress(Key key)
        {
            if (Judgement.Result != HitResult.None)
                return false;

            // Don't handle keys before the swell starts
            if (Time.Current < HitObject.StartTime)
                return false;

            // Find the keyset which this key corresponds to
            var keySet = rimKeys.Contains(key) ? rimKeys : centreKeys;

            // Ensure alternating keysets
            if (keySet == lastKeySet)
                return false;
            lastKeySet = keySet;

            UpdateJudgement(true);

            return true;
        }
    }
}
