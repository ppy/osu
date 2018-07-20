// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContent : FillFlowContainer<ChangelogContentGroup>
    {
        private APIChangelog currentBuild;
        private APIAccess api;
        private ChangelogContentGroup changelogContentGroup;

        public ChangelogContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding
            {
                Left = 70,
                Right = 70,
            };
        }

        private void add(APIChangelog changelogBuild)
        {
            Add(changelogContentGroup = new ChangelogContentGroup(changelogBuild)
                {
                    PreviousRequested = showPrevious,
                    NextRequested = showNext,
                });
        }

        public void ShowBuild(APIChangelog changelog)
        {
            Clear();
            add(changelog);
            fetchChangelogBuild(changelog);
        }

        private void showNext()
        {
            if (currentBuild.Versions.Next != null)
                ShowBuild(currentBuild.Versions.Next);
        }

        private void showPrevious()
        {
            if (currentBuild.Versions.Previous != null)
                ShowBuild(currentBuild.Versions.Previous);
        }

        private void updateChevronTooltips()
        {
            changelogContentGroup.UpdateChevronTooltips(currentBuild.Versions.Previous?.DisplayVersion,
                currentBuild.Versions.Next?.DisplayVersion);
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        private void fetchChangelogBuild(APIChangelog build)
        {
            var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);
            req.Success += res =>
            {
                currentBuild = res;
                updateChevronTooltips();
            };
            api.Queue(req);
        }
    }
}
