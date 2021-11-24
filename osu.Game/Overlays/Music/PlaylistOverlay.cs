// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public class PlaylistOverlay : VisibilityContainer
    {
        private const float transition_duration = 600;
        private const float playlist_height = 510;

        public IBindableList<BeatmapSetInfo> BeatmapSets => beatmapSets;

        private readonly BindableList<BeatmapSetInfo> beatmapSets = new BindableList<BeatmapSetInfo>();

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private FilterControl filter;
        private Playlist list;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, Bindable<WorkingBeatmap> beatmap)
        {
            this.beatmap.BindTo(beatmap);

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
                        list = new Playlist
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 95, Bottom = 10, Right = 10 },
                            RequestSelection = itemSelected
                        },
                        filter = new FilterControl
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            FilterChanged = criteria => list.Filter(criteria),
                            Padding = new MarginPadding(10),
                        },
                    },
                },
            };

            filter.Search.OnCommit += (sender, newText) =>
            {
                BeatmapInfo toSelect = list.FirstVisibleSet?.Beatmaps.FirstOrDefault();

                if (toSelect != null)
                {
                    beatmap.Value = beatmaps.GetWorkingBeatmap(toSelect);
                    beatmap.Value.Track.Restart();
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            list.Items.BindTo(beatmapSets);
            beatmap.BindValueChanged(working => list.SelectedSet.Value = working.NewValue.BeatmapSetInfo, true);
        }

        protected override void PopIn()
        {
            filter.Search.HoldFocus = true;
            Schedule(() => filter.Search.TakeFocus());

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
            if (set.Equals((beatmap.Value?.BeatmapSetInfo)))
            {
                beatmap.Value?.Track.Seek(0);
                return;
            }

            beatmap.Value = beatmaps.GetWorkingBeatmap(set.Beatmaps.First());
            beatmap.Value.Track.Restart();
        }
    }
}
