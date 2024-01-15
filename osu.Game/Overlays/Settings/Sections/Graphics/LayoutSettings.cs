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
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class LayoutSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.LayoutHeader;

        private FillFlowContainer<SettingsSlider<float>> scalingSettings = null!;
        private SettingsSlider<float> dimSlider = null!;

        private readonly Bindable<Display> currentDisplay = new Bindable<Display>();

        private Bindable<ScalingMode> scalingMode = null!;
        private Bindable<Size> sizeFullscreen = null!;

        private readonly BindableList<Size> resolutions = new BindableList<Size>(new[] { new Size(9999, 9999) });
        private readonly IBindable<FullscreenCapability> fullscreenCapability = new Bindable<FullscreenCapability>(FullscreenCapability.Capable);

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private GameHost host { get; set; } = null!;

        private IWindow? window;

        private SettingsDropdown<Size> resolutionDropdown = null!;
        private SettingsDropdown<Display> displayDropdown = null!;
        private SettingsDropdown<WindowMode> windowModeDropdown = null!;
        private SettingsCheckbox minimiseOnFocusLossCheckbox = null!;
        private SettingsCheckbox safeAreaConsiderationsCheckbox = null!;

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
            scalingSizeX = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeX);
            scalingSizeY = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeY);
            scalingPositionX = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionX);
            scalingPositionY = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionY);
            scalingBackgroundDim = osuConfig.GetBindable<float>(OsuSetting.ScalingBackgroundDim);

            if (window != null)
            {
                currentDisplay.BindTo(window.CurrentDisplayBindable);
                window.DisplaysChanged += onDisplaysChanged;
            }

            if (host.Renderer is IWindowsRenderer windowsRenderer)
                fullscreenCapability.BindTo(windowsRenderer.FullscreenCapability);

            Children = new Drawable[]
            {
                windowModeDropdown = new SettingsDropdown<WindowMode>
                {
                    LabelText = GraphicsSettingsStrings.ScreenMode,
                    Items = window?.SupportedWindowModes,
                    CanBeShown = { Value = window?.SupportedWindowModes.Count() > 1 },
                    Current = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                displayDropdown = new DisplaySettingsDropdown
                {
                    LabelText = GraphicsSettingsStrings.Display,
                    Items = window?.Displays,
                    Current = currentDisplay,
                },
                resolutionDropdown = new ResolutionSettingsDropdown
                {
                    LabelText = GraphicsSettingsStrings.Resolution,
                    ShowsDefaultIndicator = false,
                    ItemSource = resolutions,
                    Current = sizeFullscreen
                },
                minimiseOnFocusLossCheckbox = new SettingsCheckbox
                {
                    LabelText = GraphicsSettingsStrings.MinimiseOnFocusLoss,
                    Current = config.GetBindable<bool>(FrameworkSetting.MinimiseOnFocusLossInFullscreen),
                    Keywords = new[] { "alt-tab", "minimize", "focus", "hide" },
                },
                safeAreaConsiderationsCheckbox = new SettingsCheckbox
                {
                    LabelText = "Shrink game to avoid cameras and notches",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.SafeAreaConsiderations),
                },
                new SettingsSlider<float, UIScaleSlider>
                {
                    LabelText = GraphicsSettingsStrings.UIScaling,
                    TransferValueOnCommit = true,
                    Current = osuConfig.GetBindable<float>(OsuSetting.UIScale),
                    KeyboardStep = 0.01f,
                    Keywords = new[] { "scale", "letterbox" },
                },
                new SettingsEnumDropdown<ScalingMode>
                {
                    LabelText = GraphicsSettingsStrings.ScreenScaling,
                    Current = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling),
                    Keywords = new[] { "scale", "letterbox" },
                },
                scalingSettings = new FillFlowContainer<SettingsSlider<float>>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    Children = new[]
                    {
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.HorizontalPosition,
                            Keywords = new[] { "screen", "scaling" },
                            Current = scalingPositionX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.VerticalPosition,
                            Keywords = new[] { "screen", "scaling" },
                            Current = scalingPositionY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.HorizontalScale,
                            Keywords = new[] { "screen", "scaling" },
                            Current = scalingSizeX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.VerticalScale,
                            Keywords = new[] { "screen", "scaling" },
                            Current = scalingSizeY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        dimSlider = new SettingsSlider<float>
                        {
                            LabelText = GameplaySettingsStrings.BackgroundDim,
                            Current = scalingBackgroundDim,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true,
                        },
                    }
                },
            };

            fullscreenCapability.BindValueChanged(_ => Schedule(updateScreenModeWarning), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scalingSettings.ForEach(s => bindPreviewEvent(s.Current));

            windowModeDropdown.Current.BindValueChanged(_ =>
            {
                updateDisplaySettingsVisibility();
                updateScreenModeWarning();
            }, true);

            currentDisplay.BindValueChanged(display => Schedule(() =>
            {
                if (display.NewValue == null)
                {
                    resolutions.Clear();
                    return;
                }

                resolutions.ReplaceRange(1, resolutions.Count - 1, display.NewValue.DisplayModes
                                                                          .Where(m => m.Size.Width >= 800 && m.Size.Height >= 600)
                                                                          .OrderByDescending(m => Math.Max(m.Size.Height, m.Size.Width))
                                                                          .Select(m => m.Size)
                                                                          .Distinct());

                updateDisplaySettingsVisibility();
            }), true);

            scalingMode.BindValueChanged(_ =>
            {
                scalingSettings.ClearTransforms();
                scalingSettings.AutoSizeDuration = transition_duration;
                scalingSettings.AutoSizeEasing = Easing.OutQuint;

                updateScalingModeVisibility();
            });

            // initial update bypasses transforms
            updateScalingModeVisibility();

            void updateScalingModeVisibility()
            {
                if (scalingMode.Value == ScalingMode.Off)
                    scalingSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);

                scalingSettings.AutoSizeAxes = scalingMode.Value != ScalingMode.Off ? Axes.Y : Axes.None;
                scalingSettings.ForEach(s =>
                {
                    if (s == dimSlider)
                    {
                        s.CanBeShown.Value = scalingMode.Value == ScalingMode.Everything || scalingMode.Value == ScalingMode.ExcludeOverlays;
                    }
                    else
                    {
                        s.TransferValueOnCommit = scalingMode.Value == ScalingMode.Everything;
                        s.CanBeShown.Value = scalingMode.Value != ScalingMode.Off;
                    }
                });
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
            resolutionDropdown.CanBeShown.Value = resolutions.Count > 1 && windowModeDropdown.Current.Value == WindowMode.Fullscreen;
            displayDropdown.CanBeShown.Value = displayDropdown.Items.Count() > 1;
            minimiseOnFocusLossCheckbox.CanBeShown.Value = RuntimeInfo.IsDesktop && windowModeDropdown.Current.Value == WindowMode.Fullscreen;
            safeAreaConsiderationsCheckbox.CanBeShown.Value = host.Window?.SafeAreaPadding.Value.Total != Vector2.Zero;
        }

        private void updateScreenModeWarning()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.macOS)
            {
                if (windowModeDropdown.Current.Value == WindowMode.Fullscreen)
                    windowModeDropdown.SetNoticeText(LayoutSettingsStrings.FullscreenMacOSNote, true);
                else
                    windowModeDropdown.ClearNoticeText();

                return;
            }

            if (windowModeDropdown.Current.Value != WindowMode.Fullscreen)
            {
                windowModeDropdown.SetNoticeText(GraphicsSettingsStrings.NotFullscreenNote, true);
                return;
            }

            if (host.Renderer is IWindowsRenderer)
            {
                switch (fullscreenCapability.Value)
                {
                    case FullscreenCapability.Unknown:
                        windowModeDropdown.SetNoticeText(LayoutSettingsStrings.CheckingForFullscreenCapabilities, true);
                        break;

                    case FullscreenCapability.Capable:
                        windowModeDropdown.SetNoticeText(LayoutSettingsStrings.OsuIsRunningExclusiveFullscreen);
                        break;

                    case FullscreenCapability.Incapable:
                        windowModeDropdown.SetNoticeText(LayoutSettingsStrings.UnableToRunExclusiveFullscreen, true);
                        break;
                }
            }
            else
            {
                // We can only detect exclusive fullscreen status on windows currently.
                windowModeDropdown.ClearNoticeText();
            }
        }

        private void bindPreviewEvent(Bindable<float> bindable)
        {
            bindable.ValueChanged += _ =>
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

        private partial class UIScaleSlider : RoundedSliderBar<float>
        {
            public override LocalisableString TooltipText => base.TooltipText + "x";
        }

        private partial class DisplaySettingsDropdown : SettingsDropdown<Display>
        {
            protected override OsuDropdown<Display> CreateDropdown() => new DisplaySettingsDropdownControl();

            private partial class DisplaySettingsDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(Display item)
                {
                    return $"{item.Index}: {item.Name} ({item.Bounds.Width}x{item.Bounds.Height})";
                }
            }
        }

        private partial class ResolutionSettingsDropdown : SettingsDropdown<Size>
        {
            protected override OsuDropdown<Size> CreateDropdown() => new ResolutionDropdownControl();

            private partial class ResolutionDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(Size item)
                {
                    if (item == new Size(9999, 9999))
                        return CommonStrings.Default;

                    return $"{item.Width}x{item.Height}";
                }
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
