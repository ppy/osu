// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Overlays.Mods
{
    public abstract class ModSection : Container
    {
        private readonly OsuSpriteText headerLabel;

        public FillFlowContainer<ModButtonEmpty> ButtonsContainer { get; }

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

        public IEnumerable<Mod> SelectedMods => buttons.Select(b => b.SelectedMod).Where(m => m != null);

        public IEnumerable<Mod> Mods
        {
            set
            {
                var modContainers = value.Select(m =>
                {
                    if (m == null)
                        return new ModButtonEmpty();
                    else
                        return new ModButton(m)
                        {
                            SelectedColour = selectedColour,
                            SelectionChanged = Action,
                        };
                }).ToArray();

                ButtonsContainer.Children = modContainers;
                buttons = modContainers.OfType<ModButton>().ToArray();
            }
        }

        private ModButton[] buttons = { };

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
                    button.SelectedColour = value;
            }
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            var index = Array.IndexOf(ToggleKeys, args.Key);
            if (index > -1 && index < buttons.Length)
                buttons[index].SelectNext(state.Keyboard.ShiftPressed ? -1 : 1);

            return base.OnKeyDown(state, args);
        }

        public void DeselectAll() => DeselectTypes(buttons.Select(b => b.SelectedMod?.GetType()).Where(t => t != null));

        /// <summary>
        /// Deselect one or more mods in this section.
        /// </summary>
        /// <param name="modTypes">The types of <see cref="Mod"/>s which should be deselected.</param>
        /// <param name="immediate">Set to true to bypass animations and update selections immediately.</param>
        public void DeselectTypes(IEnumerable<Type> modTypes, bool immediate = false)
        {
            int delay = 0;
            foreach (var button in buttons)
            {
                Mod selected = button.SelectedMod;
                if (selected == null) continue;
                foreach (Type type in modTypes)
                    if (type.IsInstanceOfType(selected))
                    {
                        if (immediate)
                            button.Deselect();
                        else
                            Scheduler.AddDelayed(() => button.Deselect(), delay += 50);
                    }
            }
        }

        /// <summary>
        /// Select one or more mods in this section.
        /// </summary>
        /// <param name="mods">The types of <see cref="Mod"/>s which should be deselected.</param>
        public void SelectTypes(IEnumerable<Mod> mods)
        {
            foreach (var button in buttons)
            {
                for (int i = 0; i < button.Mods.Length; i++)
                {
                    foreach (var mod in mods)
                        if (mod.GetType().IsInstanceOfType(button.Mods[i]))
                            button.SelectAt(i);
                }
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
                ButtonsContainer = new FillFlowContainer<ModButtonEmpty>
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
