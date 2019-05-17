// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogBuild : ChangelogContent
    {
        private readonly APIChangelogBuild changelogBuild;

        public ChangelogBuild(APIChangelogBuild changelogBuild)
        {
            this.changelogBuild = changelogBuild;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var changelogContentGroup = new ChangelogContentGroup(changelogBuild);
            changelogContentGroup.GenerateText(changelogBuild.ChangelogEntries);
            changelogContentGroup.UpdateChevronTooltips(changelogBuild.Versions.Previous?.DisplayVersion,
                changelogBuild.Versions.Next?.DisplayVersion);
            changelogContentGroup.BuildSelected += SelectBuild;

            Add(changelogContentGroup);
        }
    }
}
