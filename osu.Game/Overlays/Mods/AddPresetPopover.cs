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
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    internal partial class AddPresetPopover : OsuPopover
    {
        private readonly AddPresetButton button;

        private readonly LabelledTextBox nameTextBox;
        private readonly LabelledTextBox descriptionTextBox;
        private readonly ShearedButton createButton;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public AddPresetPopover(AddPresetButton addPresetButton)
        {
            const float content_width = 300;

            button = addPresetButton;

            Child = new FillFlowContainer
            {
                Width = content_width,
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
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(7),
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            createButton = new ShearedButton(content_width)
                            {
                                // todo: for some very odd reason, this needs to be anchored to topright for the fill flow to be correctly sized to the AABB of the sheared button
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Text = ModSelectOverlayStrings.AddPreset,
                                Action = createPreset
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            Body.BorderThickness = 3;
            Body.BorderColour = colours.Orange1;

            createButton.DarkerColour = colours.Orange1;
            createButton.LighterColour = colours.Orange0;
            createButton.TextColour = colourProvider.Background6;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => GetContainingFocusManager()!.ChangeFocus(nameTextBox));

            nameTextBox.Current.BindValueChanged(s =>
            {
                createButton.Enabled.Value = !string.IsNullOrWhiteSpace(s.NewValue);
            }, true);
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Select:
                    createButton.TriggerClick();
                    return true;
            }

            return base.OnPressed(e);
        }

        private void createPreset()
        {
            realm.Write(r => r.Add(new ModPreset
            {
                Name = nameTextBox.Current.Value,
                Description = descriptionTextBox.Current.Value,
                Mods = selectedMods.Value.Where(mod => mod.Type != ModType.System).ToArray(),
                Ruleset = r.Find<RulesetInfo>(ruleset.Value.ShortName)!
            }));

            this.HidePopover();
        }

        protected override void UpdateState(ValueChangedEvent<Visibility> state)
        {
            base.UpdateState(state);
            if (state.NewValue == Visibility.Hidden)
                button.Active.Value = false;
        }
    }
}
