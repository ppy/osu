// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Menu;

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
                ((DummyAPIAccess)API).LocalUser.Value = new APIUser
                {
                    Username = API.LocalUser.Value.Username,
                    Id = API.LocalUser.Value.Id + 1,
                    IsSupporter = !API.LocalUser.Value.IsSupporter,
                };
            });
        }
    }
}
