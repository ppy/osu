// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input;
using OpenTK.Graphics;
using OpenTK.Input;
using JoystickEventArgs = osu.Framework.Input.EventArgs.JoystickEventArgs;

namespace osu.Game.Overlays.KeyBinding
{
    public class KeyBindingRow : Container, IFilterable
    {
        private readonly object action;
        private readonly IEnumerable<Framework.Input.Bindings.KeyBinding> bindings;

        private const float transition_time = 150;

        private const float height = 20;

        private const float padding = 5;

        private bool matchingFilter;

        public bool MatchingFilter
        {
            get { return matchingFilter; }
            set
            {
                matchingFilter = value;
                this.FadeTo(!matchingFilter ? 0 : 1);
            }
        }

        private OsuSpriteText text;
        private OsuTextFlowContainer pressAKey;

        private FillFlowContainer<KeyButton> buttons;

        public IEnumerable<string> FilterTerms => bindings.Select(b => b.KeyCombination.ReadableString()).Prepend((string)text.Text);

        public KeyBindingRow(object action, IEnumerable<Framework.Input.Bindings.KeyBinding> bindings)
        {
            this.action = action;
            this.bindings = bindings;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = padding;
        }

        private KeyBindingStore store;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, KeyBindingStore store)
        {
            this.store = store;

            EdgeEffect = new EdgeEffectParameters
            {
                Radius = 2,
                Colour = colours.YellowDark.Opacity(0),
                Type = EdgeEffectType.Shadow,
                Hollow = true,
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                text = new OsuSpriteText
                {
                    Text = action.GetDescription(),
                    Margin = new MarginPadding(padding),
                },
                buttons = new FillFlowContainer<KeyButton>
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                },
                pressAKey = new OsuTextFlowContainer
                {
                    Text = "Press a key to change binding, Shift+Delete to delete, Escape to cancel.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding(padding),
                    Padding = new MarginPadding { Top = height },
                    Alpha = 0,
                    Colour = colours.YellowDark
                }
            };

            foreach (var b in bindings)
                buttons.Add(new KeyButton(b));
        }

        public void RestoreDefaults()
        {
            int i = 0;
            foreach (var d in Defaults)
            {
                var button = buttons[i++];
                button.UpdateKeyCombination(d);
                store.Update(button.KeyBinding);
            }
        }

        protected override bool OnHover(InputState state)
        {
            FadeEdgeEffectTo(1, transition_time, Easing.OutQuint);

            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            FadeEdgeEffectTo(0, transition_time, Easing.OutQuint);

            base.OnHoverLost(state);
        }

        public override bool AcceptsFocus => bindTarget == null;

        private KeyButton bindTarget;

        public bool AllowMainMouseButtons;

        public IEnumerable<KeyCombination> Defaults;

        private bool isModifier(Key k) => k < Key.F1;

        protected override bool OnClick(InputState state) => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (!HasFocus || !bindTarget.IsHovered)
                return base.OnMouseDown(state, args);

            if (!AllowMainMouseButtons)
            {
                switch (args.Button)
                {
                    case MouseButton.Left:
                    case MouseButton.Right:
                        return true;
                }
            }

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(state));
            return true;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            // don't do anything until the last button is released.
            if (!HasFocus || state.Mouse.Buttons.Any())
                return base.OnMouseUp(state, args);

            if (bindTarget.IsHovered)
                finalise();
            else
                updateBindTarget();
            return true;
        }

        protected override bool OnScroll(InputState state)
        {
            if (HasFocus)
            {
                if (bindTarget.IsHovered)
                {
                    bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(state, state.Mouse.ScrollDelta));
                    finalise();
                    return true;
                }
            }

            return base.OnScroll(state);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!HasFocus)
                return false;

            switch (args.Key)
            {
                case Key.Delete:
                {
                    if (state.Keyboard.ShiftPressed)
                    {
                        bindTarget.UpdateKeyCombination(InputKey.None);
                        finalise();
                        return true;
                    }

                    break;
                }
            }

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(state));
            if (!isModifier(args.Key)) finalise();

            return true;
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (!HasFocus) return base.OnKeyUp(state, args);

            finalise();
            return true;
        }

        protected override bool OnJoystickPress(InputState state, JoystickEventArgs args)
        {
            if (!HasFocus)
                return false;

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(state));
            finalise();

            return true;
        }

        protected override bool OnJoystickRelease(InputState state, JoystickEventArgs args)
        {
            if (!HasFocus)
                return base.OnJoystickRelease(state, args);

            finalise();
            return true;
        }

        private void finalise()
        {
            if (bindTarget != null)
            {
                store.Update(bindTarget.KeyBinding);

                bindTarget.IsBinding = false;
                Schedule(() =>
                {
                    // schedule to ensure we don't instantly get focus back on next OnMouseClick (see AcceptFocus impl.)
                    bindTarget = null;
                });
            }

            if (HasFocus)
                GetContainingInputManager().ChangeFocus(null);

            pressAKey.FadeOut(300, Easing.OutQuint);
            pressAKey.BypassAutoSizeAxes |= Axes.Y;
        }

        protected override void OnFocus(InputState state)
        {
            AutoSizeDuration = 500;
            AutoSizeEasing = Easing.OutQuint;

            pressAKey.FadeIn(300, Easing.OutQuint);
            pressAKey.BypassAutoSizeAxes &= ~Axes.Y;

            updateBindTarget();
            base.OnFocus(state);
        }

        protected override void OnFocusLost(InputState state)
        {
            finalise();
            base.OnFocusLost(state);
        }

        private void updateBindTarget()
        {
            if (bindTarget != null) bindTarget.IsBinding = false;
            bindTarget = buttons.FirstOrDefault(b => b.IsHovered) ?? buttons.FirstOrDefault();
            if (bindTarget != null) bindTarget.IsBinding = true;
        }

        private class KeyButton : Container
        {
            public readonly Framework.Input.Bindings.KeyBinding KeyBinding;

            private readonly Box box;
            public readonly OsuSpriteText Text;

            private Color4 hoverColour;

            private bool isBinding;

            public bool IsBinding
            {
                get { return isBinding; }
                set
                {
                    if (value == isBinding) return;
                    isBinding = value;

                    updateHoverState();
                }
            }

            public KeyButton(Framework.Input.Bindings.KeyBinding keyBinding)
            {
                KeyBinding = keyBinding;

                Margin = new MarginPadding(padding);

                // todo: use this in a meaningful way
                // var isDefault = keyBinding.Action is Enum;

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
                        Colour = Color4.Black
                    },
                    Text = new OsuSpriteText
                    {
                        Font = "Venera",
                        TextSize = 10,
                        Margin = new MarginPadding(5),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = keyBinding.KeyCombination.ReadableString(),
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoverColour = colours.YellowDark;
            }

            protected override bool OnHover(InputState state)
            {
                updateHoverState();
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                updateHoverState();
                base.OnHoverLost(state);
            }

            private void updateHoverState()
            {
                if (isBinding)
                {
                    box.FadeColour(Color4.White, transition_time, Easing.OutQuint);
                    Text.FadeColour(Color4.Black, transition_time, Easing.OutQuint);
                }
                else
                {
                    box.FadeColour(IsHovered ? hoverColour : Color4.Black, transition_time, Easing.OutQuint);
                    Text.FadeColour(IsHovered ? Color4.Black : Color4.White, transition_time, Easing.OutQuint);
                }
            }

            public void UpdateKeyCombination(KeyCombination newCombination)
            {
                KeyBinding.KeyCombination = newCombination;
                Text.Text = KeyBinding.KeyCombination.ReadableString();
            }
        }
    }
}
