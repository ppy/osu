// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseChangelog : OsuTestCase
    {
        private ChangelogOverlay changelog;
        private int releaseStreamCount;
        private int index;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(changelog = new ChangelogOverlay());

            releaseStreamCount = changelog.streams.badgesContainer.Children.Count;

            AddStep(@"Show", changelog.Show);
            AddRepeatStep(@"Toggle Release Stream", () => {
                changelog.streams.badgesContainer.Children[index].Activate();
                index = (index == releaseStreamCount - 1) ? 0 : index + 1;
            }, releaseStreamCount);
            AddStep(@"Listing", changelog.header.ActivateListing);
        }

        public TestCaseChangelog()
        {
            
        }
    }
}
