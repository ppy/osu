// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class BeatmapTitle : CompositeDrawable
    {
        private readonly Room room;
        private readonly LinkFlowContainer textFlow;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public BeatmapTitle(Room room)
        {
            this.room = room;

            AutoSizeAxes = Axes.Both;
            InternalChild = textFlow = new LinkFlowContainer { AutoSizeAxes = Axes.Both };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            room.PropertyChanged += onRoomPropertyChanged;
            updateText();
        }

        private float textSize = OsuFont.DEFAULT_FONT_SIZE;

        public float TextSize
        {
            get => textSize;
            set
            {
                if (textSize == value)
                    return;

                textSize = value;

                updateText();
            }
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Room.Playlist))
                updateText();
        }

        private void updateText()
        {
            if (LoadState < LoadState.Loading)
                return;

            textFlow.Clear();

            var beatmap = room.Playlist.FirstOrDefault()?.Beatmap;

            if (beatmap == null)
            {
                textFlow.AddText("No beatmap selected", s =>
                {
                    s.Font = s.Font.With(size: TextSize);
                    s.Colour = colours.PinkLight;
                });
            }
            else
            {
                var metadataInfo = beatmap.Metadata;

                string artistUnicode = string.IsNullOrEmpty(metadataInfo.ArtistUnicode) ? metadataInfo.Artist : metadataInfo.ArtistUnicode;
                string titleUnicode = string.IsNullOrEmpty(metadataInfo.TitleUnicode) ? metadataInfo.Title : metadataInfo.TitleUnicode;

                var title = new RomanisableString($"{artistUnicode} - {titleUnicode}".Trim(), $"{metadataInfo.Artist} - {metadataInfo.Title}".Trim());

                textFlow.AddLink(title, LinkAction.OpenBeatmap, beatmap.OnlineID.ToString(), "Open beatmap");
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
