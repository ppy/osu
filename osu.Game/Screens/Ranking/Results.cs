// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Ranking
{
    public class Results : OsuScreen
    {
        private readonly Score score;
        private Container circleOuterBackground;
        private Container circleOuter;
        private Container circleInner;

        private ParallaxContainer backgroundParallax;

        private ResultModeTabControl modeChangeButtons;

        public override bool AllowBeatmapRulesetChange => false;

        private Container currentPage;

        private static readonly Vector2 background_blur = new Vector2(20);

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        private const float overscan = 1.3f;

        private const float circle_outer_scale = 0.96f;

        public Results(Score score)
        {
            this.score = score;
        }

        private const float transition_time = 800;

        private IEnumerable<Drawable> allCircles => new Drawable[] { circleOuterBackground, circleInner, circleOuter };

        protected override void OnEntering(Screen last)
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
            currentPage.FadeOut();

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
                        currentPage.FadeIn(transition_time, Easing.OutQuint);
                    }
                }
            }
        }

        protected override bool OnExiting(Screen next)
        {
            allCircles.ForEach(c =>
            {
                c.ScaleTo(0, transition_time, Easing.OutSine);
            });

            Background.ScaleTo(1f, transition_time / 4, Easing.OutQuint);

            Content.FadeOut(transition_time / 4);

            return base.OnExiting(next);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
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
                                    Text = $"{score.MaxCombo}x",
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
                                    Text = $"{score.Accuracy:P2}",
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
                    Action = Exit
                },
            };

            modeChangeButtons.AddItem(ResultMode.Summary);
            modeChangeButtons.AddItem(ResultMode.Ranking);
            //modeChangeButtons.AddItem(ResultMode.Share);

            modeChangeButtons.Current.ValueChanged += mode =>
            {
                currentPage?.FadeOut();
                currentPage?.Expire();

                switch (mode)
                {
                    case ResultMode.Summary:
                        currentPage = new ResultsPageScore(score, Beatmap);
                        break;
                    case ResultMode.Ranking:
                        currentPage = new ResultsPageRanking(score, Beatmap);
                        break;
                }

                if (currentPage != null)
                    circleInner.Add(currentPage);
            };

            modeChangeButtons.Current.TriggerChange();
        }
    }
}
