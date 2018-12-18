// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class ReadyButton : HeaderButton
    {
        public readonly IBindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public ReadyButton()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(200, 1);

            Text = "Start";
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmaps.ItemAdded += beatmapAdded;

            Beatmap.BindValueChanged(updateEnabledState, true);
        }

        private void updateEnabledState(BeatmapInfo beatmap)
        {
            if (beatmap?.OnlineBeatmapID == null)
            {
                Enabled.Value = false;
                return;
            }

            Enabled.Value = beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID) != null;
        }

        private void beatmapAdded(BeatmapSetInfo model, bool existing, bool silent)
        {
            if (model.Beatmaps.Any(b => b.OnlineBeatmapID == Beatmap.Value.OnlineBeatmapID))
                Schedule(() => Enabled.Value = true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
                beatmaps.ItemAdded -= beatmapAdded;
        }
    }
}
