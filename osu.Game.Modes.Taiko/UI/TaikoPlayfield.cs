// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Primitives;
using System.Linq;
using osu.Game.Modes.Taiko.Objects.Drawables;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoPlayfield : Playfield<TaikoHitObject, TaikoJudgement>
    {
        /// <summary>
        /// The play field height. This is relative to the size of hit objects
        /// such that the playfield is just a bit larger than strong hits.
        /// </summary>
        public const float PLAYFIELD_HEIGHT = TaikoHitObject.CIRCLE_RADIUS * 2 * 2;

        /// <summary>
        /// The offset from <see cref="left_area_size"/> which the center of the hit target lies at.
        /// </summary>
        private const float hit_target_offset = TaikoHitObject.CIRCLE_RADIUS * 1.5f + 40;

        /// <summary>
        /// The size of the left area of the playfield. This area contains the input drum.
        /// </summary>
        private const float left_area_size = 240;

        protected override Container<Drawable> Content => hitObjectContainer;

        private readonly Container<HitExplosion> hitExplosionContainer;
        private readonly Container<DrawableBarLine> barLineContainer;
        private readonly Container<DrawableTaikoJudgement> judgementContainer;

        private readonly Container hitObjectContainer;
        private readonly Container topLevelHitContainer;
        private readonly Container leftBackgroundContainer;
        private readonly Container rightBackgroundContainer;
        private readonly Box leftBackground;
        private readonly Box rightBackground;

        public TaikoPlayfield()
        {
            RelativeSizeAxes = Axes.X;
            Height = PLAYFIELD_HEIGHT;

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
                            X = hit_target_offset,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                hitExplosionContainer = new Container<HitExplosion>
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2),
                                    BlendingMode = BlendingMode.Additive
                                },
                                barLineContainer = new Container<DrawableBarLine>
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new HitTarget
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.Centre,
                                },
                                hitObjectContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                judgementContainer = new Container<DrawableTaikoJudgement>
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
                    Size = new Vector2(left_area_size, PLAYFIELD_HEIGHT),
                    BorderThickness = 1,
                    Children = new Drawable[]
                    {
                        leftBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new InputDrum
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.X,
                            Position = new Vector2(0.10f, 0),
                            Scale = new Vector2(0.9f)
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

        public override void Add(DrawableHitObject<TaikoHitObject, TaikoJudgement> h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            base.Add(h);

            // Swells should be moved at the very top of the playfield when they reach the hit target
            var swell = h as DrawableSwell;
            if (swell != null)
                swell.OnStart += () => topLevelHitContainer.Add(swell.CreateProxy());
        }

        public void AddBarLine(DrawableBarLine barLine)
        {
            barLineContainer.Add(barLine);
        }

        public override void OnJudgement(DrawableHitObject<TaikoHitObject, TaikoJudgement> judgedObject)
        {
            bool wasHit = judgedObject.Judgement.Result == HitResult.Hit;
            bool secondHit = judgedObject.Judgement.SecondHit;

            judgementContainer.Add(new DrawableTaikoJudgement(judgedObject.Judgement)
            {
                Anchor = wasHit ? Anchor.TopLeft : Anchor.CentreLeft,
                Origin = wasHit ? Anchor.BottomCentre : Anchor.Centre,
                RelativePositionAxes = Axes.X,
                X = wasHit ? judgedObject.Position.X : 0,
            });

            if (!wasHit)
                return;

            if (!secondHit)
            {
                if (judgedObject.X >= -0.05f && !(judgedObject is DrawableSwell))
                {
                    // If we're far enough away from the left stage, we should bring outselves in front of it
                    topLevelHitContainer.Add(judgedObject.CreateProxy());
                }

                hitExplosionContainer.Add(new HitExplosion(judgedObject.Judgement));
            }
            else
                hitExplosionContainer.Children.FirstOrDefault(e => e.Judgement == judgedObject.Judgement)?.VisualiseSecondHit();
        }
    }
}