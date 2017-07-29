// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Scoring;
using System;

namespace osu.Game.Overlays.Profile.Sections
{
    public class RanksSection : ProfileSection
    {
        public override string Title => "Ranks";

        public override string Identifier => "top_ranks";

        private readonly ScoreFlowContainer best, first;
        private readonly OsuSpriteText bestMissing, firstMissing;

        public RanksSection()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    TextSize = 15,
                    Text = "Best Performance",
                    Font = "Exo2.0-RegularItalic",
                    Margin = new MarginPadding { Top = 10, Bottom = 10 },
                },
                best = new ScoreFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                },
                bestMissing = new OsuSpriteText
                {
                    TextSize = 14,
                    Text = "No awesome performance records yet. :(",
                },
                new OsuSpriteText
                {
                    TextSize = 15,
                    Text = "First Place Ranks",
                    Font = "Exo2.0-RegularItalic",
                    Margin = new MarginPadding { Top = 20, Bottom = 10 },
                },
                first = new ScoreFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                },
                firstMissing = new OsuSpriteText
                {
                    TextSize = 14,
                    Text = "No awesome performance records yet. :(",
                },
            };
        }

        public Score[] ScoresBest
        {
            set
            {
                best.Clear();
                if (value.Length == 0)
                {
                    bestMissing.Show();
                }
                else
                {
                    bestMissing.Hide();
                    int i = 0;
                    foreach (Score score in value)
                    {
                        best.Add(new DrawableScore(score, Math.Pow(0.95, i))
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 60,
                        });
                        i++;
                    }
                }
                best.ShowMore();
            }
        }

        public Score[] ScoresFirst
        {
            set
            {
                first.Clear();
                if (value.Length == 0)
                {
                    firstMissing.Show();
                }
                else
                {
                    firstMissing.Hide();
                    foreach (Score score in value)
                        first.Add(new DrawableScore(score)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 60,
                        });
                }
                first.ShowMore();
            }
        }

        private class ScoreFlowContainer : Container<DrawableScore>
        {
            private readonly FillFlowContainer<DrawableScore> scores;
            private readonly OsuClickableContainer showMoreText;

            protected override Container<DrawableScore> Content => scores;

            private int shownScores;

            public ScoreFlowContainer()
            {
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        scores = new FillFlowContainer<DrawableScore>
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Vertical,
                        },
                        showMoreText = new ShowMoreContainer
                        {
                            Action = ShowMore,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Alpha = 0,
                            Child = new OsuSpriteText
                            {
                                TextSize = 14,
                                Text = "show more",
                            }
                        }
                    },
                };
            }

            public override void Clear(bool disposeChildren)
            {
                base.Clear(disposeChildren);
                shownScores = 0;
                showMoreText.Show();
            }

            public void ShowMore()
            {
                shownScores = Math.Min(Children.Count, shownScores + 5);
                int i = 0;
                foreach(DrawableScore score in Children)
                    score.FadeTo(i++ < shownScores ? 1 : 0);
                showMoreText.FadeTo(shownScores == Children.Count ? 0 : 1);
            }

            private class ShowMoreContainer : OsuClickableContainer
            {
                private Color4 hoverColour;

                protected override bool OnHover(InputState state)
                {
                    this.FadeColour(hoverColour, 500, Easing.OutQuint);
                    return base.OnHover(state);
                }

                protected override void OnHoverLost(InputState state)
                {
                    this.FadeColour(Color4.White, 500, Easing.OutQuint);
                    base.OnHoverLost(state);
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    hoverColour = colours.Yellow;
                }
            }
        }
    }
}
