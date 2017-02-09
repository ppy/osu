using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        public BeatmapSetInfo SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                foreach (var playlistItem in playlistFlow.Children)
                {
                    playlistItem.State = playlistItem.BeatmapSetInfo.Beatmaps[0].AudioEquals(selectedItem.Beatmaps[0]) ?
                        SelectionState.Selected :
                        SelectionState.NotSelected;
                }
                SelectionChanged?.Invoke();
            }
        }

        public Playlist()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new[]
            {
                new ScrollContainer
                {
                    Children =new []
                    {
                        playlistFlow = new FlowContainer<PlaylistItem>
                        {
                            Direction=FlowDirection.VerticalOnly,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(0,10)
                        }
                    }
                }
            };
        }

        private void PlaylistItem_OnSelected(BeatmapSetInfo newSelection) => SelectedItem = newSelection;

        public void Filter(string text)
        {
            foreach (var playlistItem in playlistFlow.Children)
            {
                var metadata = playlistItem.BeatmapSetInfo.Metadata;
                var match = string.IsNullOrEmpty(text)
                    || (metadata.Artist ?? "").IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (metadata.ArtistUnicode ?? "").IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (metadata.Title ?? "").IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1
                    || (metadata.TitleUnicode ?? "").IndexOf(text, StringComparison.InvariantCultureIgnoreCase) != -1;
                if (match)
                    playlistItem.Show();
                else
                    playlistItem.Hide();
            }
            playlistFlow.Invalidate();
        }
    }
}
