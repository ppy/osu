// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class TopScoreUserSection : CompositeDrawable
    {
        public TopScoreUserSection(SoloScoreInfo score, int? position)
        {
            AutoSizeAxes = Axes.Both;

            Debug.Assert(score.User != null);

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
                                Text = position?.ToLocalisableString(@"\##") ?? (LocalisableString)"-",
                            },
                            new UpdateableRank(ScoreRank.D)
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(28),
                                FillMode = FillMode.Fit,
                                Rank = score.Rank,
                            },
                        }
                    },
                    new UpdateableAvatar(showGuestOnNull: false)
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
                        User = score.User,
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
                            new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold, italics: true))
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                            }.With(u => u.AddUserLink(score.User)),
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
                                    new DrawableDate(score.EndedAt)
                                    {
                                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold)
                                    },
                                }
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(4),
                                Children = new Drawable[]
                                {
                                    new UpdateableFlag(score.User.CountryCode)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(19, 14),
                                        Margin = new MarginPadding { Top = 3 }, // makes spacing look more even
                                    },
                                    new UpdateableTeamFlag(score.User.Team)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(28, 14),
                                        Margin = new MarginPadding { Top = 3 }, // makes spacing look more even
                                    },
                                }
                            },
                        }
                    }
                }
            };
        }
    }
}
