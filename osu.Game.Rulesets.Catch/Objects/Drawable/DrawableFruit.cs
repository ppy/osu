// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableFruit : DrawableScrollingHitObject<CatchBaseHit>
    {
        private const float pulp_size = 30;

        private class Pulp : Circle, IHasAccentColour
        {
            public Pulp()
            {
                Size = new Vector2(pulp_size);

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 5,
                    Colour = AccentColour.Opacity(0.5f),
                };
            }

            public Color4 AccentColour { get; set; } = Color4.White;
        }


        public DrawableFruit(CatchBaseHit h)
            : base(h)
        {
            Origin = Anchor.Centre;
            Size = new Vector2(pulp_size * 2, pulp_size * 2.6f);

            RelativePositionAxes = Axes.Both;
            X = h.Position;

            Colour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1);

            Rotation = (float)(RNG.NextDouble() - 0.5f) * 40;
        }

        public Func<CatchBaseHit, bool> CheckPosition;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Framework.Graphics.Drawable[]
            {
                //todo: share this more
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CacheDrawnFrameBuffer = true,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Scale = new Vector2(0.6f),
                        },
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Y = -0.08f
                        },
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Y = -0.08f
                        },
                        new Pulp
                        {
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                        },
                    }
                }
            };
        }

        private const float preempt = 1000;

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (timeOffset > 0)
                AddJudgement(new Judgement { Result = CheckPosition?.Invoke(HitObject) ?? false ? HitResult.Perfect : HitResult.Miss });
        }

        protected override void UpdateState(ArmedState state)
        {
            using (BeginAbsoluteSequence(HitObject.StartTime - preempt))
            {
                // animation
                this.FadeIn(200);
            }

            switch (state)
            {
                case ArmedState.Miss:
                    using (BeginAbsoluteSequence(HitObject.StartTime, true))
                        this.FadeOut(250).RotateTo(Rotation * 2, 250, Easing.Out);
                    break;
            }
        }
    }
}
