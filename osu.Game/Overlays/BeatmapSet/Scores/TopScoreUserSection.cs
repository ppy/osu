// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class TopScoreUserSection : CompositeDrawable
    {
        private readonly ScoreInfo score;
        private readonly int? scorePosition;

        private LinkFlowContainer usernameText;

        public TopScoreUserSection(ScoreInfo score, int? scorePosition)
        {
            this.score = score;
            this.scorePosition = scorePosition;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                                Text = scorePosition.HasValue ? $"#{scorePosition.Value}" : "-"
                            },
                            new UpdateableRank(ScoreRank.D)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(28),
                                FillMode = FillMode.Fit,
                                Rank = score.Rank
                            },
                        }
                    },
                    new UpdateableAvatar
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(70),
                        Masking = true,
                        CornerRadius = 4,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Black.Opacity(0.25f),
                            Offset = new Vector2(0, 2),
                            Radius = 1,
                        },
                        ShowGuestOnNull = false,
                        User = score.User
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 3),
                        Children = new Drawable[]
                        {
                            usernameText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold, italics: true))
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new[]
                                {
                                    new OsuSpriteText
                                    {
                                        Text = "achieved ",
                                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold)
                                    },
                                    new DrawableDate(score.Date, colourProvider: colourProvider)
                                    {
                                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold)
                                    },
                                }
                            },
                            new UpdateableFlag
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(19, 13),
                                Margin = new MarginPadding { Top = 3 }, // makes spacing look more even
                                ShowPlaceholderOnNull = false,
                                Country = score.User.Country
                            },
                        }
                    }
                }
            };

            usernameText.AddUserLink(score.User);
        }
    }
}
