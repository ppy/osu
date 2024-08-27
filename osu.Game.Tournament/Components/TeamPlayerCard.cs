// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Components
{
    public partial class TeamPlayerCard : UserPanel
    {
        private readonly APIUser? teamPlayer;
        private FillFlowContainer details = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public TeamPlayerCard(APIUser user)
            : base(user)
        {
            RelativeSizeAxes = Axes.X;
            Height = 40;
            CornerRadius = 6;
            teamPlayer = user;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, LadderInfo ladderinfo)
        {
            AltBackground.Texture = textures.Get("Icons/usercard-default");
            AltBackground.Colour = ColourInfo.GradientHorizontal(Color4Extensions.FromHex("#43C7DE").Opacity(0.5f), Color4.White.Opacity(0.5f));
            AltBackground.FillMode = FillMode.Fill;
            AltBackground.Origin = Anchor.CentreRight;
            AltBackground.Anchor = Anchor.CentreRight;
            Background.Origin = Anchor.CentreRight;
            Background.Anchor = Anchor.CentreRight;
            Background.Colour = Color4.Gray;

            var request = new GetUserRequest(userId: User.Id, ruleset: ladderinfo.Ruleset.Value);

            request.Success += user =>
            {
                Scheduler.Add(() =>
                {
                    details.Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Text = $"{user.Statistics.PP}pp",
                            Font = OsuFont.TorusAlternate.With(weight: FontWeight.SemiBold, size: 20),
                            Shadow = true
                        },
                        new TournamentSpriteText
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Text = $"#{user.Statistics.GlobalRank}",
                            Font = OsuFont.TorusAlternate.With(weight: FontWeight.Medium, size: 17),
                            Shadow = true
                        }
                    };
                    details.FadeInFromZero(duration: 200, easing: Easing.OutCubic);
                });
            };

            api.Queue(request);
        }

        protected override Drawable CreateLayout()
        {
            var layout = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(40),
                                Masking = true,
                                CornerRadius = 6,
                                Margin = new MarginPadding { Left = 10 },
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(25),
                                        Icon = FontAwesome.Solid.UserAlt,
                                    },
                                    new UpdateableAvatar(user: teamPlayer)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(40),
                                    },
                                }
                            },
                            CreateUsername().With(username =>
                            {
                                username.Anchor = Anchor.CentreLeft;
                                username.Origin = Anchor.CentreLeft;
                                username.UseFullGlyphHeight = false;
                                username.Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24);
                            })
                        }
                    },
                    details = new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(10, 0),
                        Margin = new MarginPadding { Right = 10 },
                    }
                }
            };

            return layout;
        }
    }
}
