// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class KeyBindingRow
    {
        public partial class KeyButton : Container
        {
            public Bindable<RealmKeyBinding> KeyBinding { get; } = new Bindable<RealmKeyBinding>();

            private readonly Box box;
            public readonly OsuSpriteText Text;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private ReadableKeyCombinationProvider keyCombinationProvider { get; set; } = null!;

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

            public KeyButton()
            {
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

                KeyBinding.BindValueChanged(_ =>
                {
                    if (KeyBinding.Value.IsManaged)
                        throw new ArgumentException("Key binding should not be attached as we make temporary changes", nameof(KeyBinding));

                    updateKeyCombinationText();
                });
                keyCombinationProvider.KeymapChanged += updateKeyCombinationText;
                updateKeyCombinationText();
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                updateHoverState();
                FinishTransforms(true);
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

            /// <summary>
            /// Update from a key combination, only allowing a single non-modifier key to be specified.
            /// </summary>
            /// <param name="fullState">A <see cref="KeyCombination"/> generated from the full input state.</param>
            /// <param name="triggerKey">The key which triggered this update, and should be used as the binding.</param>
            public void UpdateKeyCombination(KeyCombination fullState, InputKey triggerKey) =>
                UpdateKeyCombination(new KeyCombination(fullState.Keys.Where(KeyCombination.IsModifierKey).Append(triggerKey).ToArray()));

            public void UpdateKeyCombination(KeyCombination newCombination)
            {
                if (KeyBinding.Value.RulesetName != null && !RealmKeyBindingStore.CheckValidForGameplay(newCombination))
                    return;

                KeyBinding.Value.KeyCombination = newCombination;
                updateKeyCombinationText();
            }

            private void updateKeyCombinationText()
            {
                Scheduler.AddOnce(updateText);

                void updateText()
                {
                    LocalisableString keyCombinationString = keyCombinationProvider.GetReadableString(KeyBinding.Value.KeyCombination);
                    float alpha = 1;

                    if (LocalisableString.IsNullOrEmpty(keyCombinationString))
                    {
                        keyCombinationString = InputSettingsStrings.ActionHasNoKeyBinding;
                        alpha = 0.4f;
                    }

                    Text.Text = keyCombinationString;
                    Text.Alpha = alpha;
                }
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (keyCombinationProvider.IsNotNull())
                    keyCombinationProvider.KeymapChanged -= updateKeyCombinationText;
            }
        }
    }
}
