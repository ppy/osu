// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneSupporterDisplay : OsuTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("create display", () =>
            {
                Child = new SupporterDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };
            });

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
