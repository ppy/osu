// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using osu.Framework.Input.Events;
using osu.Game.Graphics;

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
            get => headerLabel.Text;
            set => headerLabel.Text = value;
        }

        public IEnumerable<Mod> SelectedMods => buttons.Select(b => b.SelectedMod).Where(m => m != null);

        private CancellationTokenSource modsLoadCts;

        /// <summary>
        /// True when all mod icons have completed loading.
        /// </summary>
        public bool ModIconsLoaded { get; private set; } = true;

        public IEnumerable<Mod> Mods
        {
            set
            {
                var modContainers = value.Select(m =>
                {
                    if (m == null)
                        return new ModButtonEmpty();

                    return new ModButton(m)
                    {
                        SelectionChanged = Action,
                    };
                }).ToArray();

                modsLoadCts?.Cancel();

                if (modContainers.Length == 0)
                {
                    ModIconsLoaded = true;
                    headerLabel.Hide();
                    Hide();
                    return;
                }

                ModIconsLoaded = false;

                LoadComponentsAsync(modContainers, c =>
                {
                    ModIconsLoaded = true;
                    ButtonsContainer.ChildrenEnumerable = c;
                }, (modsLoadCts = new CancellationTokenSource()).Token);

                buttons = modContainers.OfType<ModButton>().ToArray();

                headerLabel.FadeIn(200);
                this.FadeIn(200);
            }
        }

        private ModButton[] buttons = Array.Empty<ModButton>();

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed) return false;

            if (ToggleKeys != null)
            {
                var index = Array.IndexOf(ToggleKeys, e.Key);
                if (index > -1 && index < buttons.Length)
                    buttons[index].SelectNext(e.ShiftPressed ? -1 : 1);
            }

            return base.OnKeyDown(e);
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

                foreach (var type in modTypes)
                {
                    if (type.IsInstanceOfType(selected))
                    {
                        if (immediate)
                            button.Deselect();
                        else
                            Scheduler.AddDelayed(button.Deselect, delay += 50);
                    }
                }
            }
        }

        /// <summary>
        /// Updates all buttons with the given list of selected mods.
        /// </summary>
        /// <param name="newSelectedMods">The new list of selected mods to select.</param>
        public void UpdateSelectedMods(IReadOnlyList<Mod> newSelectedMods)
        {
            foreach (var button in buttons)
                updateButtonMods(button, newSelectedMods);
        }

        private void updateButtonMods(ModButton button, IReadOnlyList<Mod> newSelectedMods)
        {
            foreach (var mod in newSelectedMods)
            {
                var index = Array.FindIndex(button.Mods, m1 => mod.GetType() == m1.GetType());
                if (index < 0)
                    continue;

                var buttonMod = button.Mods[index];
                buttonMod.CopyFrom(mod);
                button.SelectAt(index);
                return;
            }

            button.Deselect();
        }

        protected ModSection()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Origin = Anchor.TopCentre;
            Anchor = Anchor.TopCentre;

            Children = new Drawable[]
            {
                headerLabel = new OsuSpriteText
                {
                    Origin = Anchor.TopLeft,
                    Anchor = Anchor.TopLeft,
                    Position = new Vector2(0f, 0f),
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
                ButtonsContainer = new FillFlowContainer<ModButtonEmpty>
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Spacing = new Vector2(50f, 0f),
                    Margin = new MarginPadding
                    {
                        Top = 20,
                    },
                    AlwaysPresent = true
                },
            };
        }
    }
}
