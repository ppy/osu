// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Screens.Menu;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestCaseDisclaimer : ScreenTestCase
    {
        [Cached(typeof(IAPIProvider))]
        private readonly DummyAPIAccess api = new DummyAPIAccess();

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(api);

            AddStep("load disclaimer", () => LoadScreen(new Disclaimer()));

            AddStep("toggle support", () =>
            {
                api.LocalUser.Value = new User
                {
                    Username = api.LocalUser.Value.Username,
                    Id = api.LocalUser.Value.Id,
                    IsSupporter = !api.LocalUser.Value.IsSupporter,
                };
            });
        }
    }
}
