// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class TabletSettings : SettingsSubsection
    {
        private readonly ITabletHandler tabletHandler;

        private readonly Bindable<Vector2> areaOffset = new Bindable<Vector2>();
        private readonly Bindable<Vector2> areaSize = new Bindable<Vector2>();
        private readonly IBindable<TabletInfo> tablet = new Bindable<TabletInfo>();

        private readonly BindableNumber<float> offsetX = new BindableNumber<float> { MinValue = 0 };
        private readonly BindableNumber<float> offsetY = new BindableNumber<float> { MinValue = 0 };

        private readonly BindableNumber<float> sizeX = new BindableNumber<float> { MinValue = 10 };
        private readonly BindableNumber<float> sizeY = new BindableNumber<float> { MinValue = 10 };

        private readonly BindableNumber<float> rotation = new BindableNumber<float> { MinValue = 0, MaxValue = 360 };

        [Resolved]
        private GameHost host { get; set; }

        /// <summary>
        /// Based on ultrawide monitor configurations.
        /// </summary>
        private const float largest_feasible_aspect_ratio = 21f / 9;

        private readonly BindableNumber<float> aspectRatio = new BindableFloat(1)
        {
            MinValue = 1 / largest_feasible_aspect_ratio,
            MaxValue = largest_feasible_aspect_ratio,
            Precision = 0.01f,
        };

        private readonly BindableBool aspectLock = new BindableBool();

        private ScheduledDelegate aspectRatioApplication;

        private FillFlowContainer mainSettings;

        private OsuSpriteText noTabletMessage;

        protected override string Header => "Tablet";

        public TabletSettings(ITabletHandler tabletHandler)
        {
            this.tabletHandler = tabletHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Enabled",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Current = tabletHandler.Enabled
                },
                noTabletMessage = new OsuSpriteText
                {
                    Text = "No tablet detected!",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS }
                },
                mainSettings = new FillFlowContainer
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0, 8),
                    Direction = FillDirection.Vertical,
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
                            TransferValueOnCommit = true,
                            LabelText = "X Offset",
                            Current = offsetX
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = "Y Offset",
                            Current = offsetY
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = "Rotation",
                            Current = rotation
                        },
                        new RotationPresetButtons(tabletHandler),
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = "Aspect Ratio",
                            Current = aspectRatio
                        },
                        new SettingsCheckbox
                        {
                            LabelText = "Lock aspect ratio",
                            Current = aspectLock
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = "Width",
                            Current = sizeX
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = "Height",
                            Current = sizeY
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rotation.BindTo(tabletHandler.Rotation);

            areaOffset.BindTo(tabletHandler.AreaOffset);
            areaOffset.BindValueChanged(val =>
            {
                offsetX.Value = val.NewValue.X;
                offsetY.Value = val.NewValue.Y;
            }, true);

            offsetX.BindValueChanged(val => areaOffset.Value = new Vector2(val.NewValue, areaOffset.Value.Y));
            offsetY.BindValueChanged(val => areaOffset.Value = new Vector2(areaOffset.Value.X, val.NewValue));

            areaSize.BindTo(tabletHandler.AreaSize);
            areaSize.BindValueChanged(val =>
            {
                sizeX.Value = val.NewValue.X;
                sizeY.Value = val.NewValue.Y;
            }, true);

            sizeX.BindValueChanged(val =>
            {
                areaSize.Value = new Vector2(val.NewValue, areaSize.Value.Y);

                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => applyAspectRatio(sizeX));
            });

            sizeY.BindValueChanged(val =>
            {
                areaSize.Value = new Vector2(areaSize.Value.X, val.NewValue);

                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => applyAspectRatio(sizeY));
            });

            updateAspectRatio();
            aspectRatio.BindValueChanged(aspect =>
            {
                aspectRatioApplication?.Cancel();
                aspectRatioApplication = Schedule(() => forceAspectRatio(aspect.NewValue));
            });

            tablet.BindTo(tabletHandler.Tablet);
            tablet.BindValueChanged(val =>
            {
                Scheduler.AddOnce(toggleVisibility);

                var tab = val.NewValue;

                bool tabletFound = tab != null;
                if (!tabletFound)
                    return;

                offsetX.MaxValue = tab.Size.X;
                offsetX.Default = tab.Size.X / 2;
                sizeX.Default = sizeX.MaxValue = tab.Size.X;

                offsetY.MaxValue = tab.Size.Y;
                offsetY.Default = tab.Size.Y / 2;
                sizeY.Default = sizeY.MaxValue = tab.Size.Y;

                areaSize.Default = new Vector2(sizeX.Default, sizeY.Default);
            }, true);
        }

        private void toggleVisibility()
        {
            bool tabletFound = tablet.Value != null;

            if (!tabletFound)
            {
                mainSettings.Hide();
                noTabletMessage.Show();
                return;
            }

            mainSettings.Show();
            noTabletMessage.Hide();
        }

        private void applyAspectRatio(BindableNumber<float> sizeChanged)
        {
            try
            {
                if (!aspectLock.Value)
                {
                    float proposedAspectRatio = currentAspectRatio;

                    if (proposedAspectRatio >= aspectRatio.MinValue && proposedAspectRatio <= aspectRatio.MaxValue)
                    {
                        // aspect ratio was in a valid range.
                        updateAspectRatio();
                        return;
                    }
                }

                // if lock is applied (or the specified values were out of range) aim to adjust the axis the user was not adjusting to conform.
                if (sizeChanged == sizeX)
                    sizeY.Value = (int)(areaSize.Value.X / aspectRatio.Value);
                else
                    sizeX.Value = (int)(areaSize.Value.Y * aspectRatio.Value);
            }
            finally
            {
                // cancel any event which may have fired while updating variables as a result of aspect ratio limitations.
                // this avoids a potential feedback loop.
                aspectRatioApplication?.Cancel();
            }
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

        private void updateAspectRatio() => aspectRatio.Value = currentAspectRatio;

        private float currentAspectRatio => sizeX.Value / sizeY.Value;
    }
}
