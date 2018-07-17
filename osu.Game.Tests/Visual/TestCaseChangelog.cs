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
            
            AddStep(@"Show", changelog.Show);
            AddStep(@"Stable Release Stream", () => changelog.header.ShowReleaseStream("Stable", "Stable 20180626.1"));
            AddStep(@"Lazer Release Stream", () => changelog.header.ShowReleaseStream("Lazer", "Lazer 2018.713.1"));
            AddStep(@"Listing", changelog.header.ActivateListing);
        }

        public TestCaseChangelog()
        {
            
        }
    }
}
