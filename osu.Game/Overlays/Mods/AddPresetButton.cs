// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class AddPresetButton : ShearedToggleButton, IHasPopover
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public AddPresetButton()
            : base(1)
        {
            RelativeSizeAxes = Axes.X;
            Height = ModSelectPanel.HEIGHT;

            // shear will be applied at a higher level in `ModPresetColumn`.
            Content.Shear = Vector2.Zero;
            Padding = new MarginPadding();

            Text = "+";
            TextSize = 30;
        }

        protected override void UpdateActiveState()
        {
            DarkerColour = Active.Value ? colours.Orange1 : ColourProvider.Background3;
            LighterColour = Active.Value ? colours.Orange0 : ColourProvider.Background1;
            TextColour = Active.Value ? ColourProvider.Background6 : ColourProvider.Content1;

            if (Active.Value)
                this.ShowPopover();
            else
                this.HidePopover();
        }

        public Popover GetPopover() => new AddPresetPopover(this);

        private class AddPresetPopover : OsuPopover
        {
            private readonly AddPresetButton button;

            private readonly LabelledTextBox nameTextBox;
            private readonly LabelledTextBox descriptionTextBox;
            private readonly ShearedButton createButton;

            public AddPresetPopover(AddPresetButton addPresetButton)
            {
                button = addPresetButton;

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
                            Label = "Name",
                            TabbableContentContainer = this
                        },
                        descriptionTextBox = new LabelledTextBox
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Label = "Description",
                            TabbableContentContainer = this
                        },
                        createButton = new ShearedButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Create preset",
                            Action = this.HidePopover
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

            protected override void UpdateState(ValueChangedEvent<Visibility> state)
            {
                base.UpdateState(state);
                if (state.NewValue == Visibility.Hidden)
                    button.Active.Value = false;
            }
        }
    }
}
