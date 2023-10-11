// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    internal partial class EditPresetPopover : OsuPopover
    {
        private LabelledTextBox nameTextBox = null!;
        private LabelledTextBox descriptionTextBox = null!;
        private ShearedButton useCurrentModsButton = null!;
        private ShearedButton saveButton = null!;
        private FillFlowContainer scrollContent = null!;

        private readonly Live<ModPreset> preset;

        private HashSet<Mod> saveableMods;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public EditPresetPopover(Live<ModPreset> preset)
        {
            this.preset = preset;
            saveableMods = preset.PerformRead(p => p.Mods).ToHashSet();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Width = 300,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(7),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    nameTextBox = new LabelledTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Label = CommonStrings.Name,
                        TabbableContentContainer = this,
                        Current = { Value = preset.PerformRead(p => p.Name) },
                    },
                    descriptionTextBox = new LabelledTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Label = CommonStrings.Description,
                        TabbableContentContainer = this,
                        Current = { Value = preset.PerformRead(p => p.Description) },
                    },
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 100,
                        Padding = new MarginPadding(7),
                        Child = scrollContent = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(7),
                            Spacing = new Vector2(7),
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(7),
                        Children = new Drawable[]
                        {
                            useCurrentModsButton = new ShearedButton
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = ModSelectOverlayStrings.UseCurrentMods,
                                DarkerColour = colours.Blue1,
                                LighterColour = colours.Blue0,
                                TextColour = colourProvider.Background6,
                                Action = useCurrentMods,
                            },
                            saveButton = new ShearedButton
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = Resources.Localisation.Web.CommonStrings.ButtonsSave,
                                DarkerColour = colours.Orange1,
                                LighterColour = colours.Orange0,
                                TextColour = colourProvider.Background6,
                                Action = save,
                            },
                        }
                    }
                }
            };

            Body.BorderThickness = 3;
            Body.BorderColour = colours.Orange1;

            selectedMods.BindValueChanged(_ => updateState(), true);
            nameTextBox.Current.BindValueChanged(s =>
            {
                saveButton.Enabled.Value = !string.IsNullOrWhiteSpace(s.NewValue);
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(nameTextBox));
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Select:
                    saveButton.TriggerClick();
                    return true;
            }

            return base.OnPressed(e);
        }

        private void useCurrentMods()
        {
            saveableMods = selectedMods.Value.ToHashSet();
            updateState();
        }

        private void updateState()
        {
            scrollContent.ChildrenEnumerable = saveableMods.AsOrdered().Select(mod => new ModPresetRow(mod));
            useCurrentModsButton.Enabled.Value = checkSelectedModsDiffersFromSaved();
        }

        private bool checkSelectedModsDiffersFromSaved()
        {
            if (!selectedMods.Value.Any())
                return false;

            return !saveableMods.SetEquals(selectedMods.Value);
        }

        private void save()
        {
            preset.PerformWrite(s =>
            {
                s.Name = nameTextBox.Current.Value;
                s.Description = descriptionTextBox.Current.Value;
                s.Mods = saveableMods;
            });

            this.HidePopover();
        }
    }
}
