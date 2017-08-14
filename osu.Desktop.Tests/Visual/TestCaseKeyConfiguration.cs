// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input;
using osu.Game.Rulesets;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Desktop.Tests.Visual
{
    public class TestCaseKeyConfiguration : OsuTestCase
    {
        private readonly KeyConfiguration configuration;

        public override string Description => @"Key configuration";

        public TestCaseKeyConfiguration()
        {
            Child = configuration = new KeyConfiguration();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            configuration.Show();
        }
    }

    public class KeyConfiguration : SettingsOverlay
    {
        protected override Drawable CreateHeader() => new SettingsHeader("key configuration", "Customise your keys!");

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(RulesetStore rulesets, GlobalBindingInputManager global)
        {
            AddSection(new GlobalBindingsSection(global, "Global"));

            foreach (var ruleset in rulesets.Query<RulesetInfo>())
                AddSection(new RulesetBindingsSection(ruleset));
        }

        public KeyConfiguration()
            : base(false)
        {
        }
    }

    public class GlobalBindingsSection : KeyBindingsSection
    {
        private readonly string name;

        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Header => name;

        public GlobalBindingsSection(KeyBindingInputManager manager, string name)
        {
            this.name = name;

            Defaults = manager.DefaultMappings;
        }
    }

    public class RulesetBindingsSection : KeyBindingsSection
    {
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_nofail;
        public override string Header => Ruleset.Name;

        public RulesetBindingsSection(RulesetInfo ruleset)
        {
            Ruleset = ruleset;

            Defaults = ruleset.CreateInstance().GetDefaultKeyBindings();
        }
    }

    public abstract class KeyBindingsSection : SettingsSection
    {
        protected IEnumerable<KeyBinding> Defaults;

        protected RulesetInfo Ruleset;

        protected KeyBindingsSection()
        {
            FlowContent.Spacing = new Vector2(0, 1);
        }

        [BackgroundDependencyLoader]
        private void load(KeyBindingStore store)
        {
            var firstDefault = Defaults?.FirstOrDefault();

            if (firstDefault == null) return;

            var actionType = firstDefault.Action.GetType();

            var bindings = store.GetProcessedList(Defaults, Ruleset?.ID);

            foreach (Enum v in Enum.GetValues(actionType))
            {
                Add(new KeyBindingRow(v, bindings.Where(b => (int)b.Action == (int)(object)v)));
            }
        }
    }

    internal class KeyBindingRow : Container, IFilterable
    {
        private readonly Enum action;
        private readonly IEnumerable<KeyBinding> bindings;

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

        public KeyBindingRow(Enum action, IEnumerable<KeyBinding> bindings)
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
                    Text = "Press a key to change the binding, or ESC to cancel.",
                    Y = height,
                    Margin = new MarginPadding(padding),
                    Alpha = 0,
                    Colour = colours.YellowDark
                }
            };

            reloadBindings();
        }

        private void reloadBindings()
        {
            buttons.Clear();
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
            if (HasFocus)
            {
                if (!isModifier(args.Key))
                {
                    bindTarget.KeyBinding.KeyCombination = new KeyCombination(state.Keyboard.Keys);
                    store.Update(bindTarget.KeyBinding);
                    GetContainingInputManager().ChangeFocus(null);
                }
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override void OnFocusLost(InputState state)
        {
            bindTarget.IsBinding = false;
            bindTarget = null;
            reloadBindings();

            pressAKey.FadeOut(300, Easing.OutQuint);
            pressAKey.Padding = new MarginPadding { Bottom = -pressAKey.DrawHeight };
            base.OnFocusLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            bindTarget = buttons.FirstOrDefault(b => b.IsHovered) ?? buttons.FirstOrDefault();
            if (bindTarget != null) bindTarget.IsBinding = true;

            return bindTarget != null;
        }

        private class KeyButton : Container
        {
            public readonly KeyBinding KeyBinding;

            private readonly Box box;
            public readonly OsuSpriteText Text;

            private Color4 hoverColour;

            private bool isBinding;

            public bool IsBinding
            {
                get { return isBinding; }
                set
                {
                    isBinding = value;

                    if (value)
                    {
                        box.FadeColour(Color4.White, transition_time, Easing.OutQuint);
                        Text.FadeColour(Color4.Black, transition_time, Easing.OutQuint);

                    }
                    else
                    {
                        box.FadeColour(Color4.Black, transition_time, Easing.OutQuint);
                        Text.FadeColour(Color4.White, transition_time, Easing.OutQuint);
                    }
                }
            }

            public KeyButton(KeyBinding keyBinding)
            {
                KeyBinding = keyBinding;

                Margin = new MarginPadding(padding);

                var isDefault = keyBinding.Action is Enum;

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
                if (isBinding)
                    return false;

                box.FadeColour(hoverColour, transition_time, Easing.OutQuint);
                Text.FadeColour(Color4.Black, transition_time, Easing.OutQuint);

                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                if (isBinding)
                    return;

                box.FadeColour(Color4.Black, transition_time, Easing.OutQuint);
                Text.FadeColour(Color4.White, transition_time, Easing.OutQuint);

                base.OnHoverLost(state);
            }
        }
    }
}
