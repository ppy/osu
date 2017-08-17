// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Overlays.KeyBinding
{
    internal class KeyBindingRow : Container, IFilterable
    {
        private readonly Enum action;
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
        private OsuSpriteText pressAKey;

        private FillFlowContainer<KeyButton> buttons;

        public string[] FilterTerms => new[] { text.Text }.Concat(bindings.Select(b => b.KeyCombination.ReadableString())).ToArray();

        public KeyBindingRow(Enum action, IEnumerable<Framework.Input.Bindings.KeyBinding> bindings)
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
                pressAKey = new OsuSpriteText
                {
                    Text = "Press a key to change binding, DEL to delete, ESC to cancel.",
                    Y = height,
                    Margin = new MarginPadding(padding),
                    Alpha = 0,
                    Colour = colours.YellowDark
                }
            };

            foreach (var b in bindings)
                buttons.Add(new KeyButton(b));
        }

        protected override bool OnHover(InputState state)
        {
            this.FadeEdgeEffectTo<Container>(1, transition_time, Easing.OutQuint);

            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            this.FadeEdgeEffectTo<Container>(0, transition_time, Easing.OutQuint);

            base.OnHoverLost(state);
        }

        public override bool AcceptsFocus => true;

        private KeyButton bindTarget;

        protected override void OnFocus(InputState state)
        {
            AutoSizeDuration = 500;
            AutoSizeEasing = Easing.OutQuint;

            pressAKey.FadeIn(300, Easing.OutQuint);
            pressAKey.Padding = new MarginPadding();

            base.OnFocus(state);
        }

        private bool isModifier(Key k) => k < Key.F1;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    GetContainingInputManager().ChangeFocus(null);
                    return true;
                case Key.Delete:
                    bindTarget.UpdateKeyCombination(Key.Unknown);
                    store.Update(bindTarget.KeyBinding);
                    GetContainingInputManager().ChangeFocus(null);
                    return true;
            }

            if (HasFocus)
            {
                bindTarget.UpdateKeyCombination(state.Keyboard.Keys.ToArray());
                if (!isModifier(args.Key))
                    finalise();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (HasFocus)
            {
                finalise();
                return true;
            }

            return base.OnKeyUp(state, args);
        }

        private void finalise()
        {
            store.Update(bindTarget.KeyBinding);
            GetContainingInputManager().ChangeFocus(null);
        }

        protected override void OnFocusLost(InputState state)
        {
            bindTarget.IsBinding = false;
            bindTarget = null;

            pressAKey.FadeOut(300, Easing.OutQuint);
            pressAKey.Padding = new MarginPadding { Bottom = -pressAKey.DrawHeight };
            base.OnFocusLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            if (bindTarget != null) bindTarget.IsBinding = false;
            bindTarget = buttons.FirstOrDefault(b => b.IsHovered) ?? buttons.FirstOrDefault();
            if (bindTarget != null) bindTarget.IsBinding = true;

            return bindTarget != null;
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

            public void UpdateKeyCombination(params Key[] newCombination)
            {
                KeyBinding.KeyCombination = newCombination;
                Text.Text = KeyBinding.KeyCombination.ReadableString();
            }
        }
    }
}