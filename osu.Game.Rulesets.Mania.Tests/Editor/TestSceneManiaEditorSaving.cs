// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneManiaEditorSaving : EditorSavingTestScene
    {
        protected override Ruleset CreateRuleset() => new ManiaRuleset();

        [Test]
        public void TestKeyCountChange()
        {
            FormSliderBar<float> keyCount = null!;

            AddStep("go to setup screen", () => InputManager.Key(Key.F4));
            AddUntilStep("retrieve key count slider", () => keyCount = Editor.ChildrenOfType<SetupScreen>().Single().ChildrenOfType<FormSliderBar<float>>().First(), () => Is.Not.Null);
            AddAssert("key count is 5", () => keyCount.Current.Value, () => Is.EqualTo(5));
            AddStep("change key count to 8", () =>
            {
                keyCount.Current.Value = 8;
            });
            AddUntilStep("dialog visible", () => Game.ChildrenOfType<IDialogOverlay>().SingleOrDefault()?.CurrentDialog, Is.InstanceOf<SaveAndReloadEditorDialog>);
            AddStep("refuse", () => InputManager.Key(Key.Number2));
            AddAssert("key count is 5", () => keyCount.Current.Value, () => Is.EqualTo(5));

            AddStep("change key count to 8 again", () =>
            {
                keyCount.Current.Value = 8;
            });
            AddUntilStep("dialog visible", () => Game.ChildrenOfType<IDialogOverlay>().Single().CurrentDialog, Is.InstanceOf<SaveAndReloadEditorDialog>);
            AddStep("acquiesce", () => InputManager.Key(Key.Number1));
            AddUntilStep("beatmap became 8K", () => Game.Beatmap.Value.BeatmapInfo.Difficulty.CircleSize, () => Is.EqualTo(8));
        }
    }
}
