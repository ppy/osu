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
        private int index;
        private void indexIncrement() => index = index >= changelog.Streams.BadgesContainer.Children.Count - 1 ? 0 : index + 1;
        private bool isLoaded => changelog.Streams.BadgesContainer.Children.Count > 0;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(changelog = new ChangelogOverlay());

            AddStep(@"Show", changelog.Show);
            AddRepeatStep(@"Toggle Release Stream", () =>
            {
                if (isLoaded) changelog.Streams.BadgesContainer.Children[index].Activate();
                indexIncrement();
            }, 6);
            AddStep(@"Listing", changelog.ActivateListing);
            AddStep(@"Hide", changelog.Hide);
            AddWaitStep(3);
            AddStep(@"Show with Release Stream", () =>
            {
                if (isLoaded) changelog.Streams.BadgesContainer.Children[index].Activate();
                changelog.Show();
                indexIncrement();
            });
            AddWaitStep(3);
            AddStep(@"Hide", changelog.Hide);
            AddWaitStep(3);
            AddStep(@"Show with listing", () =>
            {
                changelog.ActivateListing();
                changelog.Show();
            });
            AddWaitStep(3);
            AddStep(@"Hide", changelog.Hide);
            AddWaitStep(3);
            AddStep(@"Activate release", () =>
            {
                if (isLoaded) changelog.Streams.BadgesContainer.Children[index].Activate();
                indexIncrement();
            });
            AddStep(@"Show with listing", () =>
            {
                changelog.ActivateListing();
                changelog.Show();
            });
        }
    }
}
