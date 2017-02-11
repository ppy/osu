// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Screens.Select;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistController : Container
    {
        public BeatmapDatabase Beatmaps { get; private set; }
        public List<BeatmapSetInfo> PlayList { get; private set; }
        public int PlayListIndex => PlayList.IndexOf(PlayList.Single(pi => pi.Hash == BeatmapSource.Value?.BeatmapSetInfo.Hash));

        public Bindable<WorkingBeatmap> BeatmapSource { get; private set; }

        private Playlist playlistView;
        private SearchTextBox searchTextBox;

        public PlaylistController(OsuGameBase osuGame, BeatmapDatabase beatmaps)
        {
            Width = 400;
            Height = 450;
            Masking = true;
            CornerRadius = 5;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("333333"),
                    Depth = float.MaxValue
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FlowDirection.VerticalOnly,
                    Padding = new MarginPadding(15),
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        searchTextBox = new SearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                        },
                        new SpriteText
                        {
                            Text = "TODO: Collections"
                        },
                        playlistView = new Playlist()
                    }
                }
            };
            searchTextBox.OnChange += (sender, newText) =>
            {
                if (newText)
                    playlistView.Filter(searchTextBox.Text);
            };

            Beatmaps = beatmaps;
            playlistView.Items = PlayList = beatmaps.GetAllWithChildren<BeatmapSetInfo>();
            BeatmapSource = osuGame.Beatmap ?? new Bindable<WorkingBeatmap>();
            playlistView.SelectionChanged += PlaylistView_SelectionChanged;
        }

        private void PlaylistView_SelectionChanged()
        {
            var newSelection = playlistView.SelectedItem;
            var differentBeatmapSet = BeatmapSource.Value == null || !BeatmapSource.Value.BeatmapInfo.AudioEquals(newSelection.Beatmaps[0]);
            if (differentBeatmapSet)
                BeatmapSource.Value = Beatmaps.GetWorkingBeatmap(newSelection.Beatmaps[0]);
        }

        protected override void LoadComplete()
        {
            BeatmapSource.ValueChanged += workingChanged;
            base.LoadComplete();
        }

        private void workingChanged(object sender, EventArgs e) => playlistView.SelectedItem = BeatmapSource.Value.BeatmapSetInfo;
    }
}
