// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Database;
using OpenTK;

namespace osu.Game.Overlays.Music
{
    public class Playlist : Container
    {
        private FlowContainer<PlaylistItem> playlistFlow;
        private List<BeatmapSetInfo> items;
        private BeatmapSetInfo selectedItem;

        public event Action SelectionChanged;

        public List<BeatmapSetInfo> Items
        {
            get { return items; }
            set
            {
                if (items == value) return;
                foreach (var playlistItem in playlistFlow.Children)
                    playlistItem.OnSelected -= PlaylistItem_OnSelected;
                items = value;
                playlistFlow.Children = items.Select(i => new PlaylistItem(i));
                foreach (var playlistItem in playlistFlow.Children)
                    playlistItem.OnSelected += PlaylistItem_OnSelected;
            }
        }

        public List<int> AvailableIndexes => playlistFlow.Children.Where(i => i.Alpha == 1).Select(i => i.BeatmapSetInfo).Select(i => Items.IndexOf(i)).ToList();

        public BeatmapSetInfo SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                foreach (var playlistItem in playlistFlow.Children)
                {
                    playlistItem.State = playlistItem.BeatmapSetInfo.Beatmaps[0].AudioEquals(selectedItem.Beatmaps[0])
                        ? DropDownMenuItemState.Selected
                        : DropDownMenuItemState.NotSelected;
                }
                SelectionChanged?.Invoke();
            }
        }

        public Playlist()
        {
            Children = new[]
            {
                new ScrollContainer
                {
                    Children = new Drawable[]
                    {
                        playlistFlow = new FlowContainer<PlaylistItem>
                        {
                            Direction = FlowDirections.Vertical,
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                }
            };
        }

        private void PlaylistItem_OnSelected(BeatmapSetInfo newSelection) => SelectedItem = newSelection;

        public void Filter(string text)
        {
            foreach (var playlistItem in playlistFlow.Children)
                BeatmapFilter.Filter(playlistItem.BeatmapSetInfo.Metadata, text, playlistItem.Show, playlistItem.Hide);
            playlistFlow.Invalidate();
        }
    }
}
