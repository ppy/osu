// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes;

namespace osu.Game.Overlays.Mods
{
    class AlwaysPresentFlowContainer : FillFlowContainer
    {
        public override bool IsPresent => true;
    }

    public class ModSection : Container
    {
        private OsuSpriteText headerLabel;

        private AlwaysPresentFlowContainer buttonsContainer;
        public FillFlowContainer ButtonsContainer => buttonsContainer;

        public Action<Mod> Action;
        protected virtual Key[] ToggleKeys => new Key[] { };

        public Mod[] SelectedMods
        {
            get
            {
                List<Mod> selectedMods = new List<Mod>();

                foreach (ModButton button in Buttons)
                {
                    Mod selectedMod = button.SelectedMod;
                    if (selectedMod != null)
                        selectedMods.Add(selectedMod);
                }

                return selectedMods.ToArray();
            }
        }

        private string header;
        public string Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
                headerLabel.Text = value;
            }
        }

        private ModButton[] buttons = {};
        public ModButton[] Buttons
        {
            get
            {
                return buttons;
            }
            set
            {
                if (value == buttons) return;
                buttons = value;

                foreach (ModButton button in value)
                {
                    button.Colour = Colour;
                    button.SelectedColour = selectedColour;
                    button.Action = buttonPressed;
                }

                buttonsContainer.Add(value);
            }
        }

        private Color4 colour = Color4.White;
        new public Color4 Colour
        {
            get
            {
                return colour;
            }
            set
            {
                if (value == colour) return;
                colour = value;

                foreach (ModButton button in buttons)
                {
                    button.Colour = value;
                }
            }
        }

        private Color4 selectedColour = Color4.White;
        public Color4 SelectedColour
        {
            get
            {
                return selectedColour;
            }
            set
            {
                if (value == selectedColour) return;
                selectedColour = value;

                foreach (ModButton button in buttons)
                {
                    button.SelectedColour = value;
                }
            }
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            var index = Array.IndexOf(ToggleKeys, args.Key);
            if (index > -1 && index < Buttons.Length)
                Buttons[index].SelectNext();

            return base.OnKeyDown(state, args);
        }

        public void DeselectAll()
        {
            foreach (ModButton button in buttons)
            {
                button.Deselect();
            }
        }

        private void buttonPressed(Mod mod)
        {
            Action?.Invoke(mod);
        }

        public ModSection()
        {
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                headerLabel = new OsuSpriteText
                {
                    Origin = Anchor.TopLeft,
                    Anchor = Anchor.TopLeft,
                    Position = new Vector2(0f, 0f),
                    Font = @"Exo2.0-Bold",
                    Text = Header,
                },
                buttonsContainer = new AlwaysPresentFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Spacing = new Vector2(50f, 0f),
                    Margin = new MarginPadding
                    {
                        Top = 6,
                    },
                },
            };
        }
    }
}
