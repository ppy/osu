// Copyright(c) 2007-2018 ppy Pty Ltd<contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


using NUnit.Framework;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseChangelog : OsuTestCase
    {
        private ChangelogOverlay changelog;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(changelog = new ChangelogOverlay());
            changelog.ToggleVisibility();

            //AddStep(@"toggle", changelog.ToggleVisibility);
            AddStep(@"toggle text 1", () => changelog.header.ActivateRelease("Release 20180626.1"));
            AddStep(@"toggle text 2", () => changelog.header.ActivateRelease("Lazer 2018.713.1"));
            AddStep(@"toggle text 3", () => changelog.header.ActivateRelease("Beta 20180626"));
            AddStep(@"go to listing", changelog.header.ActivateListing);
        }

        public TestCaseChangelog()
        {
            
        }
    }
}
