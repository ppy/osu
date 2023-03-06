// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Extensions;
using osu.Game.Graphics;
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
        private readonly LabelledSwitchButton useCurrentSwitch;
        private readonly ShearedButton createButton;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        private readonly ModPreset preset;

        public EditPresetPopover(ModPresetPanel modPresetPanel)
        {
            button = modPresetPanel;
            preset = button.Preset.Value;

            Child = new FillFlowContainer
            {
                Width = 300,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(7),
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
                    useCurrentSwitch = new LabelledSwitchButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Label = "Use Current Mod select",
                    },
                    createButton = new ShearedButton
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = CommonStrings.MenuBarEdit,
                        Action = tryEditPreset
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            Body.BorderThickness = 3;
            Body.BorderColour = colours.Orange1;

            nameTextBox.Current.Value = preset.Name;
            descriptionTextBox.Current.Value = preset.Description;

            selectedMods.BindValueChanged(_ => updateMods(), true);

            createButton.DarkerColour = colours.Orange1;
            createButton.LighterColour = colours.Orange0;
            createButton.TextColour = colourProvider.Background6;
        }

        private void updateMods()
        {
            useCurrentSwitch.Current.Disabled = false;

            // disable the switch when mod is equal.
            if (button.Active.Value)
            {
                useCurrentSwitch.Current.Value = true;
                useCurrentSwitch.Current.Disabled = true;
            }
            else
            {
                useCurrentSwitch.Current.Value = false;
                useCurrentSwitch.Current.Disabled = !selectedMods.Value.Any();
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(nameTextBox));
        }

        private void tryEditPreset()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Current.Value))
            {
                Body.Shake();
                return;
            }

            button.Preset.PerformWrite(s =>
            {
                s.Name = nameTextBox.Current.Value;
                s.Description = descriptionTextBox.Current.Value;

                if (useCurrentSwitch.Current.Value)
                {
                    s.Mods = selectedMods.Value.ToArray();
                }
            });

            this.HidePopover();
        }
    }
}
