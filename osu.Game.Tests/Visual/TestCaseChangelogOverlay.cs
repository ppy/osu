// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseChangelogOverlay : OsuTestCase
    {
        private ChangelogOverlay changelog;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(changelog = new ChangelogOverlay());
            AddStep(@"Show", changelog.Show);
            AddStep(@"Hide", changelog.Hide);

            AddWaitStep("wait for hide", 3);

            AddStep(@"Show with Lazer 2018.712.0", () =>
            {
                changelog.FetchAndShowBuild(new APIChangelogBuild
                {
                    Version = "2018.712.0",
                    UpdateStream = new APIUpdateStream { Name = "lazer" },
                });
                changelog.Show();
            });

            AddWaitStep("wait for show", 3);
            AddStep(@"Hide", changelog.Hide);
            AddWaitStep("wait for hide", 3);

            AddStep(@"Show with listing", () =>
            {
                changelog.ShowListing();
                changelog.Show();
            });
        }
    }
}
