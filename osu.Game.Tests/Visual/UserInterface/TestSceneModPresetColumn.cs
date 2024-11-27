// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneModPresetColumn : OsuManualInputManagerTestScene
    {
        protected override bool UseFreshStoragePerRun => true;

        private Container<Drawable> content = null!;
        protected override Container<Drawable> Content => content;

        private RulesetStore rulesets = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Cached(typeof(IDialogOverlay))]
        private readonly DialogOverlay dialogOverlay = new DialogOverlay();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);

            base.Content.AddRange(new Drawable[]
            {
                new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = content = new PopoverContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(30),
                    }
                },
                dialogOverlay
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear contents", Clear);
            AddStep("reset storage", () =>
            {
                Realm.Write(realm =>
                {
                    realm.RemoveAll<ModPreset>();

                    var testPresets = createTestPresets();
                    foreach (var preset in testPresets)
                        preset.Ruleset = realm.Find<RulesetInfo>(preset.Ruleset.ShortName)!;

                    realm.Add(testPresets);
                });
            });
        }

        [Test]
        public void TestBasicOperation()
        {
            AddStep("set osu! ruleset", () => Ruleset.Value = rulesets.GetRuleset(0));
            AddStep("create content", () => Child = new ModPresetColumn
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
            AddUntilStep("3 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 3);

            AddStep("change ruleset to mania", () => Ruleset.Value = rulesets.GetRuleset(3));
            AddUntilStep("1 panel visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 1);

            AddStep("add another mania preset", () => Realm.Write(r => r.Add(new ModPreset
            {
                Name = "and another one",
                Mods = new Mod[]
                {
                    new ManiaModMirror(),
                    new ManiaModNightcore(),
                    new ManiaModHardRock()
                },
                Ruleset = r.Find<RulesetInfo>("mania")!
            })));
            AddUntilStep("2 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 2);

            AddStep("add another osu! preset", () => Realm.Write(r => r.Add(new ModPreset
            {
                Name = "hdhr",
                Mods = new Mod[]
                {
                    new OsuModHidden(),
                    new OsuModHardRock()
                },
                Ruleset = r.Find<RulesetInfo>("osu")!
            })));
            AddUntilStep("2 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 2);

            AddStep("remove mania preset", () => Realm.Write(r =>
            {
                var toRemove = r.All<ModPreset>().Single(preset => preset.Name == "Different ruleset");
                r.Remove(toRemove);
            }));
            AddUntilStep("1 panel visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 1);

            AddStep("set osu! ruleset", () => Ruleset.Value = rulesets.GetRuleset(0));
            AddUntilStep("4 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 4);
        }

        [Test]
        public void TestSoftDeleteSupport()
        {
            AddStep("set osu! ruleset", () => Ruleset.Value = rulesets.GetRuleset(0));
            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddStep("create content", () => Child = new ModPresetColumn
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
            AddUntilStep("3 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 3);

            AddStep("soft delete preset", () => Realm.Write(r =>
            {
                var toSoftDelete = r.All<ModPreset>().Single(preset => preset.Name == "AR0");
                toSoftDelete.DeletePending = true;
            }));
            AddUntilStep("2 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 2);

            AddStep("soft delete all presets", () => Realm.Write(r =>
            {
                foreach (var preset in r.All<ModPreset>())
                    preset.DeletePending = true;
            }));
            AddUntilStep("no panels visible", () => !this.ChildrenOfType<ModPresetPanel>().Any());

            AddStep("select mods from first preset", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHardRock() });

            AddStep("undelete presets", () => Realm.Write(r =>
            {
                foreach (var preset in r.All<ModPreset>())
                    preset.DeletePending = false;
            }));
            AddUntilStep("3 panels visible", () => this.ChildrenOfType<ModPresetPanel>().Count() == 3);
        }

        [Test]
        public void TestAddingFlow([Values] bool withSystemModActive)
        {
            ModPresetColumn modPresetColumn = null!;

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddStep("create content", () => Child = modPresetColumn = new ModPresetColumn
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddUntilStep("items loaded", () => modPresetColumn.IsLoaded && modPresetColumn.ItemsLoaded);
            AddAssert("add preset button disabled", () => !this.ChildrenOfType<AddPresetButton>().Single().Enabled.Value);

            AddStep("set mods", () =>
            {
                var newMods = new Mod[] { new OsuModDaycore(), new OsuModClassic() };
                if (withSystemModActive)
                    newMods = newMods.Append(new OsuModTouchDevice()).ToArray();
                SelectedMods.Value = newMods;
            });
            AddAssert("add preset button enabled", () => this.ChildrenOfType<AddPresetButton>().Single().Enabled.Value);

            AddStep("click add preset button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<AddPresetButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            OsuPopover? popover = null;
            AddUntilStep("wait for popover", () => (popover = this.ChildrenOfType<OsuPopover>().FirstOrDefault()) != null);
            AddStep("attempt preset creation", () =>
            {
                InputManager.MoveMouseTo(popover.ChildrenOfType<ShearedButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddWaitStep("wait some", 3);
            AddAssert("preset creation did not occur", () => this.ChildrenOfType<ModPresetPanel>().Count() == 3);
            AddUntilStep("popover is unchanged", () => this.ChildrenOfType<OsuPopover>().FirstOrDefault() == popover);

            AddStep("fill preset name", () => popover.ChildrenOfType<LabelledTextBox>().First().Current.Value = "new preset");
            AddStep("attempt preset creation", () =>
            {
                InputManager.MoveMouseTo(popover.ChildrenOfType<ShearedButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("popover closed", () => !this.ChildrenOfType<OsuPopover>().Any());
            AddUntilStep("preset creation occurred", () => this.ChildrenOfType<ModPresetPanel>().Count() == 4);
            AddAssert("preset has correct mods",
                () => this.ChildrenOfType<ModPresetPanel>().Single(panel => panel.Preset.Value.Name == "new preset").Preset.Value.Mods,
                () => Has.Count.EqualTo(2));

            AddStep("click add preset button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<AddPresetButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for popover", () => (popover = this.ChildrenOfType<OsuPopover>().FirstOrDefault()) != null);
            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddUntilStep("popover closed", () => !this.ChildrenOfType<OsuPopover>().Any());
        }

        [Test]
        public void TestDeleteFlow()
        {
            ModPresetColumn modPresetColumn = null!;

            AddStep("create content", () => Child = modPresetColumn = new ModPresetColumn
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddUntilStep("items loaded", () => modPresetColumn.IsLoaded && modPresetColumn.ItemsLoaded);
            AddStep("right click first panel", () =>
            {
                var panel = this.ChildrenOfType<ModPresetPanel>().First();
                InputManager.MoveMouseTo(panel);
                InputManager.Click(MouseButton.Right);
            });

            AddUntilStep("wait for context menu", () => this.ChildrenOfType<OsuContextMenu>().Any());
            AddStep("click delete", () =>
            {
                var deleteItem = this.ChildrenOfType<DrawableOsuMenuItem>().ElementAt(1);
                InputManager.MoveMouseTo(deleteItem);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for dialog", () => dialogOverlay.CurrentDialog is DeleteModPresetDialog);
            AddStep("hold confirm", () =>
            {
                var confirmButton = this.ChildrenOfType<PopupDialogDangerousButton>().Single();
                InputManager.MoveMouseTo(confirmButton);
                InputManager.PressButton(MouseButton.Left);
            });
            AddUntilStep("wait for dialog to close", () => dialogOverlay.CurrentDialog == null);
            AddStep("release mouse", () => InputManager.ReleaseButton(MouseButton.Left));
            AddUntilStep("preset deletion occurred", () => this.ChildrenOfType<ModPresetPanel>().Count() == 2);
            AddAssert("preset soft-deleted", () => Realm.Run(r => r.All<ModPreset>().Count(preset => preset.DeletePending) == 1));
        }

        [Test]
        public void TestEditPresetName()
        {
            ModPresetColumn modPresetColumn = null!;
            string presetName = null!;
            ModPresetPanel panel = null!;

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddStep("create content", () => Child = modPresetColumn = new ModPresetColumn
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddUntilStep("items loaded", () => modPresetColumn.IsLoaded && modPresetColumn.ItemsLoaded);
            AddStep("right click first panel", () =>
            {
                panel = this.ChildrenOfType<ModPresetPanel>().First();
                presetName = panel.Preset.Value.Name;
                InputManager.MoveMouseTo(panel);
                InputManager.Click(MouseButton.Right);
            });

            AddUntilStep("wait for context menu", () => this.ChildrenOfType<OsuContextMenu>().Any());
            AddStep("click edit", () =>
            {
                var editItem = this.ChildrenOfType<DrawableOsuMenuItem>().ElementAt(0);
                InputManager.MoveMouseTo(editItem);
                InputManager.Click(MouseButton.Left);
            });

            OsuPopover? popover = null;
            AddUntilStep("wait for popover", () => (popover = this.ChildrenOfType<OsuPopover>().FirstOrDefault()) != null);
            AddStep("clear preset name", () => popover.ChildrenOfType<LabelledTextBox>().First().Current.Value = "");
            AddStep("attempt preset edit", () =>
            {
                InputManager.MoveMouseTo(popover.ChildrenOfType<ShearedButton>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("preset is not changed", () => panel.Preset.Value.Name == presetName);
            AddUntilStep("popover is unchanged", () => this.ChildrenOfType<OsuPopover>().FirstOrDefault() == popover);
            AddStep("edit preset name", () => popover.ChildrenOfType<LabelledTextBox>().First().Current.Value = "something new");
            AddStep("commit changes to textbox", () => InputManager.Key(Key.Enter));
            AddStep("attempt preset edit via select binding", () => InputManager.Key(Key.Enter));
            AddUntilStep("popover closed", () => !this.ChildrenOfType<OsuPopover>().Any());
            AddAssert("preset is changed", () => panel.Preset.Value.Name != presetName);
        }

        [Test]
        public void TestEditPresetMod()
        {
            ModPresetColumn modPresetColumn = null!;
            var mods = new Mod[] { new OsuModHidden(), new OsuModHardRock() };
            List<Mod> previousMod = null!;

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddStep("create content", () => Child = modPresetColumn = new ModPresetColumn
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddUntilStep("items loaded", () => modPresetColumn.IsLoaded && modPresetColumn.ItemsLoaded);

            AddStep("right click first panel", () =>
            {
                var panel = this.ChildrenOfType<ModPresetPanel>().First();
                previousMod = panel.Preset.Value.Mods.ToList();
                InputManager.MoveMouseTo(panel);
                InputManager.Click(MouseButton.Right);
            });
            AddUntilStep("wait for context menu", () => this.ChildrenOfType<OsuContextMenu>().Any());
            AddStep("click edit", () =>
            {
                var editItem = this.ChildrenOfType<DrawableOsuMenuItem>().ElementAt(0);
                InputManager.MoveMouseTo(editItem);
                InputManager.Click(MouseButton.Left);
            });

            OsuPopover? popover = null;
            AddUntilStep("wait for popover", () => (popover = this.ChildrenOfType<OsuPopover>().FirstOrDefault()) != null);
            AddStep("click use current mods", () =>
            {
                InputManager.MoveMouseTo(popover.ChildrenOfType<ShearedButton>().ElementAt(0));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("attempt preset edit", () =>
            {
                InputManager.MoveMouseTo(popover.ChildrenOfType<ShearedButton>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("preset mod not changed", () =>
                new HashSet<Mod>(this.ChildrenOfType<ModPresetPanel>().First().Preset.Value.Mods).SetEquals(previousMod));

            AddStep("select mods", () => SelectedMods.Value = mods);
            AddStep("right click first panel", () =>
            {
                var panel = this.ChildrenOfType<ModPresetPanel>().First();
                InputManager.MoveMouseTo(panel);
                InputManager.Click(MouseButton.Right);
            });

            AddUntilStep("wait for context menu", () => this.ChildrenOfType<OsuContextMenu>().Any());
            AddStep("click edit", () =>
            {
                var editItem = this.ChildrenOfType<DrawableOsuMenuItem>().ElementAt(0);
                InputManager.MoveMouseTo(editItem);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for popover", () => (popover = this.ChildrenOfType<OsuPopover>().FirstOrDefault()) != null);
            AddStep("click use current mods", () =>
            {
                InputManager.MoveMouseTo(popover.ChildrenOfType<ShearedButton>().ElementAt(0));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("attempt preset edit", () =>
            {
                InputManager.MoveMouseTo(popover.ChildrenOfType<ShearedButton>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("preset mod is changed", () =>
                new HashSet<Mod>(this.ChildrenOfType<ModPresetPanel>().First().Preset.Value.Mods).SetEquals(mods));
        }

        [Test]
        public void TestTextFiltering()
        {
            ModPresetColumn modPresetColumn = null!;

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddStep("create content", () => Child = modPresetColumn = new ModPresetColumn
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddUntilStep("items loaded", () => modPresetColumn.IsLoaded && modPresetColumn.ItemsLoaded);

            AddStep("set osu! ruleset", () => Ruleset.Value = rulesets.GetRuleset(0));
            AddStep("set text filter", () => modPresetColumn.SearchTerm = "First");
            AddUntilStep("one panel visible", () => modPresetColumn.ChildrenOfType<ModPresetPanel>().Count(panel => panel.IsPresent), () => Is.EqualTo(1));

            AddStep("set mania ruleset", () => Ruleset.Value = rulesets.GetRuleset(3));
            AddUntilStep("no panels visible", () => modPresetColumn.ChildrenOfType<ModPresetPanel>().Count(panel => panel.IsPresent), () => Is.EqualTo(0));
        }

        private ICollection<ModPreset> createTestPresets() => new[]
        {
            new ModPreset
            {
                Name = "First preset",
                Description = "Please ignore",
                Mods = new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime()
                },
                Ruleset = rulesets.GetRuleset(0).AsNonNull()
            },
            new ModPreset
            {
                Name = "AR0",
                Description = "For good readers",
                Mods = new Mod[]
                {
                    new OsuModDifficultyAdjust
                    {
                        ApproachRate = { Value = 0 }
                    }
                },
                Ruleset = rulesets.GetRuleset(0).AsNonNull()
            },
            new ModPreset
            {
                Name = "This preset is going to have an extraordinarily long name",
                Description = "This is done so that the capability to truncate overlong texts may be demonstrated",
                Mods = new Mod[]
                {
                    new OsuModFlashlight(),
                    new OsuModSpinIn()
                },
                Ruleset = rulesets.GetRuleset(0).AsNonNull()
            },
            new ModPreset
            {
                Name = "Different ruleset",
                Description = "Just to shake things up",
                Mods = new Mod[]
                {
                    new ManiaModKey4(),
                    new ManiaModFadeIn()
                },
                Ruleset = rulesets.GetRuleset(3).AsNonNull()
            }
        };
    }
}
