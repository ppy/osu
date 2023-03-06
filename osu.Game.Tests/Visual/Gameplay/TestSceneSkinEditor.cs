// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinEditor : PlayerTestScene
    {
        private SkinEditor skinEditor = null!;

        protected override bool Autoplay => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached]
        public readonly EditorClipboard Clipboard = new EditorClipboard();

        private SkinComponentsContainer targetContainer => Player.ChildrenOfType<SkinComponentsContainer>().First();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddUntilStep("wait for hud load", () => targetContainer.ComponentsLoaded);

            AddStep("reload skin editor", () =>
            {
                if (skinEditor.IsNotNull())
                    skinEditor.Expire();
                Player.ScaleTo(0.4f);
                LoadComponentAsync(skinEditor = new SkinEditor(Player), Add);
            });
            AddUntilStep("wait for loaded", () => skinEditor.IsLoaded);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestBringToFront(bool alterSelectionOrder)
        {
            AddAssert("Ensure over three components available", () => targetContainer.Components.Count, () => Is.GreaterThan(3));

            IEnumerable<ISerialisableDrawable> originalOrder = null!;

            AddStep("Save order of components before operation", () => originalOrder = targetContainer.Components.Take(3).ToArray());

            if (alterSelectionOrder)
                AddStep("Select first three components in reverse order", () => skinEditor.SelectedComponents.AddRange(originalOrder.Reverse()));
            else
                AddStep("Select first three components", () => skinEditor.SelectedComponents.AddRange(originalOrder));

            AddAssert("Components are not front-most", () => targetContainer.Components.TakeLast(3).ToArray(), () => Is.Not.EqualTo(skinEditor.SelectedComponents));

            AddStep("Bring to front", () => skinEditor.BringSelectionToFront());
            AddAssert("Ensure components are now front-most in original order", () => targetContainer.Components.TakeLast(3).ToArray(), () => Is.EqualTo(originalOrder));
            AddStep("Bring to front again", () => skinEditor.BringSelectionToFront());
            AddAssert("Ensure components are still front-most in original order", () => targetContainer.Components.TakeLast(3).ToArray(), () => Is.EqualTo(originalOrder));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSendToBack(bool alterSelectionOrder)
        {
            AddAssert("Ensure over three components available", () => targetContainer.Components.Count, () => Is.GreaterThan(3));

            IEnumerable<ISerialisableDrawable> originalOrder = null!;

            AddStep("Save order of components before operation", () => originalOrder = targetContainer.Components.TakeLast(3).ToArray());

            if (alterSelectionOrder)
                AddStep("Select last three components in reverse order", () => skinEditor.SelectedComponents.AddRange(originalOrder.Reverse()));
            else
                AddStep("Select last three components", () => skinEditor.SelectedComponents.AddRange(originalOrder));

            AddAssert("Components are not back-most", () => targetContainer.Components.Take(3).ToArray(), () => Is.Not.EqualTo(skinEditor.SelectedComponents));

            AddStep("Send to back", () => skinEditor.SendSelectionToBack());
            AddAssert("Ensure components are now back-most in original order", () => targetContainer.Components.Take(3).ToArray(), () => Is.EqualTo(originalOrder));
            AddStep("Send to back again", () => skinEditor.SendSelectionToBack());
            AddAssert("Ensure components are still back-most in original order", () => targetContainer.Components.Take(3).ToArray(), () => Is.EqualTo(originalOrder));
        }

        [Test]
        public void TestToggleEditor()
        {
            AddToggleStep("toggle editor visibility", _ => skinEditor.ToggleVisibility());
        }

        [Test]
        public void TestEditComponent()
        {
            BarHitErrorMeter hitErrorMeter = null!;

            AddStep("select bar hit error blueprint", () =>
            {
                var blueprint = skinEditor.ChildrenOfType<SkinBlueprint>().First(b => b.Item is BarHitErrorMeter);

                hitErrorMeter = (BarHitErrorMeter)blueprint.Item;
                skinEditor.SelectedComponents.Clear();
                skinEditor.SelectedComponents.Add(blueprint.Item);
            });

            AddStep("move by keyboard", () => InputManager.Key(Key.Right));

            AddAssert("hitErrorMeter moved", () => hitErrorMeter.X != 0);

            AddAssert("value is default", () => hitErrorMeter.JudgementLineThickness.IsDefault);

            AddStep("hover first slider", () =>
            {
                InputManager.MoveMouseTo(
                    skinEditor.ChildrenOfType<SkinSettingsToolbox>().First()
                              .ChildrenOfType<SettingsSlider<float>>().First()
                              .ChildrenOfType<SliderBar<float>>().First()
                );
            });

            AddStep("adjust slider via keyboard", () => InputManager.Key(Key.Left));

            AddAssert("value is less than default", () => hitErrorMeter.JudgementLineThickness.Value < hitErrorMeter.JudgementLineThickness.Default);
        }

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();
    }
}
