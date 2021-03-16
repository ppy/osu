// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Platform;
using osu.Framework.Threading;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class TabletSettings : SettingsSubsection
    {
        private readonly ITabletHandler tabletHandler;

        private readonly BindableSize areaOffset = new BindableSize();
        private readonly BindableSize areaSize = new BindableSize();
        private readonly BindableSize tabletSize = new BindableSize();

        private readonly BindableNumber<int> offsetX = new BindableNumber<int> { MinValue = 0 };
        private readonly BindableNumber<int> offsetY = new BindableNumber<int> { MinValue = 0 };

        private readonly BindableNumber<int> sizeX = new BindableNumber<int> { MinValue = 10 };
        private readonly BindableNumber<int> sizeY = new BindableNumber<int> { MinValue = 10 };

        [Resolved]
        private GameHost host { get; set; }

        /// <summary>
        /// Based on the longest available smartphone.
        /// </summary>
        private const float largest_feasible_aspect_ratio = 20f / 9;

        private readonly BindableNumber<float> aspectRatio = new BindableFloat(1)
        {
            MinValue = 1 / largest_feasible_aspect_ratio,
            MaxValue = largest_feasible_aspect_ratio,
            Precision = 0.01f,
        };

        private readonly BindableBool aspectLock = new BindableBool();

        private ScheduledDelegate aspectRatioApplication;

        protected override string Header => "Tablet";

        public TabletSettings(ITabletHandler tabletHandler)
        {
            this.tabletHandler = tabletHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            areaOffset.BindTo(tabletHandler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                offsetX.Value = val.NewValue.Width;
                offsetY.Value = val.NewValue.Height;
            }, true);

            offsetX.BindValueChanged(val => areaOffset.Value = new Size(val.NewValue, areaOffset.Value.Height));
            offsetY.BindValueChanged(val => areaOffset.Value = new Size(areaOffset.Value.Width, val.NewValue));

            areaSize.BindTo(tabletHandler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                sizeX.Value = val.NewValue.Width;
                sizeY.Value = val.NewValue.Height;

                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => applyAspectRatio(val));
            }, true);

            sizeX.BindValueChanged(val => areaSize.Value = new Size(val.NewValue, areaSize.Value.Height));
            sizeY.BindValueChanged(val => areaSize.Value = new Size(areaSize.Value.Width, val.NewValue));

            aspectRatio.BindValueChanged(aspect =>
            {
                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => forceAspectRatio(aspect.NewValue));
            });

            ((IBindable<Size>)tabletSize).BindTo(tabletHandler.TabletSize);
            tabletSize.BindValueChanged(val =>
            {
                if (tabletSize.Value == System.Drawing.Size.Empty)
                    return;

                // todo: these should propagate from a TabletChanged event or similar.
                offsetX.MaxValue = val.NewValue.Width;
                sizeX.Default = sizeX.MaxValue = val.NewValue.Width;

                offsetY.MaxValue = val.NewValue.Height;
                sizeY.Default = sizeY.MaxValue = val.NewValue.Height;

                areaSize.Default = new Size(sizeX.Default, sizeY.Default);

                updateDisplay();
            }, true);
        }

        private float curentAspectRatio => (float)sizeX.Value / sizeY.Value;

        private void applyAspectRatio(ValueChangedEvent<Size> sizeChanged)
        {
            float proposedAspectRatio = curentAspectRatio;

            try
            {
                if (!aspectLock.Value)
                {
                    // aspect ratio was in a valid range.
                    if (proposedAspectRatio >= aspectRatio.MinValue && proposedAspectRatio <= aspectRatio.MaxValue)
                    {
                        updateAspectRatio();
                        return;
                    }
                }

                if (sizeChanged.NewValue.Width != sizeChanged.OldValue.Width)
                {
                    areaSize.Value = new Size(areaSize.Value.Width, (int)(areaSize.Value.Width / aspectRatio.Value));
                }
                else
                {
                    areaSize.Value = new Size((int)(areaSize.Value.Height * aspectRatio.Value), areaSize.Value.Height);
                }
            }
            finally
            {
                // cancel any event which may have fired while updating variables as a result of aspect ratio limitations.
                // this avoids a potential feedback loop.
                aspectRatioApplication?.Cancel();
            }
        }

        private void updateAspectRatio()
        {
            aspectRatio.Value = curentAspectRatio;
        }

        private void updateDisplay()
        {
            if (Children.Count > 0)
                return;

            Children = new Drawable[]
            {
                new TabletAreaSelection(tabletHandler)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 300,
                },
                new DangerousSettingsButton
                {
                    Text = "Reset to full area",
                    Action = () =>
                    {
                        aspectLock.Value = false;

                        areaOffset.SetDefault();
                        areaSize.SetDefault();
                    },
                },
                new SettingsButton
                {
                    Text = "Conform to current game aspect ratio",
                    Action = () =>
                    {
                        forceAspectRatio((float)host.Window.ClientSize.Width / host.Window.ClientSize.Height);
                    }
                },
                new SettingsSlider<float>
                {
                    LabelText = "Aspect Ratio",
                    Current = aspectRatio
                },
                new SettingsSlider<int>
                {
                    LabelText = "X Offset",
                    Current = offsetX
                },
                new SettingsSlider<int>
                {
                    LabelText = "Y Offset",
                    Current = offsetY
                },
                new SettingsCheckbox
                {
                    LabelText = "Lock aspect ratio",
                    Current = aspectLock
                },
                new SettingsSlider<int>
                {
                    LabelText = "Width",
                    Current = sizeX
                },
                new SettingsSlider<int>
                {
                    LabelText = "Height",
                    Current = sizeY
                },
            };
        }

        private void forceAspectRatio(float aspectRatio)
        {
            aspectLock.Value = false;

            int proposedHeight = (int)(sizeX.Value / aspectRatio);

            if (proposedHeight < sizeY.MaxValue)
                sizeY.Value = proposedHeight;
            else
                sizeX.Value = (int)(sizeY.Value * aspectRatio);

            updateAspectRatio();

            aspectRatioApplication?.Cancel();
            aspectLock.Value = true;
        }
    }
}
