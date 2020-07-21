// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Linq.Expressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class ReadyButton : TriangleButton
    {
        public readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        [Resolved(typeof(Room), nameof(Room.EndDate))]
        private Bindable<DateTimeOffset> endDate { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> gameBeatmap { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private bool hasBeatmap;

        public ReadyButton()
        {
            Text = "Start";
        }

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;
        private IBindable<WeakReference<BeatmapSetInfo>> managerRemoved;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            managerUpdated = beatmaps.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);
            managerRemoved = beatmaps.ItemRemoved.GetBoundCopy();
            managerRemoved.BindValueChanged(beatmapRemoved);

            SelectedItem.BindValueChanged(item => updateSelectedItem(item.NewValue), true);

            BackgroundColour = colours.Green;
            Triangles.ColourDark = colours.Green;
            Triangles.ColourLight = colours.GreenLight;
        }

        private void updateSelectedItem(PlaylistItem item)
        {
            hasBeatmap = findBeatmap(expr => beatmaps.QueryBeatmap(expr));
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> weakSet)
        {
            if (weakSet.NewValue.TryGetTarget(out var set))
            {
                if (findBeatmap(expr => set.Beatmaps.AsQueryable().FirstOrDefault(expr)))
                    Schedule(() => hasBeatmap = true);
            }
        }

        private void beatmapRemoved(ValueChangedEvent<WeakReference<BeatmapSetInfo>> weakSet)
        {
            if (weakSet.NewValue.TryGetTarget(out var set))
            {
                if (findBeatmap(expr => set.Beatmaps.AsQueryable().FirstOrDefault(expr)))
                    Schedule(() => hasBeatmap = false);
            }
        }

        private bool findBeatmap(Func<Expression<Func<BeatmapInfo, bool>>, BeatmapInfo> expression)
        {
            int? beatmapId = SelectedItem.Value?.Beatmap.Value?.OnlineBeatmapID;
            string checksum = SelectedItem.Value?.Beatmap.Value?.MD5Hash;

            if (beatmapId == null || checksum == null)
                return false;

            return expression(b => b.OnlineBeatmapID == beatmapId && b.MD5Hash == checksum) != null;
        }

        protected override void Update()
        {
            base.Update();

            updateEnabledState();
        }

        private void updateEnabledState()
        {
            if (gameBeatmap.Value == null || SelectedItem.Value == null)
            {
                Enabled.Value = false;
                return;
            }

            bool hasEnoughTime = DateTimeOffset.UtcNow.AddSeconds(30).AddMilliseconds(gameBeatmap.Value.Track.Length) < endDate.Value;

            Enabled.Value = hasBeatmap && hasEnoughTime;
        }
    }
}
