// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using System;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContent : FillFlowContainer
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
            Padding = new MarginPadding{ Bottom = 100, };
        }

        private void add(APIChangelog[] changelog)
        {
            DateTime currentDate = new DateTime();

            Clear();

            foreach (APIChangelog build in changelog)
            {
                if (build.CreatedAt.Date != currentDate)
                {
                    if (Children.Count != 0)
                    {
                        Add(new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Colour = new Color4(17, 17, 17, 255),
                            Margin = new MarginPadding { Top = 30, },
                        });
                    }
                    Add(changelogContentGroup = new ChangelogContentGroup(build, true)
                    {
                        BuildRequested = () => showBuild(build),
                    });
                    changelogContentGroup.GenerateText(build.ChangelogEntries);
                    currentDate = build.CreatedAt.Date;
                }
                else
                {
                    changelogContentGroup.Add(new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Colour = new Color4(32, 24, 35, 255),
                        Margin = new MarginPadding { Top = 30, },
                    });
                    Add(changelogContentGroup = new ChangelogContentGroup(build, false)
                    {
                        BuildRequested = () => showBuild(build),
                    });
                    changelogContentGroup.GenerateText(build.ChangelogEntries);
                }
            }
        }

        private void add(APIChangelog changelogBuild)
        {
            Child = changelogContentGroup = new ChangelogContentGroup(changelogBuild)
            {
                PreviousRequested = showPrevious,
                NextRequested = showNext,
            };
        }

        /// <summary>
        /// Doesn't send back that the build has changed
        /// </summary>
        public void ShowBuild(APIChangelog changelog)
        {
            fetchAndShowChangelogBuild(changelog);
            CurrentBuild = changelog;
        }

        /// <summary>
        /// Sends back that the build has changed
        /// </summary>
        private void showBuild(APIChangelog changelog)
        {
            ShowBuild(changelog);
            OnBuildChanged();
        }

        public void ShowListing() => fetchAndShowChangelog();

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

        private void fetchAndShowChangelog()
        {
            var req = new GetChangelogRequest();
            req.Success += add;
            api.Queue(req);
        }

        private void fetchAndShowChangelogBuild(APIChangelog build)
        {
            var req = new GetChangelogBuildRequest(build.UpdateStream.Name, build.Version);
            req.Success += res =>
            {
                CurrentBuild = res;
                add(CurrentBuild);
                changelogContentGroup.GenerateText(CurrentBuild.ChangelogEntries);
                updateChevronTooltips();
            };
            api.Queue(req);
        }
    }
}
