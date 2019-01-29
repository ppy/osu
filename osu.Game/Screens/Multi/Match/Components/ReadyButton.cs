// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class ReadyButton : HeaderButton
    {
        public readonly IBindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        private readonly Room room;

        [Resolved]
        private IBindableBeatmap gameBeatmap { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private bool hasBeatmap;

        public ReadyButton(Room room)
        {
            this.room = room;
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(200, 1);

            Text = "Start";
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmaps.ItemAdded += beatmapAdded;

            Beatmap.BindValueChanged(updateBeatmap, true);
        }

        private void updateBeatmap(BeatmapInfo beatmap)
        {
            hasBeatmap = false;

            if (beatmap?.OnlineBeatmapID == null)
                return;

            hasBeatmap = beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID) != null;
        }

        private void beatmapAdded(BeatmapSetInfo model, bool existing, bool silent)
        {
            if (model.Beatmaps.Any(b => b.OnlineBeatmapID == Beatmap.Value.OnlineBeatmapID))
                Schedule(() => hasBeatmap = true);
        }

        protected override void Update()
        {
            base.Update();

            updateEnabledState();
        }

        private void updateEnabledState()
        {
            if (gameBeatmap.Value == null)
            {
                Enabled.Value = false;
                return;
            }

            bool hasEnoughTime = DateTimeOffset.UtcNow.AddSeconds(30).AddMilliseconds(gameBeatmap.Value.Track.Length) < room.EndDate;

            Enabled.Value = hasBeatmap && hasEnoughTime;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
                beatmaps.ItemAdded -= beatmapAdded;
        }
    }
}
