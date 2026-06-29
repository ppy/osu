// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
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
using osuTK;
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
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 3f;
                CornerExponent = 2.5f;

                Children = new Drawable[]
                {
                    new Container
                    {
                        AlwaysPresent = true,
                        Width = 65,
                        AutoSizeAxes = Axes.Y,
                    },
                    box = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    Text = new OsuSpriteText
                    {
                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                        Spacing = new Vector2(1, 0),
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
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
                const float transition_time = 120;

                if (isBinding)
                {
                    box.FadeColour(colourProvider.Light3, transition_time, Easing.OutQuint);
                    Text.FadeColour(Color4.Black, transition_time, Easing.OutQuint);
                }
                else if (IsHovered)
                {
                    box.FadeColour(colourProvider.Light4, transition_time, Easing.OutQuint);
                    Text.FadeColour(Color4.Black, transition_time, Easing.OutQuint);
                }
                else
                {
                    box.FadeColour(colourProvider.Background5, transition_time * 2, Easing.OutQuint);
                    Text.FadeColour(colourProvider.Content1, transition_time * 2, Easing.OutQuint);
                }
            }

            /// <summary>
            /// Update from a key combination, only allowing a single non-modifier key to be specified.
            /// </summary>
            /// <param name="fullState">A <see cref="KeyCombination"/> generated from the full input state.</param>
            /// <param name="triggerKey">The key which triggered this update, and should be used as the binding.</param>
            public void UpdateKeyCombination(KeyCombination fullState, InputKey triggerKey)
            {
                var keys = fullState.Keys
                                    .Where(KeyCombination.IsModifierKey)
                                    .Append(triggerKey)
                                    .ToArray();

                // For gameplay bindings, users care about being able to use both left / right shift as different bindings.
                // For global bindings, it's better to combine both of these into a virtual key which covers both side modifiers.
                var combination = KeyBinding.Value.RulesetName == null
                    ? keys.Select(k => k.GetVirtualKey() ?? k).ToArray()
                    : keys;

                UpdateKeyCombination(new KeyCombination(combination));
            }

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

                    Text.Text = keyCombinationString.ToUpper();
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
