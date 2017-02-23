// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistItem : ClickableContainer, IStateful<DropDownMenuItemState>
    {
        private SpriteText artist, title;
        private DropDownMenuItemState _state;
        private Func<string, string, string> unicodeString;
        private Bindable<bool> preferUnicode;

        public BeatmapSetInfo BeatmapSetInfo { get; }

        public DropDownMenuItemState State
        {
            get { return _state; }

            set
            {
                _state = value;
                switch (value)
                {
                    case DropDownMenuItemState.Selected:
                        selected();
                        break;
                    case DropDownMenuItemState.NotSelected:
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
                new FlowContainer<SpriteText>
                {
                    Direction = FlowDirections.Horizontal,
                    Spacing = new Vector2(5, 0),
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        title = new OsuSpriteText
                        {
                            Font = @"Exo2.0-Regular",
                            TextSize = 14,
                            Colour = Color4.White
                        },
                        artist = new OsuSpriteText
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
