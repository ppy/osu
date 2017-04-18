// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using System;

namespace osu.Game.Overlays.Mods
{
    public abstract class ModSection : Container
    {
        private readonly OsuSpriteText headerLabel;

        public FillFlowContainer<ModButton> ButtonsContainer { get; }

        public Action<Mod> Action;
        protected abstract Key[] ToggleKeys { get; }
        public abstract ModType ModType { get; }

        public string Header
        {
            get
            {
                return headerLabel.Text;
            }
            set
            {
                headerLabel.Text = value;
            }
        }

        private ModButton[] buttons = { };
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
                    button.ButtonColour = ButtonColour;
                    button.SelectedColour = selectedColour;
                    button.Action = Action;
                }

                ButtonsContainer.Children = value;
            }
        }

        private Color4 buttonsBolour = Color4.White;
        public Color4 ButtonColour
        {
            get
            {
                return buttonsBolour;
            }
            set
            {
                if (value == buttonsBolour) return;
                buttonsBolour = value;

                foreach (ModButton button in buttons)
                {
                    button.ButtonColour = value;
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

        protected ModSection()
        {
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                headerLabel = new OsuSpriteText
                {
                    Origin = Anchor.TopLeft,
                    Anchor = Anchor.TopLeft,
                    Position = new Vector2(0f, 0f),
                    Font = @"Exo2.0-Bold"
                },
                ButtonsContainer = new FillFlowContainer<ModButton>
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Spacing = new Vector2(50f, 0f),
                    Margin = new MarginPadding
                    {
                        Top = 6,
                    },
                    AlwaysPresent = true
                },
            };
        }
    }
}
