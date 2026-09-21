// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class LayoutSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.LayoutHeader;

        private FillFlowContainer<SettingsItemV2> scalingSettings = null!;

        private readonly Bindable<Display> currentDisplay = new Bindable<Display>();

        private Bindable<ScalingMode> scalingMode = null!;
        private Bindable<Size> sizeFullscreen = null!;
        private Bindable<Size> sizeWindowed = null!;

        private readonly BindableList<Size> resolutionsFullscreen = new BindableList<Size>(new[] { new Size(9999, 9999) });
        private readonly BindableList<Size> resolutionsWindowed = new BindableList<Size>();
        private readonly Bindable<Size> windowedResolution = new Bindable<Size>();
        private readonly IBindable<FullscreenCapability> fullscreenCapability = new Bindable<FullscreenCapability>(FullscreenCapability.Capable);

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private GameHost host { get; set; } = null!;

        private IWindow? window;

        private readonly BindableBool resolutionFullscreenCanBeShown = new BindableBool(true);
        private readonly BindableBool resolutionWindowedCanBeShown = new BindableBool(true);
        private readonly BindableBool displayDropdownCanBeShown = new BindableBool(true);
        private readonly BindableBool minimiseOnFocusLossCanBeShown = new BindableBool(true);
        private readonly BindableBool safeAreaConsiderationsCanBeShown = new BindableBool(true);

        private FormDropdown<Size> resolutionWindowedDropdown = null!;
        private FormDropdown<Display> displayDropdown = null!;
        private FormDropdown<WindowMode> windowModeDropdown = null!;

        private FormSliderBar<float> dimSlider = null!;

        private readonly Bindable<SettingsNote.Data?> windowModeDropdownNote = new Bindable<SettingsNote.Data?>();

        private Bindable<double> windowedPositionX = null!;
        private Bindable<double> windowedPositionY = null!;
        private Bindable<float> scalingPositionX = null!;
        private Bindable<float> scalingPositionY = null!;
        private Bindable<float> scalingSizeX = null!;
        private Bindable<float> scalingSizeY = null!;

        private Bindable<float> scalingBackgroundDim = null!;

        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, GameHost host)
        {
            window = host.Window;

            scalingMode = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);
            sizeWindowed = config.GetBindable<Size>(FrameworkSetting.WindowedSize);
            windowedPositionX = config.GetBindable<double>(FrameworkSetting.WindowedPositionX);
            windowedPositionY = config.GetBindable<double>(FrameworkSetting.WindowedPositionY);
            scalingSizeX = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeX);
            scalingSizeY = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeY);
            scalingPositionX = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionX);
            scalingPositionY = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionY);
            scalingBackgroundDim = osuConfig.GetBindable<float>(OsuSetting.ScalingBackgroundDim);

            windowedResolution.Value = sizeWindowed.Value;

            if (window != null)
            {
                currentDisplay.BindTo(window.CurrentDisplayBindable);
                window.DisplaysChanged += onDisplaysChanged;
            }

            if (host.Renderer is IWindowsRenderer windowsRenderer)
                fullscreenCapability.BindTo(windowsRenderer.FullscreenCapability);

            Children = new Drawable[]
            {
                new SettingsItemV2(windowModeDropdown = new FormDropdown<WindowMode>
                {
                    Caption = GraphicsSettingsStrings.ScreenMode,
                    Items = window?.SupportedWindowModes,
                    Current = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                })
                {
                    CanBeShown = { Value = window?.SupportedWindowModes.Count() > 1 },
                    Note = { BindTarget = windowModeDropdownNote },
                },
                new SettingsItemV2(displayDropdown = new DisplayDropdown
                {
                    Caption = GraphicsSettingsStrings.Display,
                    Items = window?.Displays,
                    Current = currentDisplay,
                })
                {
                    CanBeShown = { BindTarget = displayDropdownCanBeShown }
                },
                new SettingsItemV2(new ResolutionDropdown
                {
                    Caption = GraphicsSettingsStrings.Resolution,
                    ItemSource = resolutionsFullscreen,
                    Current = sizeFullscreen
                })
                {
                    CanBeShown = { BindTarget = resolutionFullscreenCanBeShown },
                    ShowRevertToDefaultButton = false,
                },
                new SettingsItemV2(resolutionWindowedDropdown = new ResolutionDropdown
                {
                    Caption = GraphicsSettingsStrings.Resolution,
                    ItemSource = resolutionsWindowed,
                    Current = windowedResolution
                })
                {
                    CanBeShown = { BindTarget = resolutionWindowedCanBeShown },
                    ShowRevertToDefaultButton = false,
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GraphicsSettingsStrings.MinimiseOnFocusLoss,
                    Current = config.GetBindable<bool>(FrameworkSetting.MinimiseOnFocusLossInFullscreen),
                })
                {
                    CanBeShown = { BindTarget = minimiseOnFocusLossCanBeShown },
                    Keywords = new[] { "alt-tab", "minimize", "focus", "hide" },
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GraphicsSettingsStrings.ShrinkGameToSafeArea,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.SafeAreaConsiderations),
                })
                {
                    CanBeShown = { BindTarget = safeAreaConsiderationsCanBeShown },
                },
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = GraphicsSettingsStrings.UIScaling,
                    TransferValueOnCommit = true,
                    Current = osuConfig.GetBindable<float>(OsuSetting.UIScale),
                    KeyboardStep = 0.01f,
                    LabelFormat = v => $@"{v:0.##}x",
                })
                {
                    Keywords = new[] { "scale", "letterbox" },
                },
                new SettingsItemV2(new FormEnumDropdown<ScalingMode>
                {
                    Caption = GraphicsSettingsStrings.ScreenScaling,
                    Current = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling),
                })
                {
                    Keywords = new[] { "scale", "letterbox" },
                },
                scalingSettings = new FillFlowContainer<SettingsItemV2>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    Spacing = new Vector2(0, SettingsSection.ITEM_SPACING_V2),
                    Children = new[]
                    {
                        new SettingsItemV2(new FormSliderBar<float>
                        {
                            Caption = GraphicsSettingsStrings.HorizontalPosition,
                            Current = scalingPositionX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true,
                        }.With(bindPreviewEvent))
                        {
                            Keywords = new[] { "screen", "scaling" },
                        },
                        new SettingsItemV2(new FormSliderBar<float>
                        {
                            Caption = GraphicsSettingsStrings.VerticalPosition,
                            Current = scalingPositionY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true,
                        }.With(bindPreviewEvent))
                        {
                            Keywords = new[] { "screen", "scaling" },
                        },
                        new SettingsItemV2(new FormSliderBar<float>
                        {
                            Caption = GraphicsSettingsStrings.HorizontalScale,
                            Current = scalingSizeX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true,
                        }.With(bindPreviewEvent))
                        {
                            Keywords = new[] { "screen", "scaling" },
                        },
                        new SettingsItemV2(new FormSliderBar<float>
                        {
                            Caption = GraphicsSettingsStrings.VerticalScale,
                            Current = scalingSizeY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true,
                        }.With(bindPreviewEvent))
                        {
                            Keywords = new[] { "screen", "scaling" },
                        },
                        new SettingsItemV2(dimSlider = new FormSliderBar<float>
                        {
                            Caption = GameplaySettingsStrings.BackgroundDim,
                            Current = scalingBackgroundDim,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true,
                        }.With(bindPreviewEvent)),
                    }
                },
            };

            fullscreenCapability.BindValueChanged(_ => Schedule(updateScreenModeWarning), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            windowModeDropdown.Current.BindValueChanged(_ =>
            {
                updateDisplaySettingsVisibility();
                updateScreenModeWarning();
            }, true);

            currentDisplay.BindValueChanged(display => Schedule(() =>
            {
                if (display.NewValue == null)
                {
                    resolutionsFullscreen.Clear();
                    resolutionsWindowed.Clear();
                    return;
                }

                var buffer = new Bindable<Size>(windowedResolution.Value);
                resolutionWindowedDropdown.Current = buffer;

                var fullscreenResolutions = display.NewValue.DisplayModes
                                                   .Where(m => m.Size.Width >= 800 && m.Size.Height >= 600)
                                                   .OrderByDescending(m => Math.Max(m.Size.Height, m.Size.Width))
                                                   .Select(m => m.Size)
                                                   .Distinct()
                                                   .ToList();
                var windowedResolutions = fullscreenResolutions
                                          .Where(res => res.Width <= display.NewValue.UsableBounds.Width && res.Height <= display.NewValue.UsableBounds.Height)
                                          .ToList();

                resolutionsFullscreen.ReplaceRange(1, resolutionsFullscreen.Count - 1, fullscreenResolutions);
                resolutionsWindowed.ReplaceRange(0, resolutionsWindowed.Count, windowedResolutions);

                resolutionWindowedDropdown.Current = windowedResolution;

                updateDisplaySettingsVisibility();
            }), true);

            windowedResolution.BindValueChanged(size =>
            {
                if (size.NewValue == sizeWindowed.Value || windowModeDropdown.Current.Value != WindowMode.Windowed)
                    return;

                if (window?.WindowState == Framework.Platform.WindowState.Maximised)
                {
                    window.WindowState = Framework.Platform.WindowState.Normal;
                }

                // Adjust only for top decorations (assuming system titlebar).
                // Bottom/left/right borders are ignored as invisible padding, which don't align with the screen.
                var dBounds = currentDisplay.Value.Bounds;
                var dUsable = currentDisplay.Value.UsableBounds;
                float topBar = host.Window?.BorderSize.Value.Top ?? 0;

                int w = Math.Min(size.NewValue.Width, dUsable.Width);
                int h = (int)Math.Min(size.NewValue.Height, dUsable.Height - topBar);

                windowedResolution.Value = new Size(w, h);
                sizeWindowed.Value = windowedResolution.Value;

                float adjustedY = Math.Max(
                    dUsable.Y + (dUsable.Height - h) / 2f,
                    dUsable.Y + topBar // titlebar adjustment
                );
                windowedPositionY.Value = dBounds.Height - h != 0 ? (adjustedY - dBounds.Y) / (dBounds.Height - h) : 0;
                windowedPositionX.Value = dBounds.Width - w != 0 ? (dUsable.X - dBounds.X + (dUsable.Width - w) / 2f) / (dBounds.Width - w) : 0;
            });

            sizeWindowed.BindValueChanged(size =>
            {
                if (size.NewValue != windowedResolution.Value)
                    windowedResolution.Value = size.NewValue;
            });

            scalingMode.BindValueChanged(_ =>
            {
                scalingSettings.ClearTransforms();
                scalingSettings.AutoSizeDuration = transition_duration;
                scalingSettings.AutoSizeEasing = Easing.OutQuint;

                updateScalingModeVisibility();
            });
            updateScalingModeVisibility();

            void updateScalingModeVisibility()
            {
                if (scalingMode.Value == ScalingMode.Off)
                    scalingSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);

                scalingSettings.AutoSizeAxes = scalingMode.Value != ScalingMode.Off ? Axes.Y : Axes.None;

                foreach (SettingsItemV2 item in scalingSettings)
                {
                    FormSliderBar<float> slider = (FormSliderBar<float>)item.Control;

                    if (slider == dimSlider)
                        item.CanBeShown.Value = scalingMode.Value == ScalingMode.Everything || scalingMode.Value == ScalingMode.ExcludeOverlays;
                    else
                    {
                        slider.TransferValueOnCommit = scalingMode.Value == ScalingMode.Everything;
                        item.CanBeShown.Value = scalingMode.Value != ScalingMode.Off;
                    }
                }
            }
        }

        private void onDisplaysChanged(IEnumerable<Display> displays)
        {
            Scheduler.AddOnce(d =>
            {
                if (!displayDropdown.Items.SequenceEqual(d, DisplayListComparer.DEFAULT))
                    displayDropdown.Items = d;
                updateDisplaySettingsVisibility();
            }, displays);
        }

        private void updateDisplaySettingsVisibility()
        {
            resolutionFullscreenCanBeShown.Value = windowModeDropdown.Current.Value == WindowMode.Fullscreen && resolutionsFullscreen.Count > 1;
            resolutionWindowedCanBeShown.Value = windowModeDropdown.Current.Value == WindowMode.Windowed && resolutionsWindowed.Count > 1;

            displayDropdownCanBeShown.Value = displayDropdown.Items.Count() > 1;
            minimiseOnFocusLossCanBeShown.Value = RuntimeInfo.IsDesktop && windowModeDropdown.Current.Value == WindowMode.Fullscreen;
            safeAreaConsiderationsCanBeShown.Value = host.Window?.SafeAreaPadding.Value.Total != Vector2.Zero;
        }

        private void updateScreenModeWarning()
        {
            // Can be removed once we stop supporting SDL2.
            if (RuntimeInfo.OS == RuntimeInfo.Platform.macOS && !FrameworkEnvironment.UseSDL3)
            {
                if (windowModeDropdown.Current.Value == WindowMode.Fullscreen)
                    windowModeDropdownNote.Value = new SettingsNote.Data(LayoutSettingsStrings.FullscreenMacOSNote, SettingsNote.Type.Critical);
                else
                    windowModeDropdownNote.Value = null;

                return;
            }

            if (windowModeDropdown.Current.Value != WindowMode.Fullscreen)
            {
                windowModeDropdownNote.Value = new SettingsNote.Data(GraphicsSettingsStrings.NotFullscreenNote, SettingsNote.Type.Warning);
                return;
            }

            if (host.Renderer is IWindowsRenderer)
            {
                switch (fullscreenCapability.Value)
                {
                    case FullscreenCapability.Unknown:
                        windowModeDropdownNote.Value = new SettingsNote.Data(LayoutSettingsStrings.CheckingForFullscreenCapabilities, SettingsNote.Type.Informational);
                        break;

                    case FullscreenCapability.Capable:
                        windowModeDropdownNote.Value = new SettingsNote.Data(LayoutSettingsStrings.OsuIsRunningExclusiveFullscreen, SettingsNote.Type.Informational);
                        break;

                    case FullscreenCapability.Incapable:
                        windowModeDropdownNote.Value = new SettingsNote.Data(LayoutSettingsStrings.UnableToRunExclusiveFullscreen, SettingsNote.Type.Warning);
                        break;
                }
            }
            else
            {
                // We can only detect exclusive fullscreen status on windows currently.
                windowModeDropdownNote.Value = null;
            }
        }

        private void bindPreviewEvent(FormSliderBar<float> slider)
        {
            slider.Current.ValueChanged += _ =>
            {
                switch (scalingMode.Value)
                {
                    case ScalingMode.Gameplay:
                        showPreview();
                        break;
                }
            };
        }

        private Drawable? preview;

        private void showPreview()
        {
            if (preview?.IsAlive != true)
                game.Add(preview = new ScalingPreview());

            preview.FadeOutFromOne(1500);
            preview.Expire();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (window != null)
                window.DisplaysChanged -= onDisplaysChanged;

            base.Dispose(isDisposing);
        }

        private partial class ScalingPreview : ScalingContainer
        {
            public ScalingPreview()
            {
                Child = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                };
            }
        }

        private partial class DisplayDropdown : FormDropdown<Display>
        {
            protected override LocalisableString GenerateItemText(Display item)
            {
                return $"{item.Index}: {item.Name} ({item.Bounds.Width}x{item.Bounds.Height})";
            }
        }

        private partial class ResolutionDropdown : FormDropdown<Size>
        {
            protected override LocalisableString GenerateItemText(Size item)
            {
                if (item == new Size(9999, 9999))
                    return CommonStrings.Default;

                return $"{item.Width}x{item.Height}";
            }
        }

        /// <summary>
        /// Contrary to <see cref="Display.Equals(osu.Framework.Platform.Display?)"/>, this comparer disregards the value of <see cref="Display.Bounds"/>.
        /// We want to just show a list of displays, and for the purposes of settings we don't care about their bounds when it comes to the list.
        /// However, <see cref="IWindow.DisplaysChanged"/> fires even if only the resolution of the current display was changed
        /// (because it causes the bounds of all displays to also change).
        /// We're not interested in those changes, so compare only the rest that we actually care about.
        /// This helps to avoid a bindable/event feedback loop, in which a resolution change
        /// would trigger a display "change", which would in turn reset resolution again.
        /// </summary>
        private class DisplayListComparer : IEqualityComparer<Display>
        {
            public static readonly DisplayListComparer DEFAULT = new DisplayListComparer();

            public bool Equals(Display? x, Display? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;

                return x.Index == y.Index
                       && x.Name == y.Name
                       && x.DisplayModes.SequenceEqual(y.DisplayModes);
            }

            public int GetHashCode(Display obj)
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Index);
                hashCode.Add(obj.Name);
                hashCode.Add(obj.DisplayModes.Length);
                foreach (var displayMode in obj.DisplayModes)
                    hashCode.Add(displayMode);

                return hashCode.ToHashCode();
            }
        }
    }
}
