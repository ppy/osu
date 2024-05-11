// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Testing;
using osu.Game.Screens.SelectV2;
using osu.Game.Screens.SelectV2.Footer;
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
            AddStep("load screen", () => Game.ScreenStack.Push(new SongSelectV2()));
            AddWaitStep("wait", 5);
            AddStep("move to osu", () => InputManager.MoveMouseTo(this.ChildrenOfType<OsuLogoButton>().Single()));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
        }
    }
}
