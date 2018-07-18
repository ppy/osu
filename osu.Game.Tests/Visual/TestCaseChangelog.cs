// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseChangelog : OsuTestCase
    {
        private ChangelogOverlay changelog;
        private int releaseStreamCount;
        private int index;
        private void indexIncrement() => index = (index == releaseStreamCount - 1) ? 0 : index + 1;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(changelog = new ChangelogOverlay());

            releaseStreamCount = changelog.streams.badgesContainer.Children.Count;

            AddStep(@"Show", changelog.Show);
            AddRepeatStep(@"Toggle Release Stream", () =>
            {
                changelog.streams.badgesContainer.Children[index].Activate();
                indexIncrement();
            }, releaseStreamCount);
            AddStep(@"Listing", changelog.ActivateListing);
            AddStep(@"Hide", changelog.Hide);
            AddWaitStep(4);
            AddStep(@"Show with Release Stream", () =>
            {
                changelog.streams.badgesContainer.Children[index].Activate();
                changelog.Show();
                indexIncrement();
            });
            AddWaitStep(4);
            AddStep(@"Hide", changelog.Hide);
            AddWaitStep(4);
            AddStep(@"Show with listing", () =>
            {
                // .maybe changelog should have a function that does header.ActivateListing()
                changelog.ActivateListing();
                changelog.Show();
            });
            AddWaitStep(4);
            AddStep(@"Hide", changelog.Hide);
            AddWaitStep(4);
            AddStep(@"Activate release", () =>
            {
                changelog.streams.badgesContainer.Children[index].Activate();
                indexIncrement();
            });
            AddStep(@"Show with listing", () =>
            {
                changelog.ActivateListing();
                changelog.Show();
            });
        }

        public TestCaseChangelog()
        {
        }
    }
}
