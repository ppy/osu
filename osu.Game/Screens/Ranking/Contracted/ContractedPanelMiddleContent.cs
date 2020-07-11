// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Contracted
{
    /// <summary>
    /// The content that appears in the middle of a contracted <see cref="ScorePanel"/>.
    /// </summary>
    public class ContractedPanelMiddleContent : CompositeDrawable
    {
        private readonly ScoreInfo score;

        /// <summary>
        /// Creates a new <see cref="ContractedPanelMiddleContent"/>.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to display.</param>
        public ContractedPanelMiddleContent(ScoreInfo score)
        {
            this.score = score;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerExponent = 2.5f,
                            CornerRadius = 20,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Colour = Color4.Black.Opacity(0.25f),
                                Type = EdgeEffectType.Shadow,
                                Radius = 1,
                                Offset = new Vector2(0, 4)
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("444")
                                },
                                new UserCoverBackground
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    User = score.User,
                                    Colour = ColourInfo.GradientVertical(Color4.White.Opacity(0.5f), Color4Extensions.FromHex("#444").Opacity(0))
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(10),
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 10),
                                    Children = new Drawable[]
                                    {
                                        new UpdateableAvatar(score.User)
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Size = new Vector2(110),
                                            Masking = true,
                                            CornerExponent = 2.5f,
                                            CornerRadius = 20,
                                            EdgeEffect = new EdgeEffectParameters
                                            {
                                                Colour = Color4.Black.Opacity(0.25f),
                                                Type = EdgeEffectType.Shadow,
                                                Radius = 8,
                                                Offset = new Vector2(0, 4),
                                            }
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Text = score.UserString,
                                            Font = OsuFont.GetFont(size: 16, weight: FontWeight.SemiBold)
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 5),
                                            ChildrenEnumerable = score.SortedStatistics.Select(s => createStatistic(s.Key.GetDescription(), s.Value.ToString()))
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Margin = new MarginPadding { Top = 10 },
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 5),
                                            Children = new[]
                                            {
                                                createStatistic("Max Combo", $"x{score.MaxCombo}"),
                                                createStatistic("Accuracy", $"{score.Accuracy.FormatAccuracy()}"),
                                            }
                                        },
                                        new ModDisplay
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            AutoSizeAxes = Axes.Both,
                                            ExpansionMode = ExpansionMode.AlwaysExpanded,
                                            DisplayUnrankedText = false,
                                            Current = { Value = score.Mods },
                                            Scale = new Vector2(0.5f),
                                        }
                                    }
                                }
                            }
                        },
                    },
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 5 },
                            Child = new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = score.TotalScore.ToString("N0"),
                                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Medium, fixedWidth: true),
                                            Spacing = new Vector2(-1, 0)
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Top = 2 },
                                            Child = new DrawableRank(score.Rank)
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                            }
                                        }
                                    },
                                },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            }
                        }
                    },
                },
                RowDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 45),
                }
            };
        }

        private Drawable createStatistic(string key, string value) => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = key,
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold)
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Text = value,
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                    Colour = Color4Extensions.FromHex("#FFDD55")
                }
            }
        };
    }
}
