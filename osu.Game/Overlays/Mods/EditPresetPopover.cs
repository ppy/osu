// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    internal partial class EditPresetPopover : OsuPopover
    {
        private readonly ModPresetPanel button;

        private readonly LabelledTextBox nameTextBox;
        private readonly LabelledTextBox descriptionTextBox;
        private readonly ShearedButton useCurrentModButton;
        private readonly ShearedButton editButton;
        private readonly FillFlowContainer scrollContent;

        private readonly ModPreset preset;

        private HashSet<Mod>? newMods;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public EditPresetPopover(ModPresetPanel modPresetPanel)
        {
            button = modPresetPanel;
            preset = button.Preset.Value;

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
                        TabbableContentContainer = this
                    },
                    descriptionTextBox = new LabelledTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Label = CommonStrings.Description,
                        TabbableContentContainer = this
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
                            useCurrentModButton = new ShearedButton
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = "Use Current Mods",
                                Action = saveCurrentMod
                            },
                            editButton = new ShearedButton
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = Resources.Localisation.Web.CommonStrings.ButtonsSave,
                                Action = editPreset
                            },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Body.BorderThickness = 3;
            Body.BorderColour = colours.Orange1;

            nameTextBox.Current.Value = preset.Name;
            descriptionTextBox.Current.Value = preset.Description;

            editButton.DarkerColour = colours.Orange1;
            editButton.LighterColour = colours.Orange0;
            editButton.TextColour = colourProvider.Background6;

            useCurrentModButton.DarkerColour = colours.Blue1;
            useCurrentModButton.LighterColour = colours.Blue0;
            useCurrentModButton.TextColour = colourProvider.Background6;

            selectedMods.BindValueChanged(_ => updateActiveState(), true);

            nameTextBox.Current.BindValueChanged(s =>
            {
                editButton.Enabled.Value = !string.IsNullOrWhiteSpace(s.NewValue);
            }, true);
        }

        private void saveCurrentMod()
        {
            newMods = selectedMods.Value.ToHashSet();
            scrollContent.Clear();
            updateActiveState();
        }

        private void updateActiveState()
        {
            scrollContent.ChildrenEnumerable = preset.Mods.Select(mod => new ModPresetRow(mod));
            useCurrentModButton.Enabled.Value = checkSelectedModsDiffersFromSaved();
        }

        private bool checkSelectedModsDiffersFromSaved()
        {
            if (!selectedMods.Value.Any())
                return false;

            if (newMods?.SetEquals(selectedMods.Value) == false)
                return true;

            return button.CheckCurrentModCanBeSave();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(nameTextBox));
        }

        private void editPreset()
        {
            button.Preset.PerformWrite(s =>
            {
                s.Name = nameTextBox.Current.Value;
                s.Description = descriptionTextBox.Current.Value;

                if (newMods != null)
                    s.Mods = newMods;
            });

            this.HidePopover();
        }
    }
}
