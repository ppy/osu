// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Mods.Input;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public partial class ModColumn : ModSelectColumn
    {
        public readonly ModType ModType;

        private IReadOnlyList<ModState> availableMods = Array.Empty<ModState>();

        /// <summary>
        /// Sets the list of mods to show in this column.
        /// </summary>
        public IReadOnlyList<ModState> AvailableMods
        {
            get => availableMods;
            set
            {
                Debug.Assert(value.All(mod => mod.Mod.Type == ModType));

                availableMods = value;

                foreach (var mod in availableMods)
                {
                    mod.Active.BindValueChanged(_ => updateState());
                    mod.MatchingTextFilter.BindValueChanged(_ => updateState());
                    mod.ValidForSelection.BindValueChanged(_ => updateState());
                }

                updateState();

                if (IsLoaded)
                    asyncLoadPanels();
            }
        }

        protected virtual ModPanel CreateModPanel(ModState mod) => new ModPanel(mod);

        private readonly bool allowIncompatibleSelection;

        private readonly ToggleAllCheckbox? toggleAllCheckbox;

        private Bindable<ModSelectHotkeyStyle> hotkeyStyle = null!;
        private IModHotkeyHandler hotkeyHandler = null!;

        private Task? latestLoadTask;
        private ICollection<ModPanel>? latestLoadedPanels;
        internal bool ItemsLoaded => latestLoadTask?.IsCompleted == true && latestLoadedPanels?.All(panel => panel.Parent != null) == true;

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        public ModColumn(ModType modType, bool allowIncompatibleSelection)
        {
            ModType = modType;
            this.allowIncompatibleSelection = allowIncompatibleSelection;

            HeaderText = ModType.Humanize(LetterCasing.Title);

            if (allowIncompatibleSelection)
            {
                ControlContainer.Height = 35;
                ControlContainer.Add(toggleAllCheckbox = new ToggleAllCheckbox(this)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Scale = new Vector2(0.8f),
                    RelativeSizeAxes = Axes.X,
                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0)
                });
                ItemsFlow.Padding = new MarginPadding
                {
                    Top = 0,
                    Bottom = 7,
                    Horizontal = 7
                };
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager configManager)
        {
            AccentColour = colours.ForModType(ModType);

            if (toggleAllCheckbox != null)
            {
                toggleAllCheckbox.AccentColour = AccentColour;
                toggleAllCheckbox.AccentHoverColour = AccentColour.Lighten(0.3f);
            }

            hotkeyStyle = configManager.GetBindable<ModSelectHotkeyStyle>(OsuSetting.ModSelectHotkeyStyle);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            toggleAllCheckbox?.Current.BindValueChanged(_ => updateToggleAllText(), true);
            hotkeyStyle.BindValueChanged(val => hotkeyHandler = createHotkeyHandler(val.NewValue), true);
            asyncLoadPanels();
        }

        private void updateToggleAllText()
        {
            Debug.Assert(toggleAllCheckbox != null);
            toggleAllCheckbox.LabelText = toggleAllCheckbox.Current.Value ? CommonStrings.DeselectAll : CommonStrings.SelectAll;
        }

        private CancellationTokenSource? cancellationTokenSource;

        private void asyncLoadPanels()
        {
            cancellationTokenSource?.Cancel();

            var panels = availableMods.Select(mod => CreateModPanel(mod).With(panel => panel.Shear = Vector2.Zero)).ToArray();
            latestLoadedPanels = panels;

            latestLoadTask = LoadComponentsAsync(panels, loaded =>
            {
                ItemsFlow.ChildrenEnumerable = loaded;
                updateState();
            }, (cancellationTokenSource = new CancellationTokenSource()).Token);
        }

        private void updateState()
        {
            Alpha = availableMods.All(mod => !mod.Visible) ? 0 : 1;

            if (toggleAllCheckbox != null && !SelectionAnimationRunning)
            {
                bool anyPanelsVisible = availableMods.Any(panel => panel.Visible);

                toggleAllCheckbox.Alpha = anyPanelsVisible ? 1 : 0;

                // checking `anyPanelsVisible` is important since `.All()` returns `true` for empty enumerables.
                if (anyPanelsVisible)
                    toggleAllCheckbox.Current.Value = availableMods.Where(panel => panel.Visible).All(panel => panel.Active.Value);
            }
        }

        #region Bulk select / deselect

        private const double initial_multiple_selection_delay = 120;

        private double selectionDelay = initial_multiple_selection_delay;
        private double lastSelection;

        private readonly Queue<Action> pendingSelectionOperations = new Queue<Action>();

        internal bool SelectionAnimationRunning => pendingSelectionOperations.Count > 0;

        protected override void Update()
        {
            base.Update();

            if (selectionDelay == initial_multiple_selection_delay || Time.Current - lastSelection >= selectionDelay)
            {
                if (pendingSelectionOperations.TryDequeue(out var dequeuedAction))
                {
                    dequeuedAction();

                    // each time we play an animation, we decrease the time until the next animation (to ramp the visual and audible elements).
                    selectionDelay = Math.Max(ModSelectPanel.SAMPLE_PLAYBACK_DELAY, selectionDelay * 0.8f);
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

            foreach (var button in availableMods.Where(b => !b.Active.Value && b.Visible))
                pendingSelectionOperations.Enqueue(() => button.Active.Value = true);
        }

        /// <summary>
        /// Deselects all mods.
        /// </summary>
        public void DeselectAll()
        {
            pendingSelectionOperations.Clear();

            foreach (var button in availableMods.Where(b => b.Active.Value))
            {
                if (!button.Visible)
                    button.Active.Value = false;
                else
                    pendingSelectionOperations.Enqueue(() => button.Active.Value = false);
            }
        }

        /// <summary>
        /// Run any delayed selections (due to animation) immediately to leave mods in a good (final) state.
        /// </summary>
        public void FlushPendingSelections()
        {
            while (pendingSelectionOperations.TryDequeue(out var dequeuedAction))
                dequeuedAction();
        }

        private partial class ToggleAllCheckbox : OsuCheckbox
        {
            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;
                    updateState();
                }
            }

            private Color4 accentHoverColour;

            public Color4 AccentHoverColour
            {
                get => accentHoverColour;
                set
                {
                    accentHoverColour = value;
                    updateState();
                }
            }

            private readonly ModColumn column;

            public ToggleAllCheckbox(ModColumn column)
                : base(false)
            {
                this.column = column;
            }

            protected override void ApplyLabelParameters(SpriteText text)
            {
                base.ApplyLabelParameters(text);
                text.Font = text.Font.With(weight: FontWeight.SemiBold);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                updateState();
            }

            private void updateState()
            {
                Nub.AccentColour = AccentColour;
                Nub.GlowingAccentColour = AccentHoverColour;
                Nub.GlowColour = AccentHoverColour.Opacity(0.2f);
            }

            protected override void OnUserChange(bool value)
            {
                if (value)
                    column.SelectAll();
                else
                    column.DeselectAll();
            }
        }

        #endregion

        #region Keyboard selection support

        /// <summary>
        /// Creates an appropriate <see cref="IModHotkeyHandler"/> for this column's <see cref="ModType"/> and
        /// the supplied <paramref name="hotkeyStyle"/>.
        /// </summary>
        private IModHotkeyHandler createHotkeyHandler(ModSelectHotkeyStyle hotkeyStyle)
        {
            switch (ModType)
            {
                case ModType.DifficultyReduction:
                case ModType.DifficultyIncrease:
                case ModType.Automation:
                    return hotkeyStyle == ModSelectHotkeyStyle.Sequential
                        ? SequentialModHotkeyHandler.Create(ModType)
                        : new ClassicModHotkeyHandler(allowIncompatibleSelection);

                default:
                    return new NoopModHotkeyHandler();
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed || e.AltPressed || e.SuperPressed || e.Repeat)
                return false;

            return hotkeyHandler.HandleHotkeyPressed(e, availableMods);
        }

        #endregion
    }
}
