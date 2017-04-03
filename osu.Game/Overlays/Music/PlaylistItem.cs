// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistItem : ClickableContainer, IStateful<PlaylistItemState>
    {
        private readonly SpriteText artist;
        private readonly SpriteText title;
        private PlaylistItemState _state;
        private Func<string, string, string> unicodeString;
        private Bindable<bool> preferUnicode;

        public BeatmapSetInfo BeatmapSetInfo { get; }

        public PlaylistItemState State
        {
            get { return _state; }

            set
            {
                _state = value;
                switch (value)
                {
                    case PlaylistItemState.Selected:
                        selected();
                        break;
                    case PlaylistItemState.NotSelected:
                        deselected();
                        break;
                }
            }
        }

        public event Action<BeatmapSetInfo> OnSelected;

        public PlaylistItem(BeatmapSetInfo beatmapSetInfo)
        {
            Padding = new MarginPadding
            {
                Bottom = 5,
                Top = 5
            };
            BeatmapSetInfo = beatmapSetInfo;
            Action = () => OnSelected?.Invoke(BeatmapSetInfo);

            AutoSizeAxes = Axes.Both;
            Children = new[]
            {
                new FillFlowContainer<SpriteText>
                {
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10, 0),
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        title = new SpriteText
                        {
                            Font = @"Exo2.0-Regular",
                            TextSize = 14,
                            Colour = Color4.White
                        },
                        artist = new SpriteText
                        {
                            Font = @"Exo2.0-Bold",
                            TextSize = 12,
                            Colour = OsuColour.FromHex("999")
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            unicodeString = config.GetUnicodeString;
            preferUnicode = config.GetBindable<bool>(OsuConfig.ShowUnicode);
            preferUnicode.ValueChanged += preferUnicode_changed;
            updateDisplay();
        }

        private void preferUnicode_changed(object sender, EventArgs e) => updateDisplay();

        private void updateDisplay()
        {
            title.Text = unicodeString(BeatmapSetInfo.Metadata.Title, BeatmapSetInfo.Metadata.TitleUnicode);
            artist.Text = unicodeString(BeatmapSetInfo.Metadata.Artist, BeatmapSetInfo.Metadata.ArtistUnicode);
        }

        private void selected() => title.Colour = Color4.Orange;

        private void deselected() => title.Colour = Color4.White;
    }
}
