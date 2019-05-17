// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogBuild : ChangelogContent
    {
        private APIChangelogBuild changelogBuild;

        public ChangelogBuild(APIChangelogBuild changelogBuild)
        {
            this.changelogBuild = changelogBuild;
        }

        [BackgroundDependencyLoader]
        private void load(CancellationToken? cancellation, IAPIProvider api)
        {
            var req = new GetChangelogBuildRequest(changelogBuild.UpdateStream.Name, changelogBuild.Version);
            bool complete = false;

            req.Success += res =>
            {
                changelogBuild = res;
                complete = true;
            };

            req.Failure += _ => complete = true;

            api.Queue(req);

            while (!complete && cancellation?.IsCancellationRequested != true)
                Task.Delay(1);

            var changelogContentGroup = new ChangelogContentGroup(changelogBuild);
            changelogContentGroup.GenerateText(changelogBuild.ChangelogEntries);
            changelogContentGroup.UpdateChevronTooltips(changelogBuild.Versions.Previous?.DisplayVersion,
                changelogBuild.Versions.Next?.DisplayVersion);
            changelogContentGroup.BuildSelected += SelectBuild;

            Add(changelogContentGroup);
        }
    }
}
