﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Localisation;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class TabletSettings : SettingsSubsection
    {
        public override IEnumerable<LocalisableString> FilterTerms => base.FilterTerms.Concat(new LocalisableString[] { "area" });

        public TabletAreaSelection AreaSelection { get; private set; }

        private readonly ITabletHandler tabletHandler;

        private readonly Bindable<bool> enabled = new BindableBool(true);

        private readonly Bindable<Vector2> areaOffset = new Bindable<Vector2>();
        private readonly Bindable<Vector2> areaSize = new Bindable<Vector2>();
        private readonly Bindable<Vector2> outputAreaSize = new Bindable<Vector2>();
        private readonly Bindable<Vector2> outputAreaOffset = new Bindable<Vector2>();
        private readonly IBindable<TabletInfo> tablet = new Bindable<TabletInfo>();

        private readonly BindableNumber<float> offsetX = new BindableNumber<float> { MinValue = 0, Precision = 1 };
        private readonly BindableNumber<float> offsetY = new BindableNumber<float> { MinValue = 0, Precision = 1 };

        private readonly BindableNumber<float> sizeX = new BindableNumber<float> { MinValue = 10, Precision = 1 };
        private readonly BindableNumber<float> sizeY = new BindableNumber<float> { MinValue = 10, Precision = 1 };

        private readonly BindableNumber<float> rotation = new BindableNumber<float> { MinValue = 0, MaxValue = 360, Precision = 1 };

        private readonly BindableNumber<float> pressureThreshold = new BindableNumber<float> { MinValue = 0.0f, MaxValue = 1.0f, Precision = 0.005f };

        private Bindable<ScalingMode> scalingMode = null!;
        private Bindable<float> scalingSizeX = null!;
        private Bindable<float> scalingSizeY = null!;
        private Bindable<float> scalingPositionX = new Bindable<float>();
        private Bindable<float> scalingPositionY = new Bindable<float>();

        [Resolved]
        private GameHost host { get; set; }

        /// <summary>
        /// Based on ultrawide monitor configurations, plus a bit of lenience for users which are intentionally aiming for higher horizontal velocity.
        /// </summary>
        private const float largest_feasible_aspect_ratio = 23f / 9;

        private readonly BindableNumber<float> aspectRatio = new BindableFloat(1)
        {
            MinValue = 1 / largest_feasible_aspect_ratio,
            MaxValue = largest_feasible_aspect_ratio,
            Precision = 0.01f,
        };

        private readonly BindableBool aspectLock = new BindableBool();

        private ScheduledDelegate aspectRatioApplication;

        private FillFlowContainer mainSettings;

        private FillFlowContainer noTabletMessage;

        protected override LocalisableString Header => TabletSettingsStrings.Tablet;

        public TabletSettings(ITabletHandler tabletHandler)
        {
            this.tabletHandler = tabletHandler;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, LocalisationManager localisation, OsuConfigManager osuConfig)
        {
            scalingMode = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling);
            scalingSizeX = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeX);
            scalingSizeY = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeY);
            scalingPositionX = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionX);
            scalingPositionY = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionY);

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = CommonStrings.Enabled,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Current = enabled,
                },
                noTabletMessage = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Horizontal = SettingsPanel.CONTENT_MARGINS },
                    Spacing = new Vector2(5f),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = TabletSettingsStrings.NoTabletDetected,
                        },
                        new LinkFlowContainer(cp => cp.Colour = colours.Yellow)
                        {
                            TextAnchor = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }.With(t =>
                        {
                            t.NewLine();

                            const string url = @"https://opentabletdriver.net/Wiki/FAQ/General";
                            var formattedSource = MessageFormatter.FormatText(localisation.GetLocalisedString(TabletSettingsStrings.NoTabletDetectedDescription(url)));

                            t.AddLinks(formattedSource.Text, formattedSource.Links);
                        }),
                    }
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
                        AreaSelection = new TabletAreaSelection(tabletHandler)
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 300,
                        },
                        new DangerousSettingsButton
                        {
                            Text = TabletSettingsStrings.ResetToFullArea,
                            Action = () =>
                            {
                                aspectLock.Value = false;

                                areaOffset.SetDefault();
                                areaSize.SetDefault();
                            },
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsButton
                        {
                            Text = TabletSettingsStrings.ConformToCurrentGameAspectRatio,
                            Action = () =>
                            {
                                forceAspectRatio((float)host.Window.ClientSize.Width / host.Window.ClientSize.Height);
                            },
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = TabletSettingsStrings.XOffset,
                            Current = offsetX,
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = TabletSettingsStrings.YOffset,
                            Current = offsetY,
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = TabletSettingsStrings.Rotation,
                            Current = rotation,
                            CanBeShown = { BindTarget = enabled }
                        },
                        new RotationPresetButtons(tabletHandler)
                        {
                            Padding = new MarginPadding
                            {
                                Horizontal = SettingsPanel.CONTENT_MARGINS
                            }
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = TabletSettingsStrings.AspectRatio,
                            Current = aspectRatio,
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsCheckbox
                        {
                            LabelText = TabletSettingsStrings.LockAspectRatio,
                            Current = aspectLock,
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = CommonStrings.Width,
                            Current = sizeX,
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = CommonStrings.Height,
                            Current = sizeY,
                            CanBeShown = { BindTarget = enabled }
                        },
                        new SettingsPercentageSlider<float>
                        {
                            TransferValueOnCommit = true,
                            LabelText = TabletSettingsStrings.TipPressureForClick,
                            Current = pressureThreshold,
                            CanBeShown = { BindTarget = enabled }
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            enabled.BindTo(tabletHandler.Enabled);
            enabled.BindValueChanged(_ => Scheduler.AddOnce(updateVisibility));

            rotation.BindTo(tabletHandler.Rotation);

            areaOffset.BindTo(tabletHandler.AreaOffset);
            areaOffset.BindValueChanged(val => Schedule(() =>
            {
                offsetX.Value = val.NewValue.X;
                offsetY.Value = val.NewValue.Y;
            }), true);

            offsetX.BindValueChanged(val => areaOffset.Value = new Vector2(val.NewValue, areaOffset.Value.Y));
            offsetY.BindValueChanged(val => areaOffset.Value = new Vector2(areaOffset.Value.X, val.NewValue));

            areaSize.BindTo(tabletHandler.AreaSize);
            areaSize.BindValueChanged(val => Schedule(() =>
            {
                sizeX.Value = val.NewValue.X;
                sizeY.Value = val.NewValue.Y;
            }), true);

            outputAreaSize.BindTo(tabletHandler.OutputAreaSize);
            outputAreaOffset.BindTo(tabletHandler.OutputAreaOffset);

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

            updateScaling();
            scalingMode.BindValueChanged(_ => updateScaling());
            scalingSizeX.BindValueChanged(_ => updateScaling());
            scalingSizeY.BindValueChanged(_ => updateScaling());
            scalingPositionX.BindValueChanged(_ => updateScaling());
            scalingPositionY.BindValueChanged(_ => updateScaling());

            pressureThreshold.BindTo(tabletHandler.PressureThreshold);

            tablet.BindTo(tabletHandler.Tablet);
            tablet.BindValueChanged(val => Schedule(() =>
            {
                Scheduler.AddOnce(updateVisibility);

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
                areaOffset.Default = new Vector2(offsetX.Default, offsetY.Default);
            }), true);
        }

        private void updateVisibility()
        {
            mainSettings.Hide();
            noTabletMessage.Hide();

            if (!tabletHandler.Enabled.Value)
                return;

            if (tablet.Value != null)
                mainSettings.Show();
            else
                noTabletMessage.Show();
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
                    sizeY.Value = getHeight(areaSize.Value.X, aspectRatio.Value);
                else
                    sizeX.Value = getWidth(areaSize.Value.Y, aspectRatio.Value);
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

            float proposedHeight = getHeight(sizeX.Value, aspectRatio);

            if (proposedHeight < sizeY.MaxValue)
                sizeY.Value = proposedHeight;
            else
                sizeX.Value = getWidth(sizeY.Value, aspectRatio);

            updateAspectRatio();

            aspectRatioApplication?.Cancel();
            aspectLock.Value = true;
        }

        private void updateScaling()
        {
            if (scalingMode.Value == ScalingMode.Everything)
            {
                outputAreaSize.Value = new Vector2(scalingSizeX.Value, scalingSizeY.Value);
                outputAreaOffset.Value = new Vector2(scalingPositionX.Value, scalingPositionY.Value);
            }
            else
            {
                outputAreaSize.Value = new Vector2(1, 1);
                outputAreaOffset.Value = new Vector2(0.5f, 0.5f);
            }
        }

        private void updateAspectRatio() => aspectRatio.Value = currentAspectRatio;

        private float currentAspectRatio => sizeX.Value / sizeY.Value;

        private static float getHeight(float width, float aspectRatio) => width / aspectRatio;

        private static float getWidth(float height, float aspectRatio) => height * aspectRatio;
    }
}
