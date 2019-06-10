// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class TopScoreUserSection : CompositeDrawable
    {
        private readonly SpriteText rankText;
        private readonly DrawableRank rank;
        private readonly UpdateableAvatar avatar;
        private readonly LinkFlowContainer usernameText;
        private readonly SpriteText date;
        private readonly DrawableFlag flag;

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
                    rankText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "#1",
                        Font = OsuFont.GetFont(size: 30, weight: FontWeight.Bold, italics: true)
                    },
                    rank = new DrawableRank(ScoreRank.D)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(40),
                        FillMode = FillMode.Fit,
                    },
                    avatar = new UpdateableAvatar
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(80),
                        Masking = true,
                        CornerRadius = 5,
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
                            usernameText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold, italics: true))
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.Both,
                            },
                            date = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold)
                            },
                            flag = new DrawableFlag
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(20, 13),
                            },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            rankText.Colour = colours.Yellow;
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
                date.Text = $@"achieved {value.Date.Humanize()}";

                usernameText.Clear();
                usernameText.AddUserLink(value.User);

                rank.UpdateRank(value.Rank);
            }
        }
    }
}
