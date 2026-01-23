// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Team.Sections.Members
{
    public partial class MembersGroup : CompositeDrawable
    {
        public readonly Bindable<TeamProfileData?> TeamData = new Bindable<TeamProfileData?>();

        private OsuSpriteText memberCount = null!;
        private FillFlowContainer membersContainer = null!;
        private ShowMoreButton showMoreButton = null!;

        private const int per_page = 51;
        private PaginationParameters paginationParameters;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 10;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Background2,
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(5),
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Horizontal = 10 },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                    Colour = colourProvider.Content1,
                                    Shadow = false,
                                    Text = TeamsStrings.ShowMembersMembers,
                                },
                                memberCount = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                                    Colour = colourProvider.Content1,
                                    Shadow = false,
                                    Text = @"0",
                                },
                            }
                        },
                        membersContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(5),
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                showMoreButton = new ShowMoreButton
                                {
                                    Alpha = 0,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Action = fetchMembers,
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            TeamData.ValueChanged += data =>
            {
                if (data.OldValue?.Team.Id != data.NewValue?.Team.Id)
                    onTeamChanged(data.NewValue?.Team);
            };
            onTeamChanged(null);
        }

        private void onTeamChanged(APITeam? team)
        {
            paginationParameters = new PaginationParameters(0, per_page);
            membersContainer.Clear();

            if (team == null)
                return;

            fetchMembers();
        }

        private void fetchMembers()
        {
            if (TeamData.Value == null)
                return;

            var request = new GetTeamMembersRequest(TeamData.Value.Team.Id, paginationParameters);
            request.Success += res => Schedule(() => onSuccess(res));

            api.Queue(request);
        }

        private void onSuccess(TeamMembersResponse response)
        {
            memberCount.Text = $"{response.Total}";

            membersContainer.AddRange(response.Items.Select(createUserPanel));
            paginationParameters = paginationParameters.TakeNext(response.Items.Count);

            showMoreButton.IsLoading = false;
            showMoreButton.Alpha = membersContainer.Children.Count == response.Total ? 0 : 1;
        }

        private UserGridPanel createUserPanel(APITeamMember member) => new UserGridPanel(member.User)
        {
            Width = 250,
        };
    }
}
