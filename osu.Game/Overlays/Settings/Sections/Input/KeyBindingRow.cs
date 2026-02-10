// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Input;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class KeyBindingRow : Container, IFilterable
    {
        /// <summary>
        /// Invoked when the binding of this row is updated with a change being written.
        /// </summary>
        public KeyBindingUpdated? BindingUpdated { get; set; }

        public delegate void KeyBindingUpdated(KeyBindingRow sender, KeyBindingUpdatedEventArgs args);

        public Func<List<RealmKeyBinding>> GetAllSectionBindings { get; set; } = null!;

        /// <summary>
        /// Whether left and right mouse button clicks should be included in the edited bindings.
        /// </summary>
        public bool AllowMainMouseButtons { get; init; }

        /// <summary>
        /// The bindings to display in this row.
        /// </summary>
        public BindableList<RealmKeyBinding> KeyBindings { get; } = new BindableList<RealmKeyBinding>();

        /// <summary>
        /// The default key bindings for this row.
        /// </summary>
        public IEnumerable<KeyCombination> Defaults { get; init; } = Array.Empty<KeyCombination>();

        #region IFilterable

        private bool matchingFilter;

        public bool MatchingFilter
        {
            get => matchingFilter;
            set
            {
                matchingFilter = value;
                this.FadeTo(!matchingFilter ? 0 : 1);
            }
        }

        public bool FilteringActive { get; set; }

        public IEnumerable<LocalisableString> FilterTerms => KeyBindings.Select(b => (LocalisableString)keyCombinationProvider.GetReadableString(b.KeyCombination)).Prepend(text.Text);

        #endregion

        public readonly object Action;

        private Bindable<bool> isDefault { get; } = new BindableBool(true);

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private ReadableKeyCombinationProvider keyCombinationProvider { get; set; } = null!;

        private Container content = null!;

        private OsuSpriteText text = null!;
        private SettingsRevertToDefaultButton revertButton = null!;
        private FillFlowContainer cancelAndClearButtons = null!;
        private FillFlowContainer<KeyButton> buttons = null!;

        private KeyButton? bindTarget;

        private Sample?[]? keypressSamples;

        private const float transition_time = 150;
        private const float height = 20;
        private const float padding = 5;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            content.ReceivePositionalInputAt(screenSpacePos);

        public override bool AcceptsFocus => bindTarget == null;

        /// <summary>
        /// Creates a new <see cref="KeyBindingRow"/>.
        /// </summary>
        /// <param name="action">The action that this row contains bindings for.</param>
        public KeyBindingRow(object action)
        {
            Action = action;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, AudioManager audioManager)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_PADDING.Right };

            InternalChildren = new Drawable[]
            {
                revertButton = new SettingsRevertToDefaultButton
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Action = RestoreDefaults,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_PADDING.Left },
                    Children = new Drawable[]
                    {
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Masking = true,
                            CornerRadius = padding,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Radius = 2,
                                Colour = colourProvider.Highlight1.Opacity(0),
                                Type = EdgeEffectType.Shadow,
                                Hollow = true,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background5,
                                },
                                text = new OsuSpriteText
                                {
                                    Text = Action.GetLocalisableDescription(),
                                    Margin = new MarginPadding(1.5f * padding),
                                },
                                buttons = new FillFlowContainer<KeyButton>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Spacing = new Vector2(-6, 0),
                                },
                                cancelAndClearButtons = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(padding) { Top = height + padding * 2 },
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Alpha = 0,
                                    Spacing = new Vector2(5),
                                    Children = new Drawable[]
                                    {
                                        new RoundedButton
                                        {
                                            Text = CommonStrings.ButtonsCancel,
                                            Size = new Vector2(80, 20),
                                            Action = () => finalise(false)
                                        },
                                        new DangerousRoundedButton
                                        {
                                            Text = CommonStrings.ButtonsClear,
                                            Size = new Vector2(80, 20),
                                            Action = clear
                                        },
                                    },
                                },
                                new HoverClickSounds()
                            }
                        }
                    }
                }
            };

            KeyBindings.BindCollectionChanged((_, _) =>
            {
                Scheduler.AddOnce(updateButtons);
                updateIsDefaultValue();
            }, true);

            keypressSamples = new Sample[4];
            for (int i = 0; i < keypressSamples.Length; i++)
                keypressSamples[i] = audioManager.Samples.Get($@"Keyboard/key-press-{1 + i}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isDefault.BindValueChanged(d =>
            {
                if (d.NewValue)
                    revertButton.Hide();
                else
                    revertButton.Show();
            }, true);
        }

        public void RestoreDefaults()
        {
            int i = 0;

            foreach (var d in Defaults)
            {
                var button = buttons[i++];
                button.UpdateKeyCombination(d);

                tryPersistKeyBinding(button.KeyBinding.Value, advanceToNextBinding: false, restoringDefaults: true);
            }

            isDefault.Value = true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            content.FadeEdgeEffectTo(1, transition_time, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            content.FadeEdgeEffectTo(0, transition_time, Easing.OutQuint);

            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!HasFocus)
                return base.OnMouseDown(e);

            Debug.Assert(bindTarget != null);

            if (!bindTarget.IsHovered)
                return base.OnMouseDown(e);

            if (!AllowMainMouseButtons)
            {
                switch (e.Button)
                {
                    case MouseButton.Left:
                    case MouseButton.Right:
                        return true;
                }
            }

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState), KeyCombination.FromMouseButton(e.Button));
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            // don't do anything until the last button is released.
            if (!HasFocus || e.HasAnyButtonPressed)
            {
                base.OnMouseUp(e);
                return;
            }

            Debug.Assert(bindTarget != null);

            if (bindTarget.IsHovered)
                finalise(false);
            // prevent updating bind target before clear button's action
            else if (!cancelAndClearButtons.Any(b => b.IsHovered))
                updateBindTarget();
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (HasFocus)
            {
                Debug.Assert(bindTarget != null);

                if (bindTarget.IsHovered)
                {
                    bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState, e.ScrollDelta), KeyCombination.FromScrollDelta(e.ScrollDelta).First());
                    finalise();
                    return true;
                }
            }

            return base.OnScroll(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!HasFocus || e.Repeat)
                return false;

            Debug.Assert(bindTarget != null);

            keypressSamples?[RNG.Next(0, keypressSamples.Length)]?.Play();

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState), KeyCombination.FromKey(e.Key));
            if (!isModifier(e.Key)) finalise();

            return true;

            bool isModifier(Key k) => k < Key.F1;
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (!HasFocus)
            {
                base.OnKeyUp(e);
                return;
            }

            finalise();
        }

        protected override bool OnJoystickPress(JoystickPressEvent e)
        {
            if (!HasFocus)
                return false;

            Debug.Assert(bindTarget != null);

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState), KeyCombination.FromJoystickButton(e.Button));
            finalise();

            return true;
        }

        protected override void OnJoystickRelease(JoystickReleaseEvent e)
        {
            if (!HasFocus)
            {
                base.OnJoystickRelease(e);
                return;
            }

            finalise();
        }

        protected override bool OnMidiDown(MidiDownEvent e)
        {
            if (!HasFocus)
                return false;

            Debug.Assert(bindTarget != null);

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState), KeyCombination.FromMidiKey(e.Key));
            finalise();

            return true;
        }

        protected override void OnMidiUp(MidiUpEvent e)
        {
            if (!HasFocus)
            {
                base.OnMidiUp(e);
                return;
            }

            finalise();
        }

        protected override bool OnTabletAuxiliaryButtonPress(TabletAuxiliaryButtonPressEvent e)
        {
            if (!HasFocus)
                return false;

            Debug.Assert(bindTarget != null);

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState), KeyCombination.FromTabletAuxiliaryButton(e.Button));
            finalise();

            return true;
        }

        protected override void OnTabletAuxiliaryButtonRelease(TabletAuxiliaryButtonReleaseEvent e)
        {
            if (!HasFocus)
            {
                base.OnTabletAuxiliaryButtonRelease(e);
                return;
            }

            finalise();
        }

        protected override bool OnTabletPenButtonPress(TabletPenButtonPressEvent e)
        {
            if (!HasFocus)
                return false;

            Debug.Assert(bindTarget != null);

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState), KeyCombination.FromTabletPenButton(e.Button));
            finalise();

            return true;
        }

        protected override void OnTabletPenButtonRelease(TabletPenButtonReleaseEvent e)
        {
            if (!HasFocus)
            {
                base.OnTabletPenButtonRelease(e);
                return;
            }

            finalise();
        }

        private void updateButtons()
        {
            if (buttons.Count > KeyBindings.Count)
                buttons.RemoveRange(buttons.Skip(KeyBindings.Count).ToArray(), true);

            while (buttons.Count < KeyBindings.Count)
                buttons.Add(new KeyButton());

            foreach (var (button, binding) in buttons.Zip(KeyBindings))
                button.KeyBinding.Value = binding;
        }

        private void clear()
        {
            if (bindTarget == null)
                return;

            bindTarget.UpdateKeyCombination(InputKey.None);
            finalise(false);
        }

        private void finalise(bool advanceToNextBinding = true)
        {
            if (bindTarget != null)
            {
                updateIsDefaultValue();

                bindTarget.IsBinding = false;
                var bindingToPersist = bindTarget.KeyBinding.Value;
                Schedule(() =>
                {
                    // schedule to ensure we don't instantly get focus back on next OnMouseClick (see AcceptFocus impl.)
                    bindTarget = null;
                    tryPersistKeyBinding(bindingToPersist, advanceToNextBinding);
                });
            }

            if (HasFocus)
                GetContainingFocusManager()!.ChangeFocus(null);

            cancelAndClearButtons.FadeOut(300, Easing.OutQuint);
            cancelAndClearButtons.BypassAutoSizeAxes |= Axes.Y;
        }

        protected override void OnFocus(FocusEvent e)
        {
            content.AutoSizeDuration = 250;
            content.AutoSizeEasing = Easing.OutQuint;

            cancelAndClearButtons.FadeIn(300, Easing.OutQuint);
            cancelAndClearButtons.BypassAutoSizeAxes &= ~Axes.Y;

            updateBindTarget();
            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            finalise(false);
            base.OnFocusLost(e);
        }

        private bool isConflictingBinding(RealmKeyBinding first, RealmKeyBinding second, bool restoringDefaults)
        {
            if (first.ID == second.ID)
                return false;

            // ignore conflicts with same action bindings during revert. the assumption is that the other binding will be reverted subsequently in the same higher-level operation.
            // this happens if the bindings for an action are rebound to the same keys, but the ordering of the bindings itself is different.
            if (restoringDefaults && first.ActionInt == second.ActionInt)
                return false;

            return first.KeyCombination.Equals(second.KeyCombination);
        }

        private void tryPersistKeyBinding(RealmKeyBinding keyBinding, bool advanceToNextBinding, bool restoringDefaults = false)
        {
            List<RealmKeyBinding> bindings = GetAllSectionBindings();
            RealmKeyBinding? existingBinding = keyBinding.KeyCombination.Equals(new KeyCombination(InputKey.None))
                ? null
                : bindings.FirstOrDefault(other => isConflictingBinding(keyBinding, other, restoringDefaults));

            if (existingBinding == null)
            {
                realm.Write(r => r.Find<RealmKeyBinding>(keyBinding.ID)!.KeyCombinationString = keyBinding.KeyCombination.ToString());
                BindingUpdated?.Invoke(this, new KeyBindingUpdatedEventArgs(bindingConflictResolved: false, advanceToNextBinding));
                return;
            }

            var keyBindingBeforeUpdate = bindings.Single(other => other.ID == keyBinding.ID);

            showBindingConflictPopover(
                new KeyBindingConflictInfo(
                    new ConflictingKeyBinding(existingBinding.ID, existingBinding.GetAction(rulesets), existingBinding.KeyCombination, new KeyCombination(InputKey.None)),
                    new ConflictingKeyBinding(keyBindingBeforeUpdate.ID, Action, keyBinding.KeyCombination, keyBindingBeforeUpdate.KeyCombination)));
        }

        /// <summary>
        /// Updates the bind target to the currently hovered key button or the first if clicked anywhere else.
        /// </summary>
        private void updateBindTarget()
        {
            if (bindTarget != null) bindTarget.IsBinding = false;
            bindTarget = buttons.FirstOrDefault(b => b.IsHovered) ?? buttons.FirstOrDefault();
            if (bindTarget != null) bindTarget.IsBinding = true;
        }

        private void updateIsDefaultValue()
        {
            isDefault.Value = KeyBindings.Select(b => b.KeyCombination).SequenceEqual(Defaults);
        }
    }
}
