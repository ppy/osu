// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class RendererSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.RendererHeader;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, IDialogOverlay dialogOverlay, OsuGame game, GameHost host)
        {
            var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<RendererType>
                {
                    LabelText = GraphicsSettingsStrings.Renderer,
                    Current = renderer,
                    Items = host.GetPreferredRenderersForCurrentPlatform().OrderBy(t => t),
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
                new SettingsCheckbox
                {
                    LabelText = GraphicsSettingsStrings.ShowFPS,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay)
                },
            };

            renderer.BindValueChanged(r =>
            {
                if (r.NewValue == host.ResolvedRenderer)
                    return;

                dialogOverlay.Push(new ConfirmDialog(GraphicsSettingsStrings.ChangeRendererConfirmation, game.AttemptExit, () =>
                {
                    renderer.SetDefault();
                }));
            });
        }
    }
}
