// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using OpenTK.Input;
using osu.Framework.Input;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;
using osu.Framework.Allocation;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableBash : DrawableTaikoHitObject
    {
        private const float outer_scale = 5f;

        public override Color4 ExplodeColour => new Color4(237, 171, 0, 255);

        protected virtual List<Key> Keys { get; } = new List<Key>(new[] { Key.D, Key.F, Key.J, Key.K });

        private HitCirclePiece bodyPiece;

        private Container bashInnerRing;
        private Container bashOuterRing;

        private int userHits;
        private bool ringsVisible;

        public DrawableBash(Bash spinner)
            : base(spinner)
        {
            Size = new Vector2(128);

            Children = new Drawable[]
            {
                bashOuterRing = new CircularContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Scale = new Vector2(outer_scale),
                    Alpha = 0f,

                    BorderThickness = 4,
                    BorderColour = new Color4(237, 171, 0, 25),

                    Children = new Drawable[]
                    {
                        new Box()
                        {
                            RelativeSizeAxes = Axes.Both,

                            Alpha = 0,
                            AlwaysPresent = true
                        },
                        new CircularContainer()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            RelativeSizeAxes = Axes.Both,

                            BorderThickness = 1,
                            BorderColour = Color4.White,

                            Children = new[]
                            {
                                new Box()
                                {
                                    RelativeSizeAxes = Axes.Both,

                                    Alpha = 0,
                                    AlwaysPresent = true
                                }
                            }
                        }
                    }
                },
                bashInnerRing = new CircularContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Alpha = 0f,

                    BorderThickness = 1,
                    BorderColour = Color4.White,

                    Children = new[]
                    {
                        new Box()
                        {
                            RelativeSizeAxes = Axes.Both,

                            Colour = new Color4(204, 102, 0, 255),
                            Alpha = 0.3f,
                            AlwaysPresent = true
                        }
                    }
                },
                bodyPiece = new SpinnerPiece()
                {
                    Kiai = spinner.Kiai
                },
            };
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (Judgement.Result.HasValue)
                return false;

            if (!Keys.Contains(args.Key))
                return false;

            if (Time.Current < HitObject.StartTime)
                return false;

            UpdateJudgement(true);

            return true;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            Bash spinner = HitObject as Bash;
            TaikoJudgementInfo taikoJudgement = Judgement as TaikoJudgementInfo;

            if (userTriggered)
            {
                if (Time.Current < HitObject.StartTime)
                    return;

                userHits++;

                bashInnerRing.ScaleTo(1f + (outer_scale - 1) * userHits / spinner.RequiredHits, 50);

                if (userHits == spinner.RequiredHits)
                {
                    Judgement.Result = HitResult.Hit;
                    taikoJudgement.Score = TaikoScoreResult.Great;
                }

            }
            else
            {
                if (Judgement.TimeOffset < 0)
                    return;

                if (userHits > spinner.RequiredHits / 2)
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

            const double scale_out = 150;

            switch (State)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    break;
                case ArmedState.Hit:
                    bodyPiece.ScaleTo(1.5f, scale_out);
                    bodyPiece.FadeOut(scale_out);

                    bashOuterRing.FadeOut(scale_out);
                    bashInnerRing.FadeOut(scale_out);
                    break;
            }
        }

        double lastAutoHitTime;
        protected override void UpdateAuto()
        {
            Bash spinner = HitObject as Bash;
            if (spinner.RequiredHits > 0 && (Time.Current - lastAutoHitTime) >= HitObject.Duration / spinner.RequiredHits)
            {
                UpdateJudgement(true);

                lastAutoHitTime += HitObject.Duration / spinner.RequiredHits;
            }
        }

        protected override void Update()
        {
            MoveToOffset(Math.Min(Time.Current, HitObject.StartTime));

            UpdateAuto();

            if (Time.Current >= HitObject.StartTime && !ringsVisible)
            {
                bashOuterRing.FadeIn(200);
                bashInnerRing.FadeIn(200);
                ringsVisible = true;
            }
        }
    }
}
