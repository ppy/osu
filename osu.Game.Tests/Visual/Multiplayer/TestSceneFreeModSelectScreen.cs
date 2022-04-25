// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneFreeModSelectScreen : MultiplayerTestScene
    {
        [Test]
        public void TestFreeModSelect()
        {
            FreeModSelectScreen freeModSelectScreen = null;

            AddStep("create free mod select screen", () => Child = freeModSelectScreen = new FreeModSelectScreen
            {
                State = { Value = Visibility.Visible }
            });
            AddUntilStep("all column content loaded",
                () => freeModSelectScreen.ChildrenOfType<ModColumn>().Any()
                      && freeModSelectScreen.ChildrenOfType<ModColumn>().All(column => column.IsLoaded && column.ItemsLoaded));

            AddUntilStep("all visible mods are playable",
                () => this.ChildrenOfType<ModPanel>()
                          .Where(panel => panel.IsPresent)
                          .All(panel => panel.Mod.HasImplementation && panel.Mod.UserPlayable));

            AddToggleStep("toggle visibility", visible =>
            {
                if (freeModSelectScreen != null)
                    freeModSelectScreen.State.Value = visible ? Visibility.Visible : Visibility.Hidden;
            });
        }
    }
}
