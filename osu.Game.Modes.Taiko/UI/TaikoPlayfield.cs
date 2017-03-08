// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables.BarLines;
using osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls;
using osu.Game.Modes.UI;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoPlayfield : Playfield<TaikoHitObject>
    {
        /// <summary>
        /// The default play field height.
        /// </summary>
        public const float PLAYFIELD_BASE_HEIGHT = 242;

        /// <summary>
        /// The play field height scale.
        /// </summary>
        public const float PLAYFIELD_SCALE = 0.65f;

        /// <summary>
        /// The play field height after scaling.
        /// </summary>
        public static float PlayfieldHeight => PLAYFIELD_BASE_HEIGHT * PLAYFIELD_SCALE;

        protected override Container<Drawable> Content => hitObjectContainer;

        private const float hit_target_offset = 80;

        private static float left_area_size = 240;

        private HitTarget hitTarget;
        private Container<ExplodingRing> explosionRingContainer;
        private Container<DrawableBarLine> barLineContainer;
        private Container<JudgementText> judgementContainer;

        private Container rightBackgroundContainer;
        private Box rightBackground;
        private Container leftBackgroundContainer;
        private Box leftBackground;
        private Container hitObjectContainer;
        private Container topLevelHitContainer;

        public TaikoPlayfield()
        {
            RelativeSizeAxes = Axes.X;
            Height = PlayfieldHeight;

            AddInternal(new Drawable[]
            {
                rightBackgroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    BorderThickness = 2,
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.2f),
                        Radius = 5,
                    },
                    Children = new Drawable[]
                    {
                        rightBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.6f
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = left_area_size },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Padding = new MarginPadding { Left = hit_target_offset },
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Name = @"Hit target",
                                    RelativeSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        explosionRingContainer = new Container<ExplodingRing>
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.Centre,
                                            Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2),
                                            Scale = new Vector2(PLAYFIELD_SCALE),
                                            BlendingMode = BlendingMode.Additive
                                        },
                                    }
                                },
                                barLineContainer = new Container<DrawableBarLine>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                hitTarget = new HitTarget
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.Centre,
                                },
                                hitObjectContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                judgementContainer = new Container<JudgementText>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    BlendingMode = BlendingMode.Additive
                                },
                            },
                        },
                    }
                },
                leftBackgroundContainer = new Container
                {
                    Size = new Vector2(left_area_size, PlayfieldHeight),
                    BorderThickness = 1,
                    Children = new Drawable[]
                    {
                        leftBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.X,
                            Position = new Vector2(0.10f, 0),
                            Children = new Drawable[]
                            {
                                new InputDrum
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Scale = new Vector2(0.9f)
                                },
                            }
                        },
                        new Box
                        {
                            Anchor = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 10,
                            ColourInfo = Framework.Graphics.Colour.ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.6f), Color4.Black.Opacity(0)),
                        },
                    }
                },
                topLevelHitContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            leftBackgroundContainer.BorderColour = colours.Gray0;
            leftBackground.Colour = colours.Gray1;

            rightBackgroundContainer.BorderColour = colours.Gray1;
            rightBackground.Colour = colours.Gray0;
        }

        public override void Add(DrawableHitObject<TaikoHitObject> h)
        {
            h.Scale = new Vector2(PLAYFIELD_SCALE);

            h.OnJudgement += onJudgement;

            base.Add(h);
        }

        public void AddBarLine(DrawableBarLine b)
        {
            b.Scale = new Vector2(PLAYFIELD_SCALE);

            barLineContainer.Add(b);
        }

        private void onJudgement(DrawableHitObject h, JudgementInfo j)
        {
            TaikoJudgementInfo taikoJudgement = (TaikoJudgementInfo)j;
            DrawableTaikoHitObject taikoObject = (DrawableTaikoHitObject)h;

            if (taikoObject == null)
                return;

            // Add ring
            ExplodingRing ring = null;

            if (taikoJudgement.Result == HitResult.Hit)
            {
                if (h.X >= -0.05f)
                    //if we're far enough away from the left stage, we should bring outselves in front of it.
                    topLevelHitContainer.Add(h.CreateProxy());

                if (taikoJudgement.Score == TaikoScoreResult.Great)
                {
                    hitTarget.Flash(taikoObject.ExplodeColour);
                    ring = new ExplodingRing(taikoObject.ExplodeColour, true);
                }
                else if (taikoJudgement.Score == TaikoScoreResult.Good)
                    ring = new ExplodingRing(taikoObject.ExplodeColour, false);
            }

            if (ring != null)
                explosionRingContainer.Add(ring);

            // Add judgement
            string judgementString = string.Empty;
            if (taikoJudgement.Result == HitResult.Hit)
            {
                switch (taikoJudgement.Score)
                {
                    case TaikoScoreResult.Good:
                        judgementString = "GOOD";
                        break;
                    case TaikoScoreResult.Great:
                        judgementString = "GREAT";
                        break;
                }
            }
            else
                judgementString = "MISS";

            float judgementOffset = taikoJudgement.Result == HitResult.Hit ? h.Position.X : 0;

            // Make drum roll judgements occur at 0 offset
            if (taikoObject is DrawableDrumRoll)
                judgementOffset = 0;

            // Drum roll ticks have no judgement
            if (taikoObject is DrawableDrumRollTick)
                return;

            // Add judgement
            judgementContainer.Add(new JudgementText
            {
                Anchor = taikoJudgement.Result == HitResult.Hit ? Anchor.TopLeft : Anchor.BottomLeft,
                Origin = taikoJudgement.Result == HitResult.Hit ? Anchor.BottomCentre : Anchor.TopCentre,

                RelativePositionAxes = Axes.X,
                Position = new Vector2(judgementOffset, 0),

                Text = judgementString,
                GlowColour = taikoObject.ExplodeColour,
                Direction = taikoJudgement.Result == HitResult.Hit ? -1 : 1
            });
        }
    }
}