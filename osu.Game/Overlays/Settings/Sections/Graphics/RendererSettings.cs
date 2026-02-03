// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class RendererSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.RendererHeader;

        private bool automaticRendererInUse;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, IDialogOverlay? dialogOverlay, OsuGame? game, GameHost host)
        {
            var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
            automaticRendererInUse = renderer.Value == RendererType.Automatic;

            var children = new List<Drawable>();

            children.Add(new SettingsItemV2(new RendererDropdown
            {
                Caption = GraphicsSettingsStrings.Renderer,
                Current = renderer,
                Items = host.GetPreferredRenderersForCurrentPlatform().Order()
#pragma warning disable CS0612 // Type or member is obsolete
                            .Where(t => t != RendererType.OpenGLLegacy),
#pragma warning restore CS0612 // Type or member is obsolete
            })
            {
                Keywords = new[] { @"compatibility", @"directx" },
            });

            if (RuntimeInfo.IsMobile)
            {
                children.Add(new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Use Vulkan Renderer (Experimental)",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.VulkanRenderer),
                })
                {
                    Keywords = new[] { @"android", @"vulkan", @"graphics" },
                });

                children.Add(new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Use ANGLE (GLES to Vulkan)",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.UseAngle),
                })
                {
                    Keywords = new[] { @"android", @"vulkan", @"angle" },
                });
            }

            children.Add(new SettingsItemV2(new FormEnumDropdown<FrameSync>
            {
                Caption = GraphicsSettingsStrings.FrameLimiter,
                Current = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync),
            })
            {
                Keywords = new[] { @"fps", @"framerate" },
            });

            children.Add(new SettingsItemV2(new FormEnumDropdown<ExecutionMode>
            {
                Caption = GraphicsSettingsStrings.ThreadingMode,
                Current = config.GetBindable<ExecutionMode>(FrameworkSetting.ExecutionMode)
            }));

            if (RuntimeInfo.IsMobile)
            {
                children.Add(new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Performance Mode",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.PerformanceMode),
                })
                {
                    Keywords = new[] { @"android", @"vulkan", @"low latency" },
                });
            }

            children.Add(new SettingsItemV2(new FormCheckBox
            {
                Caption = GraphicsSettingsStrings.ShowFPS,
                Current = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay),
            })
            {
                Keywords = new[] { @"framerate", @"counter" },
            });

            Children = children.ToArray();

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

        private partial class RendererDropdown : FormEnumDropdown<RendererType>
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

                if (item == RendererType.Vulkan)
                    return "Vulkan (Experimental)";

                return base.GenerateItemText(item);
            }
        }
    }
}
