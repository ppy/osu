// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Screens.Menu;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneDisclaimer : ScreenTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("load disclaimer", () => LoadScreen(new Disclaimer()));

            AddStep("toggle support", () =>
            {
                API.LocalUser.Value = new User
                {
                    Username = API.LocalUser.Value.Username,
                    Id = API.LocalUser.Value.Id + 1,
                    IsSupporter = !API.LocalUser.Value.IsSupporter,
                };
            });
        }
    }
}
