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
using Humanizer;
using osu.Framework.Input.Events;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Mods
{
    public class ModSection : CompositeDrawable
    {
        private readonly Drawable header;

        public FillFlowContainer<ModButtonEmpty> ButtonsContainer { get; }

        protected IReadOnlyList<ModButton> Buttons { get; private set; } = Array.Empty<ModButton>();

        public Action<Mod> Action;

        public Key[] ToggleKeys;

        public readonly ModType ModType;

        public IEnumerable<Mod> SelectedMods => Buttons.Select(b => b.SelectedMod).Where(m => m != null);

        private CancellationTokenSource modsLoadCts;

        protected bool SelectionAnimationRunning => pendingSelectionOperations.Count > 0;

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

                    return CreateModButton(m).With(b =>
                    {
                        b.SelectionChanged = mod =>
                        {
                            ModButtonStateChanged(mod);
                            Action?.Invoke(mod);
                        };
                    });
                }).ToArray();

                modsLoadCts?.Cancel();

                if (modContainers.Length == 0)
                {
                    ModIconsLoaded = true;
                    header.Hide();
                    Hide();
                    return;
                }

                ModIconsLoaded = false;

                LoadComponentsAsync(modContainers, c =>
                {
                    ModIconsLoaded = true;
                    ButtonsContainer.ChildrenEnumerable = c;
                }, (modsLoadCts = new CancellationTokenSource()).Token);

                Buttons = modContainers.OfType<ModButton>().ToArray();

                header.FadeIn(200);
                this.FadeIn(200);
            }
        }

        protected virtual void ModButtonStateChanged(Mod mod)
        {
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed) return false;

            if (ToggleKeys != null)
            {
                var index = Array.IndexOf(ToggleKeys, e.Key);
                if (index > -1 && index < Buttons.Count)
                    Buttons[index].SelectNext(e.ShiftPressed ? -1 : 1);
            }

            return base.OnKeyDown(e);
        }

        private const double initial_multiple_selection_delay = 120;

        private double selectionDelay = initial_multiple_selection_delay;
        private double lastSelection;

        private readonly Queue<Action> pendingSelectionOperations = new Queue<Action>();

        protected override void Update()
        {
            base.Update();

            if (selectionDelay == initial_multiple_selection_delay || Time.Current - lastSelection >= selectionDelay)
            {
                if (pendingSelectionOperations.TryDequeue(out var dequeuedAction))
                {
                    dequeuedAction();

                    // each time we play an animation, we decrease the time until the next animation (to ramp the visual and audible elements).
                    selectionDelay = Math.Max(30, selectionDelay * 0.8f);
                    lastSelection = Time.Current;
                }
                else
                {
                    // reset the selection delay after all animations have been completed.
                    // this will cause the next action to be immediately performed.
                    selectionDelay = initial_multiple_selection_delay;
                }
            }
        }

        /// <summary>
        /// Selects all mods.
        /// </summary>
        public void SelectAll()
        {
            pendingSelectionOperations.Clear();

            foreach (var button in Buttons.Where(b => !b.Selected))
                pendingSelectionOperations.Enqueue(() => button.SelectAt(0));
        }

        /// <summary>
        /// Deselects all mods.
        /// </summary>
        public void DeselectAll()
        {
            pendingSelectionOperations.Clear();
            DeselectTypes(Buttons.Select(b => b.SelectedMod?.GetType()).Where(t => t != null));
        }

        /// <summary>
        /// Deselect one or more mods in this section.
        /// </summary>
        /// <param name="modTypes">The types of <see cref="Mod"/>s which should be deselected.</param>
        /// <param name="immediate">Whether the deselection should happen immediately. Should only be used when required to ensure correct selection flow.</param>
        /// <param name="newSelection">If this deselection is triggered by a user selection, this should contain the newly selected type. This type will never be deselected, even if it matches one provided in <paramref name="modTypes"/>.</param>
        public void DeselectTypes(IEnumerable<Type> modTypes, bool immediate = false, Mod newSelection = null)
        {
            foreach (var button in Buttons)
            {
                if (button.SelectedMod == null) continue;

                if (button.SelectedMod == newSelection)
                    continue;

                foreach (var type in modTypes)
                {
                    if (type.IsInstanceOfType(button.SelectedMod))
                    {
                        if (immediate)
                            button.Deselect();
                        else
                            pendingSelectionOperations.Enqueue(button.Deselect);
                    }
                }
            }
        }

        /// <summary>
        /// Updates all buttons with the given list of selected mods.
        /// </summary>
        /// <param name="newSelectedMods">The new list of selected mods to select.</param>
        public void UpdateSelectedButtons(IReadOnlyList<Mod> newSelectedMods)
        {
            foreach (var button in Buttons)
                updateButtonSelection(button, newSelectedMods);
        }

        private void updateButtonSelection(ModButton button, IReadOnlyList<Mod> newSelectedMods)
        {
            foreach (var mod in newSelectedMods)
            {
                var index = Array.FindIndex(button.Mods, m1 => mod.GetType() == m1.GetType());
                if (index < 0)
                    continue;

                var buttonMod = button.Mods[index];

                // as this is likely coming from an external change, ensure the settings of the mod are in sync.
                buttonMod.CopyFrom(mod);

                button.SelectAt(index, false);
                return;
            }

            button.Deselect();
        }

        public ModSection(ModType type)
        {
            ModType = type;

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Origin = Anchor.TopCentre;
            Anchor = Anchor.TopCentre;

            InternalChildren = new[]
            {
                header = CreateHeader(type.Humanize(LetterCasing.Title)),
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

        protected virtual Drawable CreateHeader(string text) => new OsuSpriteText
        {
            Font = OsuFont.GetFont(weight: FontWeight.Bold),
            Text = text
        };

        protected virtual ModButton CreateModButton(Mod mod) => new ModButton(mod);

        /// <summary>
        /// Play out all remaining animations immediately to leave mods in a good (final) state.
        /// </summary>
        public void FlushAnimation()
        {
            while (pendingSelectionOperations.TryDequeue(out var dequeuedAction))
                dequeuedAction();
        }
    }
}
