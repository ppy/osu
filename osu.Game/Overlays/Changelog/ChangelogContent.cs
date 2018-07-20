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

        public override void Add(ChangelogContentGroup changelogContentGroup)
        {
            if (changelogContentGroup != null)
            {
                changelogContentGroup.PreviousRequested = showPrevious;
                changelogContentGroup.NextRequested = showNext;
            }
            base.Add(changelogContentGroup);
        }

        public void ShowBuild(APIChangelog changelog)
        {
            Clear();
            Add(new ChangelogContentGroup(changelog));
            //fetchChangelogBuild(changelog);
            fetchChangelogBuild();
        }

        private void showNext()
        {
            if (currentBuild.Versions.Next != null)
            {
                Clear();
                Add(new ChangelogContentGroup(currentBuild.Versions.Next));
                //fetchChangelogBuild(currentBuild.Versions.Next);
                fetchChangelogBuild();
            }
        }

        private void showPrevious()
        {
            if (currentBuild.Versions.Previous != null)
            {
                Clear();
                Add(new ChangelogContentGroup(currentBuild.Versions.Previous));
                //fetchChangelogBuild(currentBuild.Versions.Previous);
                fetchChangelogBuild();
            }
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        //private void fetchChangelogBuild(APIChangelog build)
        private void fetchChangelogBuild()
        {
            //var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);
            var req = new GetChangelogBuildRequest();
            req.Success += res => currentBuild = res;
            api.Queue(req);
        }
    }
}
