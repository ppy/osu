using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Screens.Select;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistController : OverlayContainer
    {
        public BeatmapDatabase Beatmaps { get; private set; }
        public List<BeatmapSetInfo> PlayList { get; private set; }
        public int PlayListIndex => PlayList.IndexOf(PlayList.Single(pi => pi.Hash == BeatmapSource.Value?.BeatmapSetInfo.Hash));

        public Bindable<WorkingBeatmap> BeatmapSource { get; private set; }

        private Playlist playlistView;
        private SearchTextBox searchTextBox;
        private Container dragContainer;

        public PlaylistController()
        {
            Width = 400;
            Height = 450;
            Margin = new MarginPadding(10);

            Children = new Drawable[]
            {
                dragContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    CornerRadius = 5,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(40),
                        Radius = 5,
                    },
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes=Axes.Both,
                            Colour = OsuColour.FromHex("333333"),
                            Depth = float.MaxValue
                        },
                        new FlowContainer
                        {
                            RelativeSizeAxes=Axes.Both,
                            Direction = FlowDirection.VerticalOnly,
                            Padding = new MarginPadding(15),
                            Spacing = new Vector2(0,5),
                            Children = new Drawable[]
                            {
                                searchTextBox = new SearchTextBox
                                {
                                RelativeSizeAxes=Axes.X,
                                Height = 40,
                                },
                                new SpriteText {Text = "TODO: Collections" },
                                playlistView = new Playlist()
                            }
                        }
                    }
                }
            };
            searchTextBox.OnChange += (TextBox sender, bool newText) =>
            {
                if (newText)
                    playlistView.Filter(searchTextBox.Text);
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame, BeatmapDatabase beatmaps)
        {
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

        protected override void PopIn() => FadeIn(100);

        protected override void PopOut() => FadeOut(100);

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            Vector2 change = state.Mouse.Position - state.Mouse.PositionMouseDown.Value;

            // Diminish the drag distance as we go further to simulate "rubber band" feeling.
            change *= (float)Math.Pow(change.Length, 0.7f) / change.Length;

            dragContainer.MoveTo(change);
            return base.OnDrag(state);
        }

        protected override bool OnDragEnd(InputState state)
        {
            dragContainer.MoveTo(Vector2.Zero, 800, EasingTypes.OutElastic);
            return base.OnDragEnd(state);
        }
    }
}
