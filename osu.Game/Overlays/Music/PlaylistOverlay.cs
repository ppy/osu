// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions;

namespace osu.Game.Overlays.Music
{
    public class PlaylistOverlay : OverlayContainer
    {
        private const float transition_duration = 600;

        private const float playlist_height = 510;

        private FilterControl filter;
        private PlaylistList list;

        private TrackManager trackManager;
        private BeatmapDatabase beatmaps;

        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        public IEnumerable<BeatmapSetInfo> BeatmapSets;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, BeatmapDatabase beatmaps, OsuColour colours)
        {
            this.beatmaps = beatmaps;
            trackManager = game.Audio.Track;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(40),
                        Radius = 5,
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.Gray3,
                            RelativeSizeAxes = Axes.Both,
                        },
                        list = new PlaylistList
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 95, Bottom = 10, Right = 10 },
                            OnSelect = itemSelected,
                        },
                        filter = new FilterControl
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ExitRequested = () => State = Visibility.Hidden,
                            FilterChanged = search => list.Filter(search),
                            Padding = new MarginPadding(10),
                        },
                    },
                },
            };

            list.BeatmapSets = BeatmapSets = beatmaps.GetAllWithChildren<BeatmapSetInfo>().ToList();

            beatmapBacking.BindTo(game.Beatmap);

            filter.Search.OnCommit = (sender, newText) => {
                var beatmap = list.FirstVisibleSet?.Beatmaps?.FirstOrDefault();
                if (beatmap != null) playSpecified(beatmap);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmapBacking.ValueChanged += b => list.SelectedItem = b?.BeatmapSetInfo;
            beatmapBacking.TriggerChange();
        }

        protected override void PopIn()
        {
            filter.Search.HoldFocus = true;
            Schedule(() => filter.Search.TriggerFocus());

            ResizeTo(new Vector2(1, playlist_height), transition_duration, EasingTypes.OutQuint);
            FadeIn(transition_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            filter.Search.HoldFocus = false;

            ResizeTo(new Vector2(1, 0), transition_duration, EasingTypes.OutQuint);
            FadeOut(transition_duration);
        }

        private void itemSelected(BeatmapSetInfo set)
        {
            if (set.ID == (beatmapBacking.Value?.BeatmapSetInfo?.ID ?? -1))
            {
                beatmapBacking.Value?.Track?.Seek(0);
                return;
            }

            playSpecified(set.Beatmaps[0]);
        }

        public void PlayPrevious()
        {
            var currentID = beatmapBacking.Value?.BeatmapSetInfo.ID ?? -1;
            var available = BeatmapSets.Reverse();

            var playable = available.SkipWhile(b => b.ID != currentID).Skip(1).FirstOrDefault() ?? available.FirstOrDefault();

            if (playable != null)
                playSpecified(playable.Beatmaps[0]);
        }

        public void PlayNext()
        {
            var currentID = beatmapBacking.Value?.BeatmapSetInfo.ID ?? -1;
            var available = BeatmapSets;

            var playable = available.SkipWhile(b => b.ID != currentID).Skip(1).FirstOrDefault() ?? available.FirstOrDefault();

            if (playable != null)
                playSpecified(playable.Beatmaps[0]);
        }

        private void playSpecified(BeatmapInfo info)
        {
            beatmapBacking.Value = beatmaps.GetWorkingBeatmap(info, beatmapBacking);

            Task.Run(() =>
            {
                var track = beatmapBacking.Value.Track;
                trackManager.SetExclusive(track);
                track.Start();
            }).ContinueWith(task => Schedule(task.ThrowIfFaulted), TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    //todo: placeholder
    public enum PlaylistCollection
    {
        All
    }
}
