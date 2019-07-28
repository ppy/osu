// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

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
            get => matchingFilter;
            set
            {
                matchingFilter = value;
                this.FadeTo(!matchingFilter ? 0 : 1);
            }
        }

        public bool FilteringActive { get; set; }

        private OsuSpriteText text;
        private Drawable pressAKey;

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

            Children = new[]
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
                pressAKey = new FillFlowContainer
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

        protected override bool OnHover(HoverEvent e)
        {
            FadeEdgeEffectTo(1, transition_time, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            FadeEdgeEffectTo(0, transition_time, Easing.OutQuint);

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

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            // don't do anything until the last button is released.
            if (!HasFocus || e.HasAnyButtonPressed)
                return base.OnMouseUp(e);

            if (bindTarget.IsHovered)
                finalise();
            else
                updateBindTarget();
            return true;
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

        protected override bool OnKeyUp(KeyUpEvent e)
        {
            if (!HasFocus) return base.OnKeyUp(e);

            finalise();
            return true;
        }

        protected override bool OnJoystickPress(JoystickPressEvent e)
        {
            if (!HasFocus)
                return false;

            bindTarget.UpdateKeyCombination(KeyCombination.FromInputState(e.CurrentState));
            finalise();

            return true;
        }

        protected override bool OnJoystickRelease(JoystickReleaseEvent e)
        {
            if (!HasFocus)
                return base.OnJoystickRelease(e);

            finalise();
            return true;
        }

        private void clear()
        {
            bindTarget.UpdateKeyCombination(InputKey.None);
            finalise();
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

        protected override void OnFocus(FocusEvent e)
        {
            AutoSizeDuration = 500;
            AutoSizeEasing = Easing.OutQuint;

            pressAKey.FadeIn(300, Easing.OutQuint);
            pressAKey.BypassAutoSizeAxes &= ~Axes.Y;

            updateBindTarget();
            base.OnFocus(e);
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            finalise();
            base.OnFocusLost(e);
        }

        private void updateBindTarget()
        {
            if (bindTarget != null) bindTarget.IsBinding = false;
            bindTarget = buttons.FirstOrDefault(b => b.IsHovered) ?? buttons.FirstOrDefault();
            if (bindTarget != null) bindTarget.IsBinding = true;
        }

        private class CancelButton : TriangleButton
        {
            public CancelButton()
            {
                Text = "Cancel";
                Size = new Vector2(80, 20);
            }
        }

        private class ClearButton : TriangleButton
        {
            public ClearButton()
            {
                Text = "Clear";
                Size = new Vector2(80, 20);
            }

            protected override bool OnMouseUp(MouseUpEvent e)
            {
                base.OnMouseUp(e);

                // without this, the mouse up triggers a finalise (and deselection) of the current binding target.
                return true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Pink;

                Triangles.ColourDark = colours.PinkDark;
                Triangles.ColourLight = colours.PinkLight;
            }
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
                get => isBinding;
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
                        Font = OsuFont.Numeric.With(size: 10),
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
