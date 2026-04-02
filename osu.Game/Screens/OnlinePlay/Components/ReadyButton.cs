// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract partial class ReadyButton : RoundedButton
    {
        private readonly IBindable<BeatmapAvailability> availability = new Bindable<BeatmapAvailability>();

        [BackgroundDependencyLoader]
        private void load(OnlinePlayBeatmapAvailabilityTracker beatmapTracker)
        {
            availability.BindTo(beatmapTracker.Availability);
            availability.BindValueChanged(_ => UpdateEnabledState());
        }

        protected virtual void UpdateEnabledState() => Enabled.Value = availability.Value.State == DownloadState.LocallyAvailable;

        public override LocalisableString TooltipText
        {
            get
            {
                if (availability.Value.State != DownloadState.LocallyAvailable)
                    return "Beatmap not downloaded";

                return string.Empty;
            }
        }
    }
}
