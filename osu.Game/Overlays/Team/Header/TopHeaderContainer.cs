// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Team.Header
{
    public partial class TopHeaderContainer : CompositeDrawable
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private ProfileCoverBackground cover = null!;
        private UpdateableTeamFlag teamFlag = null!;
        private OsuSpriteText teamName = null!;
        private OsuSpriteText shortName = null!;
        private ExternalLinkButton showTeamExternal = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        cover = new ProfileCoverBackground
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Height = 85,
                            Spacing = new Vector2(20, 0),
                            Padding = new MarginPadding
                            {
                                Left = WaveOverlayContainer.HORIZONTAL_PADDING,
                                Vertical = 10,
                            },
                            Children = new Drawable[]
                            {
                                teamFlag = new UpdateableTeamFlag(isInteractive: false, hideOnNull: false)
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Width = 240,
                                    Height = 120,
                                    CornerRadius = 40,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Offset = new Vector2(0, 1),
                                        Radius = 3,
                                        Colour = Colour4.Black.Opacity(0.25f),
                                    },
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5),
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(5, 0),
                                            Children = new Drawable[]
                                            {
                                                teamName = new OsuSpriteText
                                                {
                                                    Font = OsuFont.GetFont(size: 24, weight: FontWeight.Regular),
                                                },
                                                showTeamExternal = new ExternalLinkButton
                                                {
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                },
                                            },
                                        },
                                        shortName = new OsuSpriteText
                                        {
                                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular),
                                        },
                                    },
                                },
                            },
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            TeamData.BindValueChanged(data => updateTeam(data.NewValue?.Team), true);
        }

        private void updateTeam(APITeam? team)
        {
            cover.Item = team;
            teamFlag.Team = team;
            teamName.Text = team?.Name ?? string.Empty;
            shortName.Text = team != null ? $"[{team.ShortName}]" : string.Empty;
            showTeamExternal.Link = $@"{api.Endpoints.WebsiteUrl}/teams/{team?.Id ?? 0}";
        }

        private partial class ProfileCoverBackground : CoverBackground
        {
            protected override double LoadDelay => 0;

            public ProfileCoverBackground()
            {
                Masking = true;
                Height = 250;
            }
        }
    }
}
