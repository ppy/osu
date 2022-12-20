using System;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Numbers.Components;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Numbers
{
    public partial class NumbersScreen : SandboxScreen
    {
        private readonly BindableInt bestScore = new BindableInt();

        private readonly NumbersPlayfield playfield;
        private readonly Container scoresContainer;

        public NumbersScreen()
        {
            ScoreContainer currentScore;

            AddRangeInternal(new Drawable[]
            {
                new OsuClickableContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 6,
                    Masking = true,
                    Margin = new MarginPadding { Top = 240 },
                    Action = () => playfield?.Restart(),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(187, 173, 160, 255)
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = @"Restart".ToUpperInvariant(),
                            Font = OsuFont.GetFont(size: 25, weight: FontWeight.Bold),
                            Colour = new Color4(119, 110, 101, 255),
                            Shadow = false,
                            Margin = new MarginPadding { Horizontal = 10, Vertical = 10 },
                        }
                    }
                },
                scoresContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Bottom = 240 },
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Relative, size: 0.5f),
                            new Dimension()
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Right = 10 },
                                    Child = currentScore = new ScoreContainer("Current Score"),
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Left = 10 },
                                    Child = new ScoreContainer("Best Score")
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Score = { BindTarget = bestScore }
                                    }
                                }
                            }
                        }
                    }
                },
                playfield = new NumbersPlayfield
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });

            currentScore.Score.BindTo(playfield.Score);
        }

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager config)
        {
            config?.BindWith(SandboxRulesetSetting.NumbersGameBestScore, bestScore);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            playfield.Score.BindValueChanged(score => bestScore.Value = Math.Max(score.NewValue, bestScore.Value), true);
        }

        protected override void Update()
        {
            base.Update();
            scoresContainer.Width = playfield.DrawWidth;
        }

        private partial class ScoreContainer : CompositeDrawable
        {
            public readonly BindableInt Score = new BindableInt();

            private readonly OsuSpriteText spriteText;

            public ScoreContainer(string header)
            {
                RelativeSizeAxes = Axes.X;
                Height = 60;
                CornerRadius = 6;
                Masking = true;
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(187, 173, 160, 255)
                    },
                    spriteText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Y = 5,
                        Font = OsuFont.GetFont(size: 40, weight: FontWeight.Bold),
                        Colour = new Color4(119, 110, 101, 255),
                        Shadow = false
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 5 },
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold),
                        Colour = new Color4(119, 110, 101, 255),
                        Shadow = false,
                        Text = header
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Score.BindValueChanged(s => spriteText.Text = s.NewValue.ToString(), true);
            }
        }
    }
}
