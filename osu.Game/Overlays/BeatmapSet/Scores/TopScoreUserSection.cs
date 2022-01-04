// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
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
        private readonly SpriteText rankText;
        private readonly UpdateableRank rank;
        private readonly UpdateableAvatar avatar;
        private readonly LinkFlowContainer usernameText;
        private readonly DrawableDate achievedOn;
        private readonly UpdateableFlag flag;

        public TopScoreUserSection()
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
                            rankText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold)
                            },
                            rank = new UpdateableRank(ScoreRank.D)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(28),
                                FillMode = FillMode.Fit,
                            },
                        }
                    },
                    avatar = new UpdateableAvatar(showGuestOnNull: false)
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
                                    achievedOn = new DrawableDate(DateTimeOffset.MinValue)
                                    {
                                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold)
                                    },
                                }
                            },
                            flag = new UpdateableFlag
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(19, 13),
                                Margin = new MarginPadding { Top = 3 }, // makes spacing look more even
                                ShowPlaceholderOnNull = false,
                            },
                        }
                    }
                }
            };
        }

        public int? ScorePosition
        {
            set => rankText.Text = value?.ToLocalisableString(@"\##") ?? (LocalisableString)"-";
        }

        /// <summary>
        /// Sets the score to be displayed.
        /// </summary>
        public ScoreInfo Score
        {
            set
            {
                avatar.User = value.User;
                flag.Country = value.User.Country;
                achievedOn.Date = value.Date;

                usernameText.Clear();
                usernameText.AddUserLink(value.User);

                rank.Rank = value.Rank;
            }
        }
    }
}
