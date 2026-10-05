// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerUserModDisplay : MultiplayerTestScene
    {
        private MultiplayerUserModDisplay modDisplay = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom()));
            WaitForJoined();

            AddStep("add display", () => Child = modDisplay = new MultiplayerUserModDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestChangeMods()
        {
            AddStep("set DT", () => MultiplayerClient.ChangeUserMods([new OsuModDoubleTime()]).WaitSafely());
            AddUntilStep("mod displayed", () => modDisplay.ChildrenOfType<ModIcon>().Count() == 1);

            AddStep("set DT, HR", () => MultiplayerClient.ChangeUserMods([new OsuModDoubleTime(), new OsuModHardRock()]).WaitSafely());
            AddUntilStep("mods displayed", () => modDisplay.ChildrenOfType<ModIcon>().Count() == 2);

            AddStep("set no mods", () => MultiplayerClient.ChangeUserMods(Enumerable.Empty<APIMod>()).WaitSafely());
            AddUntilStep("no mods displayed", () => !modDisplay.ChildrenOfType<ModIcon>().Any());
        }
    }
}
