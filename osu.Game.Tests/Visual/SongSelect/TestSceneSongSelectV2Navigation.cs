// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Screens.Menu;
using osu.Game.Screens.SelectV2;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneSongSelectV2Navigation : OsuGameTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("press enter", () => InputManager.Key(Key.Enter));
            AddWaitStep("wait", 5);
            PushAndConfirm(() => new SongSelectV2());
        }

        [Test]
        public void TestClickLogo()
        {
            AddStep("click", () =>
            {
                InputManager.MoveMouseTo(Game.ChildrenOfType<OsuLogo>().Single());
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
