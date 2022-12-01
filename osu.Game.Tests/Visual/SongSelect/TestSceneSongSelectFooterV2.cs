// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Select.FooterV2;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneSongSelectFooterV2 : OsuManualInputManagerTestScene
    {
        private FooterButtonRandomV2 randomButton = null!;

        private bool nextRandomCalled;
        private bool previousRandomCalled;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            nextRandomCalled = false;
            previousRandomCalled = false;

            FooterV2 footer;

            Child = footer = new FooterV2
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            footer.AddButton(new FooterButtonModsV2());
            footer.AddButton(randomButton = new FooterButtonRandomV2
            {
                NextRandom = () => nextRandomCalled = true,
                PreviousRandom = () => previousRandomCalled = true
            });
            footer.AddButton(new FooterButtonOptionsV2());

            InputManager.MoveMouseTo(Vector2.Zero);
        });

        [Test]
        public void TestState()
        {
            AddRepeatStep("toggle options state", () => this.ChildrenOfType<FooterButtonV2>().Last().Enabled.Toggle(), 20);
        }

        [Test]
        public void TestFooterRandom()
        {
            AddStep("press F2", () => InputManager.Key(Key.F2));
            AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        }

        [Test]
        public void TestFooterRandomViaMouse()
        {
            AddStep("click button", () =>
            {
                InputManager.MoveMouseTo(randomButton);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        }

        [Test]
        public void TestFooterRewind()
        {
            AddStep("press Shift+F2", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.PressKey(Key.F2);
                InputManager.ReleaseKey(Key.F2);
                InputManager.ReleaseKey(Key.LShift);
            });
            AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        }

        [Test]
        public void TestFooterRewindViaShiftMouseLeft()
        {
            AddStep("shift + click button", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.MoveMouseTo(randomButton);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.LShift);
            });
            AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        }

        [Test]
        public void TestFooterRewindViaMouseRight()
        {
            AddStep("right click button", () =>
            {
                InputManager.MoveMouseTo(randomButton);
                InputManager.Click(MouseButton.Right);
            });
            AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        }
    }
}
