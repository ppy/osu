// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osuTK;
using osuTK.Input;
using CommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class SliderVelocityAdjustmentControl : CompositeDrawable
    {
        public Bindable<double> Current { get; } = new BindableNumber<double>(1)
        {
            Precision = 0.01,
            MinValue = 0.1,
            MaxValue = 10
        };

        public BindableList<HitObject> ObjectsToAdjust { get; } = new BindableList<HitObject>();

        public bool IsMultipleValues { get; private set; }

        private bool applyingState;

        private FormDiscreteAdjustmentControl<double> control = null!;
        private FillFlowContainer presetsFlow = null!;
        private RoundedButton addPresetButton = null!;

        private readonly BindableList<double> presets = new BindableList<double>();

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    control = new FormDiscreteAdjustmentControl<double>(0.05)
                    {
                        Caption = "Slider velocity",
                        Current = Current,
                    },
                    presetsFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Spacing = new Vector2(5),
                        Child = addPresetButton = new AddPresetButton
                        {
                            Width = 50,
                            Height = 25,
                            Text = "+",
                            Action = () => presets.Add(Current.Value),
                        }
                    }
                }
            };

            presets.BindTo(beatmap.SliderVelocityPresets);
            presetsFlow.SetLayoutPosition(addPresetButton, float.MaxValue);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            control.TextBox.Focused.BindValueChanged(focused =>
            {
                if (focused.NewValue && IsMultipleValues)
                    control.TextBox.Text = string.Empty;
            });

            beatmap.TransactionEnded += updateState;
            beatmap.BeatmapReprocessed += updateState;
            ObjectsToAdjust.BindCollectionChanged((_, _) => updateState(), true);
            presets.BindCollectionChanged((_, _) => updatePresets(), true);

            Current.BindValueChanged(val =>
            {
                applyVelocity(val.NewValue);
                updateState();
            });
        }

        private void updateState()
        {
            HashSet<double> velocities = ObjectsToAdjust.OfType<IHasSliderVelocity>().Select(point => Math.Round(point.SliderVelocityMultiplier, 2)).Distinct().ToHashSet();
            IsMultipleValues = velocities.Count > 1;

            applyingState = true;

            control.Current.Value = velocities.FirstOrDefault(defaultValue: control.Current.Value);

            control.LabelFormat = IsMultipleValues
                ? static _ => "(multiple)"
                : v => LocalisableString.Interpolate($@"{v:0.00}x");
            control.TextBox.PlaceholderText = IsMultipleValues ? "(multiple)" : string.Empty;

            foreach (var preset in presetsFlow.OfType<SliderVelocityPresetTernaryButton>())
            {
                if (velocities.Count > 0 && velocities.Contains(preset.Velocity))
                    preset.Current.Value = IsMultipleValues ? TernaryState.Indeterminate : TernaryState.True;
                else if (velocities.Count == 0 && preset.Velocity == control.Current.Value)
                    preset.Current.Value = TernaryState.True;
                else
                    preset.Current.Value = TernaryState.False;
            }

            addPresetButton.Enabled.Value = (velocities.Count == 0 && !presets.Contains(Current.Value)) || (velocities.Count == 1 && !presets.Contains(velocities.Single()));

            applyingState = false;
        }

        private void updatePresets()
        {
            var remainingPresets = presets.ToHashSet();

            foreach (var button in presetsFlow.OfType<SliderVelocityPresetTernaryButton>())
            {
                if (remainingPresets.Contains(button.Velocity))
                    remainingPresets.Remove(button.Velocity);
                else
                    button.Expire();
            }

            foreach (double preset in remainingPresets)
            {
                var presetButton = new SliderVelocityPresetTernaryButton(preset)
                {
                    Description = default,
                    OnDelete = v => presets.Remove(v),
                };
                presetButton.Current.BindValueChanged(val =>
                {
                    if (val.NewValue != TernaryState.True)
                        return;

                    if (applyingState)
                        return;

                    if (val.NewValue == TernaryState.True)
                        applyVelocity(preset);
                    else
                        updateState();
                });

                presetsFlow.Add(presetButton);
                presetsFlow.SetLayoutPosition(presetButton, (float)preset);
            }

            updateState();
        }

        private void applyVelocity(double velocity)
        {
            if (applyingState)
                return;

            if (ObjectsToAdjust.Count == 0)
            {
                Current.Value = velocity;
                return;
            }

            beatmap.BeginChange();

            foreach (var h in ObjectsToAdjust)
            {
                if (h is IHasSliderVelocity sv)
                {
                    sv.SliderVelocityMultiplier = velocity;
                    beatmap.Update(h);
                }
            }

            beatmap.EndChange();
        }

        public bool TakeFocus()
        {
            if (IsMultipleValues)
                control.TextBox.Text = string.Empty;
            return GetContainingFocusManager()!.ChangeFocus(control.TextBox);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (beatmap.IsNotNull())
            {
                beatmap.TransactionEnded -= updateState;
                beatmap.BeatmapReprocessed -= updateState;
            }

            base.Dispose(isDisposing);
        }

        internal partial class SliderVelocityPresetTernaryButton : DrawableTernaryButton, IHasContextMenu
        {
            public double Velocity { get; }
            public Action<double>? OnDelete { get; init; }

            public SliderVelocityPresetTernaryButton(double velocity)
            {
                Velocity = velocity;
                CreateIcon = () => new Container
                {
                    Child = new OsuSpriteText
                    {
                        Text = LocalisableString.Format($@"{velocity:0.00}x"),
                        Font = OsuFont.Style.Body.With(weight: FontWeight.Bold),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                };
                RelativeSizeAxes = Axes.None;
                Width = 50;
                Height = 25;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Icon.Position = Vector2.Zero;
                Icon.RelativeSizeAxes = Axes.Both;
                Icon.Size = new Vector2(1);
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if ((e.ShiftPressed && e.Button == MouseButton.Right) || e.Button == MouseButton.Middle)
                {
                    OnDelete?.Invoke(Velocity);
                    return true;
                }

                return base.OnMouseDown(e);
            }

            public MenuItem[] ContextMenuItems =>
            [
                new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => OnDelete?.Invoke(Velocity))
            ];
        }

        private partial class AddPresetButton : RoundedButton
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                Content.CornerRadius = 5;
            }
        }
    }
}
