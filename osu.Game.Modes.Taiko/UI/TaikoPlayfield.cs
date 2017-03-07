// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoPlayfield : Playfield<TaikoHitObject>
    {
        public const float PLAYFIELD_BASE_HEIGHT = 242;
        public const float PLAYFIELD_SCALE = 0.65f;

        public static float PlayfieldHeight => PLAYFIELD_BASE_HEIGHT * PLAYFIELD_SCALE;

        private static float left_area_size = 0.15f / PLAYFIELD_SCALE;
        private const float hit_target_offset = 0.1f;

        private HitTarget hitTarget;
        private Container<ExplodingRing> explosionRingContainer;
        private Container<DrawableBarLine> barLineContainer;
        private Container<JudgementText> judgementContainer;

        private Container leftBackgroundContainer;
        private Box leftBackground;
        private Container rightBackgroundContainer;
        private Box rightBackground;

        private Color4 missColour;

        public TaikoPlayfield()
        {
            RelativeSizeAxes = Axes.Both;

            // Right area under notes
            AddInternal(leftBackgroundContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                RelativePositionAxes = Axes.X,
                Position = new Vector2(left_area_size, 0),
                Size = new Vector2(1f - left_area_size, PlayfieldHeight),

                BorderThickness = 2,

                Depth = 1,

                Children = new Drawable[]
                {
                    // Background
                    leftBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = 0.5f
                    },
                    // Hit target
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Both,

                        Position = new Vector2(hit_target_offset, 0),

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
                            hitTarget = new HitTarget
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,

                                Scale = new Vector2(PLAYFIELD_SCALE)
                            }
                        }
                    },
                }
            });

            // Notes
            HitObjects.Anchor = Anchor.TopLeft;
            HitObjects.Origin = Anchor.TopLeft;

            HitObjects.RelativePositionAxes = Axes.X;
            HitObjects.RelativeSizeAxes = Axes.X;
            HitObjects.Position = new Vector2(left_area_size + hit_target_offset * (1f - left_area_size), 0);
            HitObjects.Size = new Vector2(1f - left_area_size - hit_target_offset * (1f - left_area_size), PlayfieldHeight);

            // Bar lines
            AddInternal(barLineContainer = new Container<DrawableBarLine>
            {
                RelativePositionAxes = HitObjects.RelativePositionAxes,
                RelativeSizeAxes = HitObjects.RelativeSizeAxes,
                Position = HitObjects.Position,
                Size = HitObjects.Size
            });

            // Judgements
            AddInternal(judgementContainer = new Container<JudgementText>
            {
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = HitObjects.RelativeSizeAxes,
                Position = new Vector2(left_area_size + hit_target_offset * (1f - left_area_size), 0),
                Size = HitObjects.Size,

                BlendingMode = BlendingMode.Additive
            });

            // Left area above notes
            AddInternal(rightBackgroundContainer = new Container
            {
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(left_area_size, PlayfieldHeight),

                Masking = true,

                BorderThickness = 1,

                Children = new Drawable[]
                {
                    // Background
                    rightBackground = new Box
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
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            leftBackgroundContainer.BorderColour = colours.Gray1;
            leftBackground.Colour = colours.Gray0;

            rightBackgroundContainer.BorderColour = colours.Gray0;
            rightBackground.Colour = colours.Gray1;

            missColour = colours.Red;
        }

        public override void Add(DrawableHitObject<TaikoHitObject> h)
        {
            h.Depth = (float)h.HitObject.StartTime;
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
            TaikoJudgementInfo tji = j as TaikoJudgementInfo;
            DrawableTaikoHitObject dth = h as DrawableTaikoHitObject;

            if (dth == null)
                return;

            // Add ring
            ExplodingRing ring = null;

            if (tji.Result == HitResult.Hit)
            {
                if (tji.Score == TaikoScoreResult.Great)
                {
                    hitTarget.Flash(dth.ExplodeColour);
                    ring = new ExplodingRing(dth.ExplodeColour, true);
                }
                else if (tji.Score == TaikoScoreResult.Good)
                    ring = new ExplodingRing(dth.ExplodeColour, false);
            }

            if (ring != null)
                explosionRingContainer.Add(ring);

            // Add judgement
            string judgementString = "";
            if (tji.Result == HitResult.Hit)
            {
                switch (tji.Score)
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

            float judgementOffset = tji.Result == HitResult.Hit ? h.Position.X : 0;

            // Make drum roll judgements occur at 0 offset
            if (dth is DrawableDrumRoll)
                judgementOffset = 0;

            // Drum roll ticks have no judgement
            if (dth is DrawableDrumRollTick)
                return;

            // Add judgement
            judgementContainer.Add(new JudgementText
            {
                Anchor = tji.Result == HitResult.Hit ? Anchor.TopLeft : Anchor.BottomLeft,
                Origin = tji.Result == HitResult.Hit ? Anchor.BottomCentre : Anchor.TopCentre,

                RelativePositionAxes = Axes.X,
                Position = new Vector2(judgementOffset, 0),

                Text = judgementString,
                GlowColour = dth.ExplodeColour,
                Direction = tji.Result == HitResult.Hit ? -1 : 1
            });
        }
    }
}