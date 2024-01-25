// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestScenePreciseRotation : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset);

        [Test]
        public void TestHotkeyHandling()
        {
            AddStep("deselect everything", () => EditorBeatmap.SelectedHitObjects.Clear());
            AddStep("press rotate hotkey", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddUntilStep("no popover present", getPopover, () => Is.Null);

            AddStep("select single circle",
                () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.OfType<HitCircle>().First()));
            AddStep("press rotate hotkey", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddUntilStep("popover present", getPopover, () => Is.Not.Null);
            AddAssert("only playfield centre origin rotation available", () =>
            {
                var popover = getPopover();
                var buttons = popover.ChildrenOfType<EditorRadioButton>();
                return buttons.Any(btn => btn.Text == "Selection centre" && btn.Enabled.Value is false) &&
                    buttons.Any(btn => btn.Text == "Playfield centre" && btn.Enabled.Value is true);
            });
            AddStep("press rotate hotkey", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddUntilStep("no popover present", getPopover, () => Is.Null);

            AddStep("select first three objects", () =>
            {
                EditorBeatmap.SelectedHitObjects.Clear();
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects.Take(3));
            });
            AddStep("press rotate hotkey", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddUntilStep("popover present", getPopover, () => Is.Not.Null);
            AddAssert("both origin rotation available", () =>
            {
                var popover = getPopover();
                var buttons = popover.ChildrenOfType<EditorRadioButton>();
                return buttons.Any(btn => btn.Text == "Selection centre" && btn.Enabled.Value is true) &&
                    buttons.Any(btn => btn.Text == "Playfield centre" && btn.Enabled.Value is true);
            });
            AddStep("press rotate hotkey", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddUntilStep("no popover present", getPopover, () => Is.Null);

            PreciseRotationPopover? getPopover() => this.ChildrenOfType<PreciseRotationPopover>().SingleOrDefault();
        }

        [Test]
        public void TestRotateCorrectness()
        {
            AddStep("replace objects", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.AddRange(new HitObject[]
                {
                    new HitCircle { Position = new Vector2(100) },
                    new HitCircle { Position = new Vector2(200) },
                });
            });
            AddStep("select both circles", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            AddStep("press rotate hotkey", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddUntilStep("popover present", getPopover, () => Is.Not.Null);

            AddStep("rotate by 180deg", () => getPopover().ChildrenOfType<TextBox>().Single().Current.Value = "180");
            AddAssert("first object rotated 180deg around playfield centre",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(0).Position,
                () => Is.EqualTo(OsuPlayfield.BASE_SIZE - new Vector2(100)));
            AddAssert("second object rotated 180deg around playfield centre",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(1).Position,
                () => Is.EqualTo(OsuPlayfield.BASE_SIZE - new Vector2(200)));

            AddStep("change rotation origin", () => getPopover().ChildrenOfType<EditorRadioButton>().ElementAt(1).TriggerClick());
            AddAssert("first object rotated 90deg around selection centre",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(0).Position, () => Is.EqualTo(new Vector2(200, 200)));
            AddAssert("second object rotated 90deg around selection centre",
                () => EditorBeatmap.HitObjects.OfType<HitCircle>().ElementAt(1).Position, () => Is.EqualTo(new Vector2(100, 100)));

            PreciseRotationPopover? getPopover() => this.ChildrenOfType<PreciseRotationPopover>().SingleOrDefault();
        }
    }
}
