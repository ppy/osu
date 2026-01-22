// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Team;
using osu.Game.Overlays.Team.Sections;
using osu.Game.Rulesets;
using osu.Game.Teams;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class TeamProfileOverlay : FullscreenOverlay<TeamProfileHeader>
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private readonly OnlineViewContainer onlineViewContainer;
        private readonly LoadingLayer loadingLayer;

        private GetTeamRequest? req;

        private IBindable<APIUser> apiUser = new Bindable<APIUser>();

        private readonly Bindable<TeamProfileData?> teamData = new Bindable<TeamProfileData?>();
        private (ITeam team, IRulesetInfo? ruleset)? lastLookup;

        protected override Container<Drawable> Content => onlineViewContainer;

        public TeamProfileOverlay()
            : base(OverlayColourScheme.Pink)
        {
            base.Content.Add(new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    onlineViewContainer = new OnlineViewContainer($"Sign in to view the {Header.Title.Title}")
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    loadingLayer = new LoadingLayer(true),
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.ValueChanged += _ => Schedule(() =>
            {
                if (api.IsLoggedIn)
                    Scheduler.AddOnce(fetchAndSetContent);
            });
            Child = new OverlayScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        Header,
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 10),
                            Padding = new MarginPadding(10),
                            Children = new Drawable[]
                            {
                                new InfoSection { TeamData = { BindTarget = teamData } },
                                new MembersSection { TeamData = { BindTarget = teamData } },
                            },
                        },
                    },
                },
            };
        }

        protected override TeamProfileHeader CreateHeader() => new TeamProfileHeader { TeamData = { BindTarget = teamData } };

        public void ShowTeam(ITeam teamToShow, IRulesetInfo? teamRuleset = null)
        {
            lastLookup = (teamToShow, teamRuleset);

            Show();
            fetchAndSetContent();
        }

        private void fetchAndSetContent()
        {
            if (lastLookup == null)
                return;

            req?.Cancel();

            if (!api.IsLoggedIn)
                return;

            req = new GetTeamRequest(lastLookup.Value.team.OnlineID, lastLookup.Value.ruleset);
            req.Success += team => teamLoadComplete(team, lastLookup.Value.ruleset);

            API.Queue(req);
            loadingLayer.Show();
        }

        private void teamLoadComplete(APITeam team, IRulesetInfo? ruleset)
        {
            var actualRuleset = rulesets.GetRuleset(ruleset?.OnlineID ?? team.DefaultRulesetId).AsNonNull();

            teamData.Value = new TeamProfileData(team, actualRuleset);
            loadingLayer.Hide();
        }
    }
}
