// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
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
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class KeyBindingRow : Container, IFilterable
    {
        private readonly object action;
        private readonly IEnumerable<RealmKeyBinding> bindings;

        private const float transition_time = 150;

        private const float height = 20;

        private const float padding = 5;

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

        private Container content;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            content.ReceivePositionalInputAt(screenSpacePos);

        public bool FilteringActive { get; set; }

        [Resolved]
        private ReadableKeyCombinationProvider keyCombinationProvider { get; set; }

        private OsuSpriteText text;
        private FillFlowContainer cancelAndClearButtons;
        private FillFlowContainer<KeyButton> buttons;

        private Bindable<bool> isDefault { get; } = new BindableBool(true);

        public IEnumerable<string> FilterTerms => bindings.Select(b => keyCombinationProvider.GetReadableString(b.KeyCombination)).Prepend(text.Text.ToString());

        public KeyBindingRow(object action, List<RealmKeyBinding> bindings)
        {
            this.action = action;
            this.bindings = bindings;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [Resolved]
        private RealmAccess realm { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = SettingsPanel.CONTENT_MARGINS,
                    Child = new RestoreDefaultValueButton<bool>
                    {
                        Current = isDefault,
                        Action = RestoreDefaults,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
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
                                    Text = action.GetLocalisableDescription(),
                                    Margin = new MarginPadding(1.5f * padding),
                                },
                                buttons = new FillFlowContainer<KeyButton>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight
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
                                        new CancelButton { Action = finalise },
                                        new ClearButton { Action = clear },
                                    },
                                },
                                new HoverClickSounds()
                            }
                        }
                    }
                }
            };

            foreach (var b in bindings)
                buttons.Add(new KeyButton(b));

            updateIsDefaultValue();
        }

        public void RestoreDefaults()
        {
            int i = 0;

            foreach (var d in Defaults)
            {
                var button = buttons[i++];
                button.UpdateKeyCombination(d);

                updateStoreFromButton(button);
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

        public override bool AcceptsFocus => bindTarget == null;

        private KeyButton bindTarget;

        public bool AllowMainMouseButtons;

        public IEnumerable<KeyCombination> Defaults;

        private bool isModifier(Key k) => k < Key.F1;

        protected override bool OnClick(ClickEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!HasFocus || !bindTarget.IsHovered)
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

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState));
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

            if (bindTarget.IsHovered)
                finalise();
            // prevent updating bind target before clear button's action
            else if (!cancelAndClearButtons.Any(b => b.IsHovered))
                updateBindTarget();
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (HasFocus)
            {
                if (bindTarget.IsHovered)
                {
                    bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState, e.ScrollDelta));
                    finalise();
                    return true;
                }
            }

            return base.OnScroll(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!HasFocus)
                return false;

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState));
            if (!isModifier(e.Key)) finalise();

            return true;
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

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState));
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

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState));
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

        private void clear()
        {
            if (bindTarget == null)
                return;

            bindTarget.UpdateKeyCombination(InputKey.None);
            finalise();
        }

        private void finalise()
        {
            if (bindTarget != null)
            {
                updateStoreFromButton(bindTarget);

                updateIsDefaultValue();

                bindTarget.IsBinding = false;
                Schedule(() =>
                {
                    // schedule to ensure we don't instantly get focus back on next OnMouseClick (see AcceptFocus impl.)
                    bindTarget = null;
                });
            }

            if (HasFocus)
                GetContainingInputManager().ChangeFocus(null);

            cancelAndClearButtons.FadeOut(300, Easing.OutQuint);
            cancelAndClearButtons.BypassAutoSizeAxes |= Axes.Y;
        }

        protected override void OnFocus(FocusEvent e)
        {
            content.AutoSizeDuration = 500;
            content.AutoSizeEasing = Easing.OutQuint;

            cancelAndClearButtons.FadeIn(300, Easing.OutQuint);
            cancelAndClearButtons.BypassAutoSizeAxes &= ~Axes.Y;

            updateBindTarget();
            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            finalise();
            base.OnFocusLost(e);
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

        private void updateStoreFromButton(KeyButton button)
        {
            realm.Run(r =>
            {
                var binding = r.Find<RealmKeyBinding>(((IHasGuidPrimaryKey)button.KeyBinding).ID);
                r.Write(() => binding.KeyCombinationString = button.KeyBinding.KeyCombinationString);
            });
        }

        private void updateIsDefaultValue()
        {
            isDefault.Value = bindings.Select(b => b.KeyCombination).SequenceEqual(Defaults);
        }

        private class CancelButton : TriangleButton
        {
            public CancelButton()
            {
                Text = CommonStrings.Cancel;
                Size = new Vector2(80, 20);
            }
        }

        public class ClearButton : DangerousTriangleButton
        {
            public ClearButton()
            {
                Text = CommonStrings.Clear;
                Size = new Vector2(80, 20);
            }
        }

        public class KeyButton : Container
        {
            public readonly RealmKeyBinding KeyBinding;

            private readonly Box box;
            public readonly OsuSpriteText Text;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            [Resolved]
            private ReadableKeyCombinationProvider keyCombinationProvider { get; set; }

            private bool isBinding;

            public bool IsBinding
            {
                get => isBinding;
                set
                {
                    if (value == isBinding) return;

                    isBinding = value;

                    updateHoverState();
                }
            }

            public KeyButton(RealmKeyBinding keyBinding)
            {
                if (keyBinding.IsManaged)
                    throw new ArgumentException("Key binding should not be attached as we make temporary changes", nameof(keyBinding));

                KeyBinding = keyBinding;

                Margin = new MarginPadding(padding);

                Masking = true;
                CornerRadius = padding;

                Height = height;
                AutoSizeAxes = Axes.X;

                Children = new Drawable[]
                {
                    new Container
                    {
                        AlwaysPresent = true,
                        Width = 80,
                        Height = height,
                    },
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    Text = new OsuSpriteText
                    {
                        Font = OsuFont.Numeric.With(size: 10),
                        Margin = new MarginPadding(5),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new HoverSounds()
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                keyCombinationProvider.KeymapChanged += updateKeyCombinationText;
                updateKeyCombinationText();
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                updateHoverState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateHoverState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateHoverState();
                base.OnHoverLost(e);
            }

            private void updateHoverState()
            {
                if (isBinding)
                {
                    box.FadeColour(colourProvider.Light2, transition_time, Easing.OutQuint);
                    Text.FadeColour(Color4.Black, transition_time, Easing.OutQuint);
                }
                else
                {
                    box.FadeColour(IsHovered ? colourProvider.Light4 : colourProvider.Background6, transition_time, Easing.OutQuint);
                    Text.FadeColour(IsHovered ? Color4.Black : Color4.White, transition_time, Easing.OutQuint);
                }
            }

            public void UpdateKeyCombination(KeyCombination newCombination)
            {
                if (KeyBinding.RulesetName != null && !RealmKeyBindingStore.CheckValidForGameplay(newCombination))
                    return;

                KeyBinding.KeyCombination = newCombination;
                updateKeyCombinationText();
            }

            private void updateKeyCombinationText()
            {
                Scheduler.AddOnce(updateText);

                void updateText() => Text.Text = keyCombinationProvider.GetReadableString(KeyBinding.KeyCombination);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (keyCombinationProvider != null)
                    keyCombinationProvider.KeymapChanged -= updateKeyCombinationText;
            }
        }
    }
}
