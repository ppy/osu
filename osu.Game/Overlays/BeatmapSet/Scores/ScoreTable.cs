// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTable : CompositeDrawable
    {
        private const int fade_duration = 100;
        private const int text_size = 14;

        private readonly FillFlowContainer scoresFlow;
        private readonly ScoresGrid scoresGrid;

        public ScoreTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = scoresGrid = new ScoresGrid
            {
                RelativeSizeAxes = Axes.X,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 40),
                    new Dimension(GridSizeMode.Absolute, 70),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Distributed, minSize: 180),
                    new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                    new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                    new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                    new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                    new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                    new Dimension(GridSizeMode.Distributed, minSize: 40, maxSize: 70),
                    new Dimension(GridSizeMode.AutoSize),
                }
            };

            scoresFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical
            };
        }

        public IEnumerable<APIScoreInfo> Scores
        {
            set
            {
                scoresFlow.Clear();

                if (value == null || !value.Any())
                    return;

                int maxModsAmount = 0;
                foreach (var s in value)
                {
                    var scoreModsAmount = s.Mods.Length;
                    if (scoreModsAmount > maxModsAmount)
                        maxModsAmount = scoreModsAmount;
                }

                scoresFlow.Add(new ScoreTableHeader(maxModsAmount));

                int index = 0;
                foreach (var s in value)
                    scoresFlow.Add(new ScoreTableScore(index++, s, maxModsAmount));

                scoresGrid.Content = value.Select((s, i) => createRowContents(s, i).ToArray()).ToArray();
            }
        }

        private IEnumerable<Drawable> createRowContents(APIScoreInfo score, int index)
        {
            yield return new SpriteText
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Text = $"#{index + 1}",
                Font = @"Exo2.0-Bold",
                TextSize = text_size,
            };

            yield return new DrawableRank(score.Rank)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(30, 20),
                FillMode = FillMode.Fit,
            };

            yield return new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding { Right = 20 },
                Text = $@"{score.TotalScore:N0}",
                TextSize = text_size,
                Font = index == 0 ? OsuFont.GetFont(weight: FontWeight.Bold) : OsuFont.Default
            };

            yield return new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding { Right = 20 },
                Text = $@"{score.Accuracy:P2}",
                TextSize = text_size,
                Colour = score.Accuracy == 1 ? Color4.GreenYellow : Color4.White
            };

            yield return new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new DrawableFlag(score.User.Country)
                    {
                        Size = new Vector2(20, 13),
                    },
                    new ClickableScoreUsername
                    {
                        User = score.User,
                    }
                }
            };

            yield return new SpriteText
            {
                Text = $@"{score.MaxCombo:N0}x",
                TextSize = text_size,
            };

            yield return new SpriteText
            {
                Text = $"{score.Statistics[HitResult.Great]}",
                TextSize = text_size,
                Colour = score.Statistics[HitResult.Great] == 0 ? Color4.Gray : Color4.White
            };

            yield return new SpriteText
            {
                Text = $"{score.Statistics[HitResult.Good]}",
                TextSize = text_size,
                Colour = score.Statistics[HitResult.Good] == 0 ? Color4.Gray : Color4.White
            };

            yield return new SpriteText
            {
                Text = $"{score.Statistics[HitResult.Meh]}",
                TextSize = text_size,
                Colour = score.Statistics[HitResult.Meh] == 0 ? Color4.Gray : Color4.White
            };

            yield return new SpriteText
            {
                Text = $"{score.Statistics[HitResult.Miss]}",
                TextSize = text_size,
                Colour = score.Statistics[HitResult.Miss] == 0 ? Color4.Gray : Color4.White
            };

            yield return new SpriteText
            {
                Text = $@"{score.PP:N0}",
                TextSize = text_size,
            };

            yield return new FillFlowContainer
            {
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                ChildrenEnumerable = score.Mods.Select(m => new ModIcon(m)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Scale = new Vector2(0.3f),
                })
            };
        }

        private class ScoresGrid : GridContainer
        {
            public ScoresGrid()
            {
                AutoSizeAxes = Axes.Y;
            }

            public Drawable[][] Content
            {
                get => base.Content;
                set
                {
                    base.Content = value;

                    RowDimensions = Enumerable.Repeat(new Dimension(GridSizeMode.Absolute, 25), value.Length).ToArray();
                }
            }
        }

        private class ClickableScoreUsername : ClickableUserContainer
        {
            private readonly SpriteText text;
            private readonly SpriteText textBold;

            public ClickableScoreUsername()
            {
                Add(text = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = text_size,
                });

                Add(textBold = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = text_size,
                    Font = @"Exo2.0-Bold",
                    Alpha = 0,
                });
            }

            protected override void OnUserChange(User user)
            {
                text.Text = textBold.Text = user.Username;
            }

            protected override bool OnHover(HoverEvent e)
            {
                textBold.FadeIn(fade_duration, Easing.OutQuint);
                text.FadeOut(fade_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                textBold.FadeOut(fade_duration, Easing.OutQuint);
                text.FadeIn(fade_duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
