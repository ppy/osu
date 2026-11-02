// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osu.Game.Tests.Resources;
using osuTK;
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

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private SkinnableContainer targetContainer => Player.ChildrenOfType<SkinnableContainer>().First();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset skin", () => skins.CurrentSkinInfo.SetDefault());
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

        [Test]
        public void TestDragSelection()
        {
            BigBlackBox box1 = null!;
            BigBlackBox box2 = null!;
            BigBlackBox box3 = null!;

            AddStep("Add big black boxes", () =>
            {
                var target = Player.ChildrenOfType<SkinnableContainer>().First();
                target.Add(box1 = new BigBlackBox
                {
                    Position = new Vector2(-90),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
                target.Add(box2 = new BigBlackBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
                target.Add(box3 = new BigBlackBox
                {
                    Position = new Vector2(90),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            });

            // This step is specifically added to reproduce an edge case which was found during cyclic selection development.
            // If everything is working as expected it should not affect the subsequent drag selections.
            AddRepeatStep("Select top left", () =>
            {
                InputManager.MoveMouseTo(box1.ScreenSpaceDrawQuad.TopLeft + new Vector2(box1.ScreenSpaceDrawQuad.Width / 8));
                InputManager.Click(MouseButton.Left);
            }, 2);

            AddStep("Begin drag top left", () =>
            {
                InputManager.MoveMouseTo(box1.ScreenSpaceDrawQuad.TopLeft - new Vector2(box1.ScreenSpaceDrawQuad.Width / 4, box1.ScreenSpaceDrawQuad.Height / 8));
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("Drag to bottom right", () =>
            {
                InputManager.MoveMouseTo(box3.ScreenSpaceDrawQuad.TopRight + new Vector2(-box3.ScreenSpaceDrawQuad.Width / 8, box3.ScreenSpaceDrawQuad.Height / 4));
            });

            AddStep("Release button", () =>
            {
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("First two boxes selected", () => skinEditor.SelectedComponents, () => Is.EqualTo(new[] { box1, box2 }));

            AddStep("Begin drag bottom right", () =>
            {
                InputManager.MoveMouseTo(box3.ScreenSpaceDrawQuad.BottomRight + new Vector2(box3.ScreenSpaceDrawQuad.Width / 4));
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("Drag to top left", () =>
            {
                InputManager.MoveMouseTo(box2.ScreenSpaceDrawQuad.Centre - new Vector2(box2.ScreenSpaceDrawQuad.Width / 4));
            });

            AddStep("Release button", () =>
            {
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("Last two boxes selected", () => skinEditor.SelectedComponents, () => Is.EqualTo(new[] { box2, box3 }));

            // Test cyclic selection doesn't trigger in this state.
            AddStep("click on black box stack", () => InputManager.Click(MouseButton.Left));
            AddAssert("Last two boxes still selected", () => skinEditor.SelectedComponents, () => Is.EqualTo(new[] { box2, box3 }));
        }

        [Test]
        public void TestCyclicSelection()
        {
            List<SkinBlueprint> blueprints = new List<SkinBlueprint>();

            AddStep("clear list", blueprints.Clear);

            for (int i = 0; i < 3; i++)
            {
                AddStep("Add big black box", () =>
                {
                    skinEditor.ChildrenOfType<SkinComponentToolbox.ToolboxComponentButton>().First(b => b.ChildrenOfType<BigBlackBox>().FirstOrDefault() != null).TriggerClick();
                });

                AddStep("store box", () =>
                {
                    // Add blueprints one-by-one so we have a stable order for testing reverse cyclic selection against.
                    blueprints.Add(skinEditor.ChildrenOfType<SkinBlueprint>().Single(s => s.IsSelected));
                });
            }

            AddAssert("Three black boxes added", () => targetContainer.Components.OfType<BigBlackBox>().Count(), () => Is.EqualTo(3));

            AddAssert("Selection is last", () => skinEditor.SelectedComponents.Single(), () => Is.EqualTo(blueprints[2].Item));

            AddStep("move cursor to black box", () =>
            {
                // Slightly offset from centre to avoid random failures (see https://github.com/ppy/osu-framework/issues/5669).
                InputManager.MoveMouseTo(((Drawable)blueprints[0].Item).ScreenSpaceDrawQuad.Centre + new Vector2(1));
            });

            AddStep("click on black box stack", () => InputManager.Click(MouseButton.Left));
            AddAssert("Selection is second last", () => skinEditor.SelectedComponents.Single(), () => Is.EqualTo(blueprints[1].Item));

            AddStep("click on black box stack", () => InputManager.Click(MouseButton.Left));
            AddAssert("Selection is last", () => skinEditor.SelectedComponents.Single(), () => Is.EqualTo(blueprints[0].Item));

            AddStep("click on black box stack", () => InputManager.Click(MouseButton.Left));
            AddAssert("Selection is first", () => skinEditor.SelectedComponents.Single(), () => Is.EqualTo(blueprints[2].Item));

            AddStep("select all boxes", () =>
            {
                skinEditor.SelectedComponents.Clear();
                skinEditor.SelectedComponents.AddRange(targetContainer.Components.OfType<BigBlackBox>().Skip(1));
            });

            AddAssert("all boxes selected", () => skinEditor.SelectedComponents, () => Has.Count.EqualTo(2));
            AddStep("click on black box stack", () => InputManager.Click(MouseButton.Left));
            AddStep("click on black box stack", () => InputManager.Click(MouseButton.Left));
            AddStep("click on black box stack", () => InputManager.Click(MouseButton.Left));
            AddAssert("all boxes still selected", () => skinEditor.SelectedComponents, () => Has.Count.EqualTo(2));
        }

        [Test]
        public void TestUndoEditHistory()
        {
            SkinnableContainer firstTarget = null!;
            TestSkinEditorChangeHandler changeHandler = null!;
            byte[] defaultState = null!;
            IEnumerable<ISerialisableDrawable> testComponents = null!;

            AddStep("Load necessary things", () =>
            {
                firstTarget = Player.ChildrenOfType<SkinnableContainer>().First();
                changeHandler = new TestSkinEditorChangeHandler(firstTarget);

                changeHandler.SaveState();
                defaultState = changeHandler.GetCurrentState();

                testComponents = new[]
                {
                    targetContainer.Components.First(),
                    targetContainer.Components[targetContainer.Components.Count / 2],
                    targetContainer.Components.Last()
                };
            });

            AddStep("Press undo", () => InputManager.Keys(PlatformAction.Undo));
            AddAssert("Nothing changed", () => defaultState.SequenceEqual(changeHandler.GetCurrentState()));

            AddStep("Add components", () =>
            {
                InputManager.MoveMouseTo(skinEditor.ChildrenOfType<BigBlackBox>().First());
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });
            revertAndCheckUnchanged();

            AddStep("Move components", () =>
            {
                changeHandler.BeginChange();
                testComponents.ForEach(c => ((Drawable)c).Position += Vector2.One);
                changeHandler.EndChange();
            });
            revertAndCheckUnchanged();

            AddStep("Select components", () => skinEditor.SelectedComponents.AddRange(testComponents));
            AddStep("Bring to front", () => skinEditor.BringSelectionToFront());
            revertAndCheckUnchanged();

            AddStep("Remove components", () => testComponents.ForEach(c => firstTarget.Remove(c, false)));
            revertAndCheckUnchanged();

            void revertAndCheckUnchanged()
            {
                AddStep("Revert changes", () => changeHandler.RestoreState(int.MinValue));
                AddAssert("Current state is same as default",
                    () => Encoding.UTF8.GetString(defaultState),
                    () => Is.EqualTo(Encoding.UTF8.GetString(changeHandler.GetCurrentState())));
            }
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
            ArgonAccuracyCounter accuracyCounter = null!;

            AddStep("select argon accuracy counter blueprint", () =>
            {
                var blueprint = skinEditor.ChildrenOfType<SkinBlueprint>().First(b => b.Item is ArgonAccuracyCounter);

                accuracyCounter = (ArgonAccuracyCounter)blueprint.Item;
                skinEditor.SelectedComponents.Clear();
                skinEditor.SelectedComponents.Add(blueprint.Item);
            });

            AddStep("move by keyboard", () => InputManager.Key(Key.Right));

            AddAssert("accuracyCounter moved", () => accuracyCounter.X != -20);

            AddAssert("value is default", () => accuracyCounter.WireframeOpacity.IsDefault);

            AddStep("hover first slider", () =>
            {
                InputManager.MoveMouseTo(
                    skinEditor.ChildrenOfType<SkinSettingsToolbox>().First()
                              .ChildrenOfType<SettingsSlider<float>>().First()
                              .ChildrenOfType<SliderBar<float>>().First()
                );
            });

            AddStep("adjust slider via keyboard", () => InputManager.Key(Key.Left));

            AddAssert("value is less than default", () => accuracyCounter.WireframeOpacity.Value < accuracyCounter.WireframeOpacity.Default);
        }

        [Test]
        public void TestCopyPaste()
        {
            AddStep("paste", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.V);
                InputManager.ReleaseKey(Key.LControl);
            });
            // no assertions. just make sure nothing crashes.

            AddStep("select argon accuracy counter blueprint", () =>
            {
                var blueprint = skinEditor.ChildrenOfType<SkinBlueprint>().First(b => b.Item is ArgonAccuracyCounter);
                skinEditor.SelectedComponents.Clear();
                skinEditor.SelectedComponents.Add(blueprint.Item);
            });
            AddStep("copy", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.C);
                InputManager.ReleaseKey(Key.LControl);
            });
            AddStep("paste", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.V);
                InputManager.ReleaseKey(Key.LControl);
            });
            AddAssert("two argon accuracy counters present",
                () => skinEditor.ChildrenOfType<SkinBlueprint>().Count(b => b.Item is ArgonAccuracyCounter),
                () => Is.EqualTo(2));
        }

        [Test]
        public void TestCopyPasteIdempotency()
        {
            string state = null!;
            AddStep("select everything", () => InputManager.Keys(PlatformAction.SelectAll));
            AddStep("dump state", () =>
            {
                state = JsonConvert.SerializeObject(skinEditor.SelectedComponents.Cast<Drawable>().Select(s => s.CreateSerialisedInfo()).ToArray());
            });
            AddStep("copy", () => InputManager.Keys(PlatformAction.Copy));
            AddStep("delete", () => InputManager.Keys(PlatformAction.Delete));
            AddStep("paste", () => InputManager.Keys(PlatformAction.Paste));
            AddAssert("pasted state equals dumped",
                () => JsonConvert.SerializeObject(skinEditor.SelectedComponents.Cast<Drawable>().Select(s => s.CreateSerialisedInfo()).ToArray()),
                () => Is.EqualTo(state));
        }

        private SkinnableContainer globalHUDTarget => Player.ChildrenOfType<SkinnableContainer>()
                                                            .Single(c => c.Lookup.Lookup == GlobalSkinnableContainers.MainHUDComponents && c.Lookup.Ruleset == null);

        private SkinnableContainer rulesetHUDTarget => Player.ChildrenOfType<SkinnableContainer>()
                                                             .Single(c => c.Lookup.Lookup == GlobalSkinnableContainers.MainHUDComponents && c.Lookup.Ruleset != null);

        [Test]
        public void TestMigrationArgon()
        {
            Live<SkinInfo> importedSkin = null!;

            AddStep("import old argon skin", () => skins.CurrentSkinInfo.Value = importedSkin = importSkinFromArchives(@"argon-layout-version-0.osk").SkinInfo);
            AddUntilStep("wait for load", () => globalHUDTarget.ComponentsLoaded && rulesetHUDTarget.ComponentsLoaded);
            AddAssert("no combo in global target", () => !globalHUDTarget.Components.OfType<ArgonComboCounter>().Any());
            AddAssert("combo placed in ruleset target", () => rulesetHUDTarget.Components.OfType<ArgonComboCounter>().Count() == 1);

            AddStep("add combo to global target", () => globalHUDTarget.Add(new ArgonComboCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2f),
            }));
            AddStep("save skin", () => skins.Save(skins.CurrentSkin.Value));

            AddStep("select another skin", () => skins.CurrentSkinInfo.SetDefault());
            AddStep("select skin again", () => skins.CurrentSkinInfo.Value = importedSkin);
            AddUntilStep("wait for load", () => globalHUDTarget.ComponentsLoaded && rulesetHUDTarget.ComponentsLoaded);
            AddAssert("combo placed in global target", () => globalHUDTarget.Components.OfType<ArgonComboCounter>().Count() == 1);
            AddAssert("combo placed in ruleset target", () => rulesetHUDTarget.Components.OfType<ArgonComboCounter>().Count() == 1);
        }

        [Test]
        public void TestMigrationTriangles()
        {
            Live<SkinInfo> importedSkin = null!;

            AddStep("import old triangles skin", () => skins.CurrentSkinInfo.Value = importedSkin = importSkinFromArchives(@"triangles-layout-version-0.osk").SkinInfo);
            AddUntilStep("wait for load", () => globalHUDTarget.ComponentsLoaded && rulesetHUDTarget.ComponentsLoaded);
            AddAssert("no combo in global target", () => !globalHUDTarget.Components.OfType<DefaultComboCounter>().Any());
            AddAssert("combo placed in ruleset target", () => rulesetHUDTarget.Components.OfType<DefaultComboCounter>().Count() == 1);

            AddStep("add combo to global target", () => globalHUDTarget.Add(new DefaultComboCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2f),
            }));
            AddStep("save skin", () => skins.Save(skins.CurrentSkin.Value));

            AddStep("select another skin", () => skins.CurrentSkinInfo.SetDefault());
            AddStep("select skin again", () => skins.CurrentSkinInfo.Value = importedSkin);
            AddUntilStep("wait for load", () => globalHUDTarget.ComponentsLoaded && rulesetHUDTarget.ComponentsLoaded);
            AddAssert("combo placed in global target", () => globalHUDTarget.Components.OfType<DefaultComboCounter>().Count() == 1);
            AddAssert("combo placed in ruleset target", () => rulesetHUDTarget.Components.OfType<DefaultComboCounter>().Count() == 1);
        }

        [Test]
        public void TestMigrationLegacy()
        {
            Live<SkinInfo> importedSkin = null!;

            AddStep("import old classic skin", () => skins.CurrentSkinInfo.Value = importedSkin = importSkinFromArchives(@"classic-layout-version-0.osk").SkinInfo);
            AddUntilStep("wait for load", () => globalHUDTarget.ComponentsLoaded && rulesetHUDTarget.ComponentsLoaded);
            AddAssert("no combo in global target", () => !globalHUDTarget.Components.OfType<LegacyDefaultComboCounter>().Any());
            AddAssert("combo placed in ruleset target", () => rulesetHUDTarget.Components.OfType<LegacyDefaultComboCounter>().Count() == 1);

            AddStep("add combo to global target", () => globalHUDTarget.Add(new LegacyDefaultComboCounter
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2f),
            }));
            AddStep("save skin", () => skins.Save(skins.CurrentSkin.Value));

            AddStep("select another skin", () => skins.CurrentSkinInfo.SetDefault());
            AddStep("select skin again", () => skins.CurrentSkinInfo.Value = importedSkin);
            AddUntilStep("wait for load", () => globalHUDTarget.ComponentsLoaded && rulesetHUDTarget.ComponentsLoaded);
            AddAssert("combo placed in global target", () => globalHUDTarget.Components.OfType<LegacyDefaultComboCounter>().Count() == 1);
            AddAssert("combo placed in ruleset target", () => rulesetHUDTarget.Components.OfType<LegacyDefaultComboCounter>().Count() == 1);
        }

        [Test]
        public void TestAnchorRadioButtonBehavior()
        {
            ISerialisableDrawable? selectedComponent = null;

            AddStep("Select first component", () =>
            {
                var blueprint = skinEditor.ChildrenOfType<SkinBlueprint>().First();
                skinEditor.SelectedComponents.Clear();
                skinEditor.SelectedComponents.Add(blueprint.Item);
                selectedComponent = blueprint.Item;
            });

            AddStep("Right-click to open context menu", () =>
            {
                if (selectedComponent != null)
                    InputManager.MoveMouseTo(((Drawable)selectedComponent).ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Right);
            });

            AddStep("Click on Anchor menu", () =>
            {
                InputManager.MoveMouseTo(getMenuItemByText("Anchor"));
                InputManager.Click(MouseButton.Left);
            });

            AddStep("Click TopLeft anchor", () =>
            {
                InputManager.MoveMouseTo(getMenuItemByText("TopLeft"));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("TopLeft item checked", () => (getMenuItemByText("TopLeft").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.True);

            AddStep("Click Centre anchor", () =>
            {
                InputManager.MoveMouseTo(getMenuItemByText("Centre"));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Centre item checked", () => (getMenuItemByText("Centre").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.True);
            AddAssert("TopLeft item unchecked", () => (getMenuItemByText("TopLeft").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.False);

            AddStep("Click Closest anchor", () =>
            {
                InputManager.MoveMouseTo(getMenuItemByText("Closest"));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Closest item checked", () => (getMenuItemByText("Closest").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.True);
            AddAssert("Centre item unchecked", () => (getMenuItemByText("Centre").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.False);

            Menu.DrawableMenuItem getMenuItemByText(string text)
                => this.ChildrenOfType<Menu.DrawableMenuItem>().First(m => m.Item.Text.ToString() == text);
        }

        [Test]
        public void TestFlip()
        {
            BigBlackBox box1 = null!;

            AddStep("add big black box to top left", () =>
            {
                var target = Player.ChildrenOfType<SkinnableContainer>().First();
                target.Add(box1 = new BigBlackBox
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                });
            });

            AddAssert("big black box position is 0,0", () => Precision.AlmostEquals(box1.Position, new Vector2()));

            AddStep("click big black box", () =>
            {
                InputManager.MoveMouseTo(box1);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("flip horizontally", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxButton>().ElementAt(2));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("big black box position is 250,0", () => Precision.AlmostEquals(box1.Position, new Vector2(250, 0)));

            AddStep("flip horizontally", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxButton>().ElementAt(2));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("big black box position is 0,0", () => Precision.AlmostEquals(box1.Position, new Vector2()));

            AddStep("flip vertically", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxButton>().ElementAt(3));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("big black box position is 0,250", () => Precision.AlmostEquals(box1.Position, new Vector2(0, 250)));

            AddStep("flip vertically", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBoxButton>().ElementAt(3));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("big black box position is 0,0", () => Precision.AlmostEquals(box1.Position, new Vector2()));
        }

        [Test]
        public void TestConstraints()
        {
            TestConstrainedBox unselectable = null!, immovable = null!, unscaleable = null!, nonrotatable = null!;

            AddStep("add constrained boxes", () =>
            {
                var target = Player.ChildrenOfType<SkinnableContainer>().First();
                target.Add(unselectable = new TestConstrainedBox
                {
                    Text = nameof(unselectable),
                    CanBeSelected = false,
                    X = 400,
                    Y = 200,
                });
                target.Add(immovable = new TestConstrainedBox
                {
                    Text = nameof(immovable),
                    CanBeMoved = false,
                    X = 800,
                    Y = 200,
                });
                target.Add(unscaleable = new TestConstrainedBox
                {
                    Text = nameof(unscaleable),
                    CanBeScaled = false,
                    X = 400,
                    Y = 600,
                });
                target.Add(nonrotatable = new TestConstrainedBox
                {
                    Text = nameof(nonrotatable),
                    CanBeRotated = false,
                    X = 800,
                    Y = 600,
                });
            });

            AddStep("click unselectable", () =>
            {
                InputManager.MoveMouseTo(unselectable);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("nothing selected", () => skinEditor.SelectedComponents, () => Is.Empty);

            AddStep("start dragging immovable", () =>
            {
                InputManager.MoveMouseTo(immovable);
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("finish drag", () =>
            {
                InputManager.MoveMouseTo(immovable.ScreenSpaceDrawQuad.Centre + new Vector2(300));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("immovable didn't move", () => immovable.Position, () => Is.EqualTo(new Vector2(800, 200)));

            AddStep("click unscaleable", () =>
            {
                InputManager.MoveMouseTo(unscaleable);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("start dragging on bottom right of selection box", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBox>().Single().ScreenSpaceDrawQuad.BottomRight);
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("attempt resize", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SelectionBox>().Single().ScreenSpaceDrawQuad.BottomRight + new Vector2(200));
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("unscaleable didn't resize", () => unscaleable.Size, () => Is.EqualTo(new Vector2(200)));
            AddAssert("unscaleable didn't scale", () => unscaleable.Scale, () => Is.EqualTo(Vector2.One));
            AddStep("attempt flip", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.H);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("unscaleable didn't scale", () => unscaleable.Scale, () => Is.EqualTo(Vector2.One));

            AddStep("click nonrotatable", () =>
            {
                InputManager.MoveMouseTo(nonrotatable);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("no rotation handles present", () => this.ChildrenOfType<SelectionBoxRotationHandle>(), () => Is.Empty);
            AddStep("attempt rotate", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Comma);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("nonrotatable didn't rotate", () => nonrotatable.Rotation, () => Is.Zero);
        }

        private Skin importSkinFromArchives(string filename)
        {
            var imported = skins.Import(new ImportTask(TestResources.OpenResource($@"Archives/{filename}"), filename)).GetResultSafely();
            return imported.PerformRead(skinInfo => skins.GetSkin(skinInfo));
        }

        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        private partial class TestSkinEditorChangeHandler : SkinEditorChangeHandler
        {
            public TestSkinEditorChangeHandler(Drawable targetScreen)
                : base(targetScreen)
            {
            }

            public byte[] GetCurrentState()
            {
                using var stream = new MemoryStream();

                WriteCurrentStateToStream(stream);
                byte[] newState = stream.ToArray();

                return newState;
            }
        }

        private partial class TestConstrainedBox : CompositeDrawable, ISerialisableDrawable
        {
            public bool UsesFixedAnchor { get; set; }

            public required LocalisableString Text { get; init; }

            public bool CanBeSelected { get; init; } = true;
            public bool CanBeMoved { get; init; } = true;
            public bool CanBeScaled { get; init; } = true;
            public bool CanBeRotated { get; init; } = true;

            [BackgroundDependencyLoader]
            private void load()
            {
                var backgroundColour = new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1);
                var foregroundColour = OsuColour.ForegroundTextColourFor(backgroundColour);

                Size = new Vector2(200);
                InternalChildren =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = Text,
                        Colour = foregroundColour,
                        Font = OsuFont.Style.Title,
                    }
                ];
            }
        }
    }
}
