// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;

namespace osu.Game.Online.Chat
{
    public class ExternalLinkOpener : Component
    {
        private GameHost host;
        private DialogOverlay dialogOverlay;
        private Bindable<bool> externalLinkWarning;

        [BackgroundDependencyLoader(true)]
        private void load(GameHost host, DialogOverlay dialogOverlay, OsuConfigManager config)
        {
            this.host = host;
            this.dialogOverlay = dialogOverlay;
            externalLinkWarning = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning);
        }

        public void OpenUrlExternally(string url)
        {
            if (externalLinkWarning.Value)
                dialogOverlay.Push(new ExternalLinkDialog(url, () => host.OpenUrlExternally(url)));
            else
                host.OpenUrlExternally(url);
        }
    }
}
