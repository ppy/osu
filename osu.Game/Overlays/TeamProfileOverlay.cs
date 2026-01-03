// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Team;
using osu.Game.Rulesets;
using osu.Game.Teams;

namespace osu.Game.Overlays
{
    public partial class TeamProfileOverlay : OnlineOverlay<TeamHeader>
    {
        private GetTeamRequest? req;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private IBindable<APIUser> apiUser = new Bindable<APIUser>();

        private readonly Bindable<TeamProfileData?> profileData = new Bindable<TeamProfileData?>();
        private (ITeam team, IRulesetInfo? ruleset)? lastLookup;

        public TeamProfileOverlay()
            : base(OverlayColourScheme.Pink)
        {
            Header.TeamData.BindTo(profileData);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.ValueChanged += _ => Schedule(() =>
            {
                if (api.IsLoggedIn)
                    fetchAndSetContent();
            });
        }

        protected override TeamHeader CreateHeader() => new TeamHeader();

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
            Loading.Show();
        }

        private void teamLoadComplete(APITeam team, IRulesetInfo? ruleset)
        {
            var actualRuleset = rulesets.GetRuleset(ruleset?.ShortName ?? team.DefaultRuleset).AsNonNull();

            profileData.Value = new TeamProfileData(team, actualRuleset);
            Loading.Hide();
        }
    }
}
