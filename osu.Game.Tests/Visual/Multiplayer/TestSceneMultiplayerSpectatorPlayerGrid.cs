// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerSpectatorPlayerGrid : OsuManualInputManagerTestScene
    {
        private PlayerGrid grid;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = grid = new PlayerGrid { RelativeSizeAxes = Axes.Both };
        });

        [Test]
        public void TestMaximiseAndMinimise()
        {
            addCells(2);

            assertMaximisation(0, false, true);
            assertMaximisation(1, false, true);

            clickCell(0);
            assertMaximisation(0, true);
            assertMaximisation(1, false, true);
            clickCell(0);
            assertMaximisation(0, false);
            assertMaximisation(1, false, true);

            clickCell(1);
            assertMaximisation(1, true);
            assertMaximisation(0, false, true);
            clickCell(1);
            assertMaximisation(1, false);
            assertMaximisation(0, false, true);
        }

        [Test]
        public void TestClickBothCellsSimultaneously()
        {
            addCells(2);

            AddStep("click cell 0 then 1", () =>
            {
                InputManager.MoveMouseTo(grid.Content.ElementAt(0));
                InputManager.Click(MouseButton.Left);

                InputManager.MoveMouseTo(grid.Content.ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });

            assertMaximisation(1, true);
            assertMaximisation(0, false);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(9)]
        [TestCase(11)]
        [TestCase(12)]
        [TestCase(15)]
        [TestCase(16)]
        public void TestCellCount(int count)
        {
            addCells(count);
            AddWaitStep("wait for display", 2);
        }

        private void addCells(int count) => AddStep($"add {count} grid cells", () =>
        {
            for (int i = 0; i < count; i++)
                grid.Add(new GridContent());
        });

        private void clickCell(int index) => AddStep($"click cell index {index}", () =>
        {
            InputManager.MoveMouseTo(grid.Content.ElementAt(index));
            InputManager.Click(MouseButton.Left);
        });

        private void assertMaximisation(int index, bool shouldBeMaximised, bool instant = false)
        {
            string assertionText = $"cell index {index} {(shouldBeMaximised ? "is" : "is not")} maximised";

            if (instant)
                AddAssert(assertionText, checkAction);
            else
                AddUntilStep(assertionText, checkAction);

            bool checkAction() => Precision.AlmostEquals(grid.MaximisedFacade.DrawSize, grid.Content.ElementAt(index).DrawSize, 10) == shouldBeMaximised;
        }

        private partial class GridContent : Box
        {
            public GridContent()
            {
                RelativeSizeAxes = Axes.Both;
                Colour = new Color4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1f);
            }
        }
    }
}
