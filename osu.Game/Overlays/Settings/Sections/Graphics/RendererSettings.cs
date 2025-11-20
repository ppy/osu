// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering.LowLatency;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class RendererSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.RendererHeader;

        private bool automaticRendererInUse;

        private SettingsEnumDropdown<LatencyMode>? reflexSetting;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, IDialogOverlay? dialogOverlay, OsuGame? game, GameHost host)
        {
            var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
            automaticRendererInUse = renderer.Value == RendererType.Automatic;

            var reflexMode = config.GetBindable<LatencyMode>(FrameworkSetting.LatencyMode);
            var frameSyncMode = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync);

            Children = new Drawable[]
            {
                new RendererSettingsDropdown
                {
                    LabelText = GraphicsSettingsStrings.Renderer,
                    Current = renderer,
                    Items = host.GetPreferredRenderersForCurrentPlatform().Order()
#pragma warning disable CS0612 // Type or member is obsolete
                                .Where(t => t != RendererType.Vulkan && t != RendererType.OpenGLLegacy),
#pragma warning restore CS0612 // Type or member is obsolete
                    Keywords = new[] { @"compatibility", @"directx" },
                },
                // TODO: this needs to be a custom dropdown at some point
                new SettingsEnumDropdown<FrameSync>
                {
                    LabelText = GraphicsSettingsStrings.FrameLimiter,
                    Current = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync),
                    Keywords = new[] { @"fps" },
                },
                new SettingsEnumDropdown<ExecutionMode>
                {
                    LabelText = GraphicsSettingsStrings.ThreadingMode,
                    Current = config.GetBindable<ExecutionMode>(FrameworkSetting.ExecutionMode)
                },
                reflexSetting = new SettingsEnumDropdown<LatencyMode>
                {
                    LabelText = "NVIDIA Reflex",
                    Current = reflexMode,
                    Keywords = new[] { @"nvidia", @"latency", @"reflex" },
                    TooltipText = "Reduces latency by leveraging the NVIDIA Reflex API on NVIDIA GPUs.\nRecommended to have On, turn Off only if experiencing issues."
                },
                new SettingsCheckbox
                {
                    LabelText = GraphicsSettingsStrings.ShowFPS,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay)
                },
            };

            // Ensure NVIDIA reflex is turned off and hidden if the resolved renderer isn't Direct3D 11
            if (host.ResolvedRenderer is not (RendererType.Deferred_Direct3D11 or RendererType.Direct3D11))
            {
                reflexMode.Value = LatencyMode.Off;
                reflexSetting.Hide();
            }

            // Disable frame limiter if reflex is enabled and add notice when reflex boost is enabled
            reflexMode.BindValueChanged(r =>
            {
                if (r.NewValue != LatencyMode.Off)
                    frameSyncMode.Disabled = true;

                frameSyncMode.Disabled = false;

                reflexSetting.ClearNoticeText();

                if (r.NewValue == LatencyMode.Boost)
                    setReflexBoostNotice();
            }, true);

            renderer.BindValueChanged(r =>
            {
                if (r.NewValue == host.ResolvedRenderer)
                    return;

                // Need to check startup renderer for the "automatic" case, as ResolvedRenderer above will track the final resolved renderer instead.
                if (r.NewValue == RendererType.Automatic && automaticRendererInUse)
                    return;

                if (game?.RestartAppWhenExited() == true)
                {
                    game.AttemptExit();
                }
                else
                {
                    dialogOverlay?.Push(new ConfirmDialog(GraphicsSettingsStrings.ChangeRendererConfirmation, () => game?.AttemptExit(), () =>
                    {
                        renderer.Value = automaticRendererInUse ? RendererType.Automatic : host.ResolvedRenderer;
                    }));
                }
            });
        }

        private void setReflexBoostNotice() => reflexSetting?.SetNoticeText("Boost increases GPU power consumption and may increase latency in some cases. Disable Boost if experiencing issues.", true);

        private partial class RendererSettingsDropdown : SettingsEnumDropdown<RendererType>
        {
            protected override OsuDropdown<RendererType> CreateDropdown() => new RendererDropdown();

            protected partial class RendererDropdown : DropdownControl
            {
                private RendererType hostResolvedRenderer;
                private bool automaticRendererInUse;

                [BackgroundDependencyLoader]
                private void load(FrameworkConfigManager config, GameHost host)
                {
                    var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
                    automaticRendererInUse = renderer.Value == RendererType.Automatic;
                    hostResolvedRenderer = host.ResolvedRenderer;
                }

                protected override LocalisableString GenerateItemText(RendererType item)
                {
                    if (item == RendererType.Automatic && automaticRendererInUse)
                        return LocalisableString.Interpolate($"{base.GenerateItemText(item)} ({hostResolvedRenderer.GetDescription()})");

                    return base.GenerateItemText(item);
                }
            }
        }
    }
}
