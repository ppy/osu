﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking
{
    public abstract class Results : OsuScreen
    {
        private Container circleOuterBackground;
        private Container circleOuter;
        private Container circleInner;

        private ParallaxContainer backgroundParallax;

        private ResultModeTabControl modeChangeButtons;

        public override bool AllowBeatmapRulesetChange => false;

        protected readonly ScoreInfo Score;

        private Container currentPage;

        private static readonly Vector2 background_blur = new Vector2(20);

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        private const float overscan = 1.3f;

        private const float circle_outer_scale = 0.96f;

        protected Results(ScoreInfo score)
        {
            Score = score;
        }

        private const float transition_time = 800;

        private IEnumerable<Drawable> allCircles => new Drawable[] { circleOuterBackground, circleInner, circleOuter };

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            (Background as BackgroundScreenBeatmap)?.BlurTo(background_blur, 2500, Easing.OutQuint);
            Background.ScaleTo(1.1f, transition_time, Easing.OutQuint);

            allCircles.ForEach(c =>
            {
                c.FadeOut();
                c.ScaleTo(0);
            });

            backgroundParallax.FadeOut();
            modeChangeButtons.FadeOut();
            currentPage?.FadeOut();

            circleOuterBackground
                .FadeIn(transition_time, Easing.OutQuint)
                .ScaleTo(1, transition_time, Easing.OutQuint);

            using (BeginDelayedSequence(transition_time * 0.25f, true))
            {
                circleOuter
                    .FadeIn(transition_time, Easing.OutQuint)
                    .ScaleTo(1, transition_time, Easing.OutQuint);

                using (BeginDelayedSequence(transition_time * 0.3f, true))
                {
                    backgroundParallax.FadeIn(transition_time, Easing.OutQuint);

                    circleInner
                        .FadeIn(transition_time, Easing.OutQuint)
                        .ScaleTo(1, transition_time, Easing.OutQuint);

                    using (BeginDelayedSequence(transition_time * 0.4f, true))
                    {
                        modeChangeButtons.FadeIn(transition_time, Easing.OutQuint);
                        currentPage?.FadeIn(transition_time, Easing.OutQuint);
                    }
                }
            }
        }

        public override bool OnExiting(IScreen next)
        {
            allCircles.ForEach(c =>
            {
                c.ScaleTo(0, transition_time, Easing.OutSine);
            });

            Background.ScaleTo(1f, transition_time / 4, Easing.OutQuint);

            this.FadeOut(transition_time / 4);

            return base.OnExiting(next);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new AspectContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Height = overscan,
                    Children = new Drawable[]
                    {
                        circleOuterBackground = new CircularContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Alpha = 0.2f,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                }
                            }
                        },
                        circleOuter = new CircularContainer
                        {
                            Size = new Vector2(circle_outer_scale),
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Colour = Color4.Black.Opacity(0.4f),
                                Type = EdgeEffectType.Shadow,
                                Radius = 15,
                            },
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White,
                                },
                                backgroundParallax = new ParallaxContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ParallaxAmount = 0.01f,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        new Sprite
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Alpha = 0.2f,
                                            Texture = Beatmap.Value.Background,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            FillMode = FillMode.Fill
                                        }
                                    }
                                },
                                modeChangeButtons = new ResultModeTabControl
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 50,
                                    Margin = new MarginPadding { Bottom = 110 },
                                },
                                new OsuSpriteText
                                {
                                    Text = $"{Score.MaxCombo}x",
                                    TextSize = 40,
                                    RelativePositionAxes = Axes.X,
                                    Font = @"Exo2.0-Bold",
                                    X = 0.1f,
                                    Colour = colours.BlueDarker,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.BottomCentre,
                                },
                                new OsuSpriteText
                                {
                                    Text = "max combo",
                                    TextSize = 20,
                                    RelativePositionAxes = Axes.X,
                                    X = 0.1f,
                                    Colour = colours.Gray6,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopCentre,
                                },
                                new OsuSpriteText
                                {
                                    Text = $"{Score.Accuracy:P2}",
                                    TextSize = 40,
                                    RelativePositionAxes = Axes.X,
                                    Font = @"Exo2.0-Bold",
                                    X = 0.9f,
                                    Colour = colours.BlueDarker,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.BottomCentre,
                                },
                                new OsuSpriteText
                                {
                                    Text = "accuracy",
                                    TextSize = 20,
                                    RelativePositionAxes = Axes.X,
                                    X = 0.9f,
                                    Colour = colours.Gray6,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.TopCentre,
                                },
                            }
                        },
                        circleInner = new CircularContainer
                        {
                            Size = new Vector2(0.6f),
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Colour = Color4.Black.Opacity(0.4f),
                                Type = EdgeEffectType.Shadow,
                                Radius = 15,
                            },
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White,
                                },
                            }
                        }
                    }
                },
                new BackButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Action = this.Exit
                },
            };

            foreach (var t in CreateResultPages())
                modeChangeButtons.AddItem(t);
            modeChangeButtons.Current.Value = modeChangeButtons.Items.FirstOrDefault();

            modeChangeButtons.Current.BindValueChanged(m =>
            {
                currentPage?.FadeOut();
                currentPage?.Expire();

                currentPage = m?.CreatePage();

                if (currentPage != null)
                    circleInner.Add(currentPage);
            }, true);
        }

        protected abstract IEnumerable<IResultPageInfo> CreateResultPages();
    }
}
