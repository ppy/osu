// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract partial class ReadyButtonV2 : ShearedButtonWithIcon
    {
        private readonly IBindable<BeatmapAvailability> availability = new Bindable<BeatmapAvailability>();

        [BackgroundDependencyLoader]
        private void load(OnlinePlayBeatmapAvailabilityTracker beatmapTracker)
        {
            availability.BindTo(beatmapTracker.Availability);
            availability.BindValueChanged(_ => UpdateState());
        }

        protected virtual void UpdateState() => Enabled.Value = availability.Value.State == DownloadState.LocallyAvailable;

        public override LocalisableString TooltipText
        {
            get
            {
                if (availability.Value.State != DownloadState.LocallyAvailable)
                    return OnlinePlayStrings.FooterButtonBeatmapNotDownloaded;

                return string.Empty;
            }
        }
    }
}
