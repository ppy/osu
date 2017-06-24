// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Extensions.Color4Extensions;
using System.Linq;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using System;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class TaikoPlayfield : Playfield<TaikoHitObject, TaikoJudgement>
    {
        /// <summary>
        /// The default play field height.
        /// </summary>
        public const float DEFAULT_PLAYFIELD_HEIGHT = 178f;

        /// <summary>
        /// The offset from <see cref="left_area_size"/> which the center of the hit target lies at.
        /// </summary>
        public const float HIT_TARGET_OFFSET = TaikoHitObject.DEFAULT_STRONG_CIRCLE_DIAMETER / 2f + 40;

        /// <summary>
        /// The size of the left area of the playfield. This area contains the input drum.
        /// </summary>
        private const float left_area_size = 240;

        protected override Container<Drawable> Content => hitObjectContainer;

        private readonly Container<HitExplosion> hitExplosionContainer;
        private readonly Container<KiaiHitExplosion> kiaiExplosionContainer;
        private readonly Container<DrawableBarLine> barLineContainer;
        private readonly Container<DrawableTaikoJudgement> judgementContainer;

        private readonly Container hitObjectContainer;
        private readonly Container topLevelHitContainer;

        private readonly Container overlayBackgroundContainer;
        private readonly Container backgroundContainer;

        private readonly Box overlayBackground;
        private readonly Box background;

        public TaikoPlayfield()
        {
            AddInternal(new Drawable[]
            {
                new ScaleFixContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = DEFAULT_PLAYFIELD_HEIGHT,
                    Children = new[]
                    {
                        backgroundContainer = new Container
                        {
                            Name = "Transparent playfield background",
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 2,
                            Masking = true,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Radius = 5,
                            },
                            Children = new Drawable[]
                            {
                                background = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.6f
                                },
                            }
                        },
                        new Container
                        {
                            Name = "Right area",
                            RelativeSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Left = left_area_size },
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Name = "Masked elements",
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Left = HIT_TARGET_OFFSET },
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        hitExplosionContainer = new Container<HitExplosion>
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            BlendingMode = BlendingMode.Additive,
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
                                    }
                                },
                                kiaiExplosionContainer = new Container<KiaiHitExplosion>
                                {
                                    Name = "Kiai hit explosions",
                                    RelativeSizeAxes = Axes.Y,
                                    Margin = new MarginPadding { Left = HIT_TARGET_OFFSET },
                                    BlendingMode = BlendingMode.Additive
                                },
                                judgementContainer = new Container<DrawableTaikoJudgement>
                                {
                                    Name = "Judgements",
                                    RelativeSizeAxes = Axes.Y,
                                    Margin = new MarginPadding { Left = HIT_TARGET_OFFSET },
                                    BlendingMode = BlendingMode.Additive
                                },
                            }
                        },
                        overlayBackgroundContainer = new Container
                        {
                            Name = "Left overlay",
                            Size = new Vector2(left_area_size, DEFAULT_PLAYFIELD_HEIGHT),
                            BorderThickness = 1,
                            Children = new Drawable[]
                            {
                                overlayBackground = new Box
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
                    }
                },
                topLevelHitContainer = new Container
                {
                    Name = "Top level hit objects",
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            overlayBackgroundContainer.BorderColour = colours.Gray0;
            overlayBackground.Colour = colours.Gray1;

            backgroundContainer.BorderColour = colours.Gray1;
            background.Colour = colours.Gray0;
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

            bool isRim = judgedObject.HitObject is RimHit;

            if (!secondHit)
            {
                if (judgedObject.X >= -0.05f && !(judgedObject is DrawableSwell))
                {
                    // If we're far enough away from the left stage, we should bring outselves in front of it
                    topLevelHitContainer.Add(judgedObject.CreateProxy());
                }

                hitExplosionContainer.Add(new HitExplosion(judgedObject.Judgement, isRim));

                if (judgedObject.HitObject.Kiai)
                    kiaiExplosionContainer.Add(new KiaiHitExplosion(judgedObject.Judgement, isRim));
            }
            else
                hitExplosionContainer.Children.FirstOrDefault(e => e.Judgement == judgedObject.Judgement)?.VisualiseSecondHit();
        }

        /// <summary>
        /// This is a very special type of container. It serves a similar purpose to <see cref="FillMode.Fit"/>, however unlike <see cref="FillMode.Fit"/>,
        /// this will only adjust the scale relative to the height of its parent and will maintain the original width relative to its parent.
        ///
        /// <para>
        /// By adjusting the scale relative to the height of its parent, the aspect ratio of this container's children is maintained, however this is undesirable
        /// in the case where the hit object container should not have its width adjusted by scale. To counteract this, another container is nested inside this
        /// container which takes care of reversing the width adjustment while appearing transparent to the user.
        /// </para>
        /// </summary>
        private class ScaleFixContainer : Container
        {
            protected override Container<Drawable> Content => widthAdjustmentContainer;
            private readonly WidthAdjustmentContainer widthAdjustmentContainer;

            /// <summary>
            /// We only want to apply DrawScale in the Y-axis to preserve aspect ratio and <see cref="TaikoPlayfield"/> doesn't care about having its width adjusted.
            /// </summary>
            protected override Vector2 DrawScale => Scale * RelativeToAbsoluteFactor.Y / DrawHeight;

            public ScaleFixContainer()
            {
                AddInternal(widthAdjustmentContainer = new WidthAdjustmentContainer { ParentDrawScaleReference = () => DrawScale.X });
            }

            /// <summary>
            /// The container type that reverses the <see cref="Drawable.DrawScale"/> width adjustment.
            /// </summary>
            private class WidthAdjustmentContainer : Container
            {
                /// <summary>
                /// This container needs to know its parent's <see cref="Drawable.DrawScale"/> so it can reverse the width adjustment caused by <see cref="Drawable.DrawScale"/>.
                /// </summary>
                public Func<float> ParentDrawScaleReference;

                public WidthAdjustmentContainer()
                {
                    // This container doesn't care about height, it should always fill its parent
                    RelativeSizeAxes = Axes.Y;
                }

                protected override void Update()
                {
                    base.Update();

                    // Reverse the DrawScale adjustment
                    Width = Parent.DrawSize.X / ParentDrawScaleReference();
                }
            }
        }
    }
}