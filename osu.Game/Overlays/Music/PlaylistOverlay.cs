// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistOverlay : OverlayContainer
    {
        private const float transition_duration = 600;

        private const float playlist_height = 510;

        private FilterControl filter;
        private PlaylistList list;

        private BeatmapManager beatmaps;

        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        public IEnumerable<BeatmapSetInfo> BeatmapSets => list.BeatmapSets;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, BeatmapManager beatmaps, OsuColour colours)
        {
            this.beatmaps = beatmaps;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
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

            beatmaps.ItemAdded += handleBeatmapAdded;
            beatmaps.ItemRemoved += handleBeatmapRemoved;

            list.BeatmapSets = beatmaps.GetAllUsableBeatmapSets();

            beatmapBacking.BindTo(game.Beatmap);

            filter.Search.OnCommit = (sender, newText) =>
            {
                var beatmap = list.FirstVisibleSet?.Beatmaps?.FirstOrDefault();
                if (beatmap != null) playSpecified(beatmap);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmapBacking.ValueChanged += b => list.SelectedSet = b?.BeatmapSetInfo;
            beatmapBacking.TriggerChange();
        }

        private void handleBeatmapAdded(BeatmapSetInfo setInfo) => Schedule(() => list.AddBeatmapSet(setInfo));
        private void handleBeatmapRemoved(BeatmapSetInfo setInfo) => Schedule(() => list.RemoveBeatmapSet(setInfo));

        protected override void PopIn()
        {
            filter.Search.HoldFocus = true;
            Schedule(() => GetContainingInputManager().ChangeFocus(filter.Search));

            this.ResizeTo(new Vector2(1, playlist_height), transition_duration, Easing.OutQuint);
            this.FadeIn(transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            filter.Search.HoldFocus = false;

            this.ResizeTo(new Vector2(1, 0), transition_duration, Easing.OutQuint);
            this.FadeOut(transition_duration);
        }

        private void itemSelected(BeatmapSetInfo set)
        {
            if (set.ID == (beatmapBacking.Value?.BeatmapSetInfo?.ID ?? -1))
            {
                beatmapBacking.Value?.Track?.Seek(0);
                return;
            }

            playSpecified(set.Beatmaps.First());
        }

        public void PlayPrevious()
        {
            var playable = list.PreviousSet;

            if (playable != null)
            {
                playSpecified(playable.Beatmaps.First());
                list.SelectedSet = playable;
            }
        }

        public void PlayNext()
        {
            var playable = list.NextSet;

            if (playable != null)
            {
                playSpecified(playable.Beatmaps.First());
                list.SelectedSet = playable;
            }
        }

        private void playSpecified(BeatmapInfo info)
        {
            beatmapBacking.Value = beatmaps.GetWorkingBeatmap(info, beatmapBacking);

            var track = beatmapBacking.Value.Track;

            track.Restart();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmaps != null)
            {
                beatmaps.ItemAdded -= handleBeatmapAdded;
                beatmaps.ItemRemoved -= handleBeatmapRemoved;
            }
        }
    }

    //todo: placeholder
    public enum PlaylistCollection
    {
        All
    }
}
