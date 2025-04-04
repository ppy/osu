// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
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

            AddStep("clear list", () => blueprints.Clear());

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

            AddStep("select bar hit error blueprint", () =>
            {
                var blueprint = skinEditor.ChildrenOfType<SkinBlueprint>().First(b => b.Item is BarHitErrorMeter);
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
            AddAssert("three hit error meters present",
                () => skinEditor.ChildrenOfType<SkinBlueprint>().Count(b => b.Item is BarHitErrorMeter),
                () => Is.EqualTo(3));
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

            AddStep("Right-click TopLeft anchor", () =>
            {
                InputManager.MoveMouseTo(getMenuItemByText("TopLeft"));
                InputManager.Click(MouseButton.Right);
            });

            AddAssert("TopLeft item checked", () => (getMenuItemByText("TopLeft").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.True);

            AddStep("Right-click Centre anchor", () =>
            {
                InputManager.MoveMouseTo(getMenuItemByText("Centre"));
                InputManager.Click(MouseButton.Right);
            });

            AddAssert("Centre item checked", () => (getMenuItemByText("Centre").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.True);
            AddAssert("TopLeft item unchecked", () => (getMenuItemByText("TopLeft").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.False);

            AddStep("Right-click Closest anchor", () =>
            {
                InputManager.MoveMouseTo(getMenuItemByText("Closest"));
                InputManager.Click(MouseButton.Right);
            });

            AddAssert("Closest item checked", () => (getMenuItemByText("Closest").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.True);
            AddAssert("Centre item unchecked", () => (getMenuItemByText("Centre").Item as TernaryStateRadioMenuItem)?.State.Value == TernaryState.False);

            Menu.DrawableMenuItem getMenuItemByText(string text)
                => this.ChildrenOfType<Menu.DrawableMenuItem>().First(m => m.Item.Text.ToString() == text);
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
    }
}
