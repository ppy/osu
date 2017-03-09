﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces.Circle;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Bashes
{
    public class DrawableBash : DrawableTaikoHitObject
    {
        /// <summary>
        /// Scale of the outer ring.
        /// </summary>
        private const float outer_ring_scale = 5f;

        public override Color4 ExplodeColour { get; protected set; }

        /// <summary>
        /// A list of keys which this HitObject will accept. These are the standard Taiko keys for now.
        /// These should be moved to bindings later.
        /// </summary>
        private List<Key> validKeys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private CirclePiece bodyPiece;

        private Container bashInnerRing;
        private CircularContainer bashOuterRing;
        private Box innerRingBackground;

        private int userHits;

        public DrawableBash(Bash spinner)
            : base(spinner)
        {
            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2);

            Children = new Drawable[]
            {
                // Outer ring (the one that the inner ring scales to)
                bashOuterRing = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Scale = new Vector2(outer_ring_scale),
                    Alpha = 0f,

                    BorderThickness = 4,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,

                            Alpha = 0,
                            AlwaysPresent = true
                        },
                        // Outer ring internal border
                        new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            BorderThickness = 1,
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
                // Inner ring (the one that scales up)
                bashInnerRing = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Alpha = 0f,

                    BorderThickness = 1,
                    BorderColour = Color4.White,

                    Children = new[]
                    {
                        // Inner ring background
                        innerRingBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,

                            Alpha = 0f,
                            AlwaysPresent = true
                        }
                    }
                },
                // Inner circle
                bodyPiece = new BashCirclePiece
                {
                    Kiai = spinner.Kiai
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            ExplodeColour = colours.YellowDark;
            bashOuterRing.BorderColour = colours.YellowDark.Opacity(25);
            innerRingBackground.Colour = colours.YellowDark;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            bashOuterRing.Delay(HitObject.StartTime - Time.Current).FadeIn(200);
            bashInnerRing.Delay(HitObject.StartTime - Time.Current).FadeIn(200);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (Judgement.Result.HasValue)
                return false;

            if (!validKeys.Contains(args.Key))
                return false;

            UpdateJudgement(true);

            return true;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            Bash bash = (Bash)HitObject;
            TaikoJudgementInfo taikoJudgement = (TaikoJudgementInfo)Judgement;

            if (userTriggered)
            {
                if (Time.Current < HitObject.StartTime)
                    return;

                userHits++;

                innerRingBackground.FadeTo((float)userHits / bash.RequiredHits, 250, EasingTypes.OutQuint);
                bashInnerRing.ScaleTo(1f + (outer_ring_scale - 1) * userHits / bash.RequiredHits, 50);

                if (userHits == bash.RequiredHits)
                {
                    taikoJudgement.Result = HitResult.Hit;
                    taikoJudgement.Score = TaikoScoreResult.Great;
                }

            }
            else
            {
                if (Judgement.TimeOffset < 0)
                    return;

                if (userHits > bash.RequiredHits / 2)
                {
                    Judgement.Result = HitResult.Hit;
                    taikoJudgement.Score = TaikoScoreResult.Good;
                }
                else
                    Judgement.Result = HitResult.Miss;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            base.UpdateState(state);

            switch (State)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    bodyPiece.FadeColour(Color4.Red, 100, EasingTypes.OutQuint);
                    bodyPiece.FadeOut(100);

                    bashOuterRing.FadeColour(Color4.Red, 100, EasingTypes.OutQuint);
                    bashOuterRing.FadeOut(100);

                    bashInnerRing.FadeColour(Color4.Red, 100, EasingTypes.OutQuint);
                    bashInnerRing.FadeOut(100);
                    break;
                case ArmedState.Hit:
                    bodyPiece.ScaleTo(1.5f, 150, EasingTypes.OutQuint);
                    bodyPiece.FadeOut(150);

                    bashOuterRing.ScaleTo(1f, 100, EasingTypes.OutQuint);
                    bashOuterRing.FadeOut(100);

                    bashInnerRing.ScaleTo(1f, 100, EasingTypes.OutQuint);
                    bashInnerRing.FadeOut(100);
                    break;
            }
        }

        protected override void Update()
        {
            MoveToOffset(Math.Min(Time.Current, HitObject.StartTime));
        }
    }
}
