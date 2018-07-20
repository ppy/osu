// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContent : FillFlowContainer<ChangelogContentGroup>
    {
        public APIChangelog CurrentBuild { get; private set; }
        public Action OnBuildChanged;
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
            CurrentBuild = changelog;
            fetchChangelogBuild(changelog);
        }

        private void showBuild(APIChangelog changelog)
        {
            ShowBuild(changelog);
            OnBuildChanged();
        }

        private void showNext()
        {
            if (CurrentBuild.Versions.Next != null)
                showBuild(CurrentBuild.Versions.Next);
        }

        private void showPrevious()
        {
            if (CurrentBuild.Versions.Previous != null)
                showBuild(CurrentBuild.Versions.Previous);
        }

        private void updateChevronTooltips()
        {
            changelogContentGroup.UpdateChevronTooltips(CurrentBuild.Versions.Previous?.DisplayVersion,
                CurrentBuild.Versions.Next?.DisplayVersion);
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
                CurrentBuild = res;
                updateChevronTooltips();
            };
            api.Queue(req);
        }
    }
}
