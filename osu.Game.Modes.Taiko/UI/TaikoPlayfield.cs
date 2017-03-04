// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Modes.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Framework.MathUtils;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoPlayfield : Playfield
    {
        public const float PLAYFIELD_BASE_HEIGHT = 242;
        public const float PLAYFIELD_SCALE = 0.65f;

        public static float PLAYFIELD_HEIGHT => PLAYFIELD_BASE_HEIGHT * PLAYFIELD_SCALE;

        protected override Container<Drawable> Content => this;

        private static float left_area_size = 0.15f / PLAYFIELD_SCALE;
        private const float hit_target_offset = 0.1f;

        private HitTarget hitTarget;
        private Container explosionRingContainer;
        private Container judgementContainer;

        public TaikoPlayfield()
        {
            RelativeSizeAxes = Axes.Both;

            // Right area under notes
            AddInternal(new Container()
            {
                RelativeSizeAxes = Axes.X,
                RelativePositionAxes = Axes.X,
                Position = new Vector2(left_area_size, 0),
                Size = new Vector2(1f - left_area_size, PLAYFIELD_HEIGHT),

                BorderColour = new Color4(17, 17, 17, 255),
                BorderThickness = 2,

                Depth = 1,

                Children = new Drawable[]
                {
                    // Background
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(0, 0, 0, 127)
                    },
                    // Hit target
                    new Container()
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Both,

                        Position = new Vector2(hit_target_offset, 0),

                        Children = new Drawable[]
                        {
                            explosionRingContainer = new Container()
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,
                                
                                Size = new Vector2(128),
                                Scale = new Vector2(PLAYFIELD_SCALE),

                                BlendingMode = BlendingMode.Additive
                            },
                            hitTarget = new HitTarget()
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre
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
            HitObjects.Size = new Vector2(1f - left_area_size - hit_target_offset * (1f - left_area_size), PLAYFIELD_HEIGHT);

            AddInternal(judgementContainer = new Container()
            {
                Origin = Anchor.BottomCentre,

                RelativePositionAxes = Axes.Both,
                Position = new Vector2(left_area_size + hit_target_offset * (1f - left_area_size), 0),

                BlendingMode = BlendingMode.Additive
            });

            // Bar lines
            AddInternal(new Container()
            {
                RelativePositionAxes = HitObjects.RelativePositionAxes,
                RelativeSizeAxes = HitObjects.RelativeSizeAxes,
                Position = HitObjects.Position,
                Size = HitObjects.Size
            });

            // Left area above notes
            AddInternal(new Container()
            {
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(left_area_size, PLAYFIELD_HEIGHT),

                Masking = true,

                BorderColour = Color4.Black,
                BorderThickness = 1,

                Children = new Drawable[]
                {
                    // Background
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(17, 17, 17, 255)
                    },
                    new Container()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        RelativePositionAxes = Axes.X,
                        Position = new Vector2(0.10f, 0),

                        Children = new Drawable[]
                        {
                            new InputDrum()
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

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;
            h.Scale = new Vector2(PLAYFIELD_SCALE);

            h.OnJudgement += onJudgement;

            base.Add(h);
        }

        private void onJudgement(DrawableHitObject h, JudgementInfo j)
        {
            TaikoJudgementInfo tji = j as TaikoJudgementInfo;
            DrawableTaikoHitObject dth = h as DrawableTaikoHitObject;

            // Add ring
            ExplodingRing ring = null;

            if (tji.Score == TaikoScoreResult.Great)
            {
                hitTarget.Flash(dth.ExplodeColour);
                ring = new ExplodingRing(dth.ExplodeColour, true);
            }
            else if (tji.Score == TaikoScoreResult.Good)
                ring = new ExplodingRing(dth.ExplodeColour, false);
            else
                hitTarget.Flash(Color4.Red);

            if (ring != null)
                explosionRingContainer.Add(ring);

            // Add judgement
            string judgementString = "";
            switch (tji.Score)
            {
                case TaikoScoreResult.Miss:
                    judgementString = "MISS";
                    break;
                case TaikoScoreResult.Good:
                    judgementString = "GOOD";
                    break;
                case TaikoScoreResult.Great:
                    judgementString = "GREAT";
                    break;
            }

            judgementContainer.Add(new JudgementText()
            {
                Text = judgementString,
                GlowColour = dth.ExplodeColour
            });
        }
    }
}