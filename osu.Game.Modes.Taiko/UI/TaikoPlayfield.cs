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

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoPlayfield : Playfield<TaikoHitObject, TaikoJudgement>
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

        /// <summary>
        /// The offset from <see cref="left_area_size"/> which the center of the hit target lies at.
        /// </summary>
        private const float hit_target_offset = 80;

        /// <summary>
        /// The size of the left area of the playfield. This area contains the input drum.
        /// </summary>
        private const float left_area_size = 240;

        protected override Container<Drawable> Content => hitObjectContainer;

        private readonly Container<HitExplosion> hitExplosionContainer;
        //private Container<DrawableBarLine> barLineContainer;
        private readonly Container<DrawableTaikoJudgement> judgementContainer;

        private readonly Container hitObjectContainer;
        //private Container topLevelHitContainer;
        private readonly Container leftBackgroundContainer;
        private readonly Container rightBackgroundContainer;
        private readonly Box leftBackground;
        private readonly Box rightBackground;

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
                                hitExplosionContainer = new Container<HitExplosion>
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(TaikoHitObject.CIRCLE_RADIUS * 2),
                                    Scale = new Vector2(PLAYFIELD_SCALE),
                                    BlendingMode = BlendingMode.Additive
                                },
                                //barLineContainer = new Container<DrawableBarLine>
                                //{
                                //    RelativeSizeAxes = Axes.Both,
                                //},
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
                    Size = new Vector2(left_area_size, PlayfieldHeight),
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
                //topLevelHitContainer = new Container
                //{
                //    RelativeSizeAxes = Axes.Both,
                //}
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
        }

        public override void OnJudgement(DrawableHitObject<TaikoHitObject, TaikoJudgement> judgedObject)
        {
            bool wasHit = judgedObject.Judgement.Result == HitResult.Hit;

            if (wasHit)
                hitExplosionContainer.Add(new HitExplosion(judgedObject.Judgement));

            judgementContainer.Add(new DrawableTaikoJudgement(judgedObject.Judgement)
            {
                Anchor = wasHit ? Anchor.TopLeft : Anchor.CentreLeft,
                Origin = wasHit ? Anchor.BottomCentre : Anchor.Centre,
                RelativePositionAxes = Axes.X,
                X = wasHit ? judgedObject.Position.X : 0,
            });
        }
    }
}