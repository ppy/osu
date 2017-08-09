// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Scoring;
using System;
using System.Linq;

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
                            Alpha = 0,
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
                            Alpha = 0,
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
                        showMoreText = new OsuHoverContainer
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
                showMoreText.Show();
            }

            public void ShowMore() => showMoreText.Alpha = Children.Where(d => !d.IsPresent).Where((d, i) => (d.Alpha = i < 5 ? 1 : 0) == 0).Any() ? 1 : 0;
        }
    }
}
