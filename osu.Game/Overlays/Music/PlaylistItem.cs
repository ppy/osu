using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistItem : FlowContainer<SpriteText>, IStateful<SelectionState>
    {
        private SpriteText artist, title;
        private SelectionState _state;
        private Func<string, string, string> unicodeString;
        private Bindable<bool> preferUnicode;

        public BeatmapSetInfo BeatmapSetInfo { get; }
        public SelectionState State
        {
            get { return _state; }

            set
            {
                _state = value;
                switch (value)
                {
                    case SelectionState.Selected:
                        selected();
                        break;
                    case SelectionState.NotSelected:
                        deselected();
                        break;
                }
            }
        }

        public event Action<BeatmapSetInfo> OnSelected;

        public PlaylistItem(BeatmapSetInfo beatmapSetInfo)
        {
            BeatmapSetInfo = beatmapSetInfo;
            Direction = FlowDirection.HorizontalOnly;
            AutoSizeAxes = Axes.Both;
            Spacing = new Vector2(5, 0);

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

        protected override bool OnClick(InputState state)
        {
            OnSelected?.Invoke(BeatmapSetInfo);
            return true;
        }

        private void selected() => title.Colour = Color4.Orange;

        private void deselected() => title.Colour = Color4.White;
    }
}
