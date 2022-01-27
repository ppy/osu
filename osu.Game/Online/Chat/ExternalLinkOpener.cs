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
        [Resolved]
        private GameHost host { get; set; }

        [Resolved(CanBeNull = true)]
        private DialogOverlay dialogOverlay { get; set; }

        private Bindable<bool> externalLinkWarning;

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config)
        {
            externalLinkWarning = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning);
        }

        public void OpenUrlExternally(string url, bool bypassWarning = false)
        {
            if (!bypassWarning && externalLinkWarning.Value)
                dialogOverlay.Push(new ExternalLinkDialog(url, () => host.OpenUrlExternally(url)));
            else
                host.OpenUrlExternally(url);
        }
    }
}
