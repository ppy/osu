// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using Realms;

namespace osu.Game.Overlays.Music
{
    [Cached]
    public partial class PlaylistOverlay : VisibilityContainer
    {
        public Bindable<Live<BeatmapSetInfo>?> SelectedSet = new Bindable<Live<BeatmapSetInfo>?>();

        private const float transition_duration = 600;
        public const float PLAYLIST_HEIGHT = 510;

        private readonly BindableList<Live<BeatmapSetInfo>> beatmapSets = new BindableList<Live<BeatmapSetInfo>>();

        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? beatmapSubscription;

        private Playlist list = null!;

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
                            Padding = new MarginPadding { Vertical = 10, Right = 10 },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapSubscription = realm.RegisterForNotifications(r => r.All<BeatmapSetInfo>().Where(s => !s.DeletePending && !s.Protected), beatmapsChanged);

            list.RowData.BindTo(beatmapSets);
            beatmap.BindValueChanged(working => SelectedSet.Value = working.NewValue.BeatmapSetInfo.ToLive(realm), true);
        }

        private void beatmapsChanged(IRealmCollection<BeatmapSetInfo> sender, ChangeSet? changes)
        {
            if (changes == null)
            {
                beatmapSets.Clear();
                // must use AddRange to avoid RearrangeableList sort overhead per add op.
                beatmapSets.AddRange(sender.Select(b => b.ToLive(realm)));
                return;
            }

            foreach (int i in changes.InsertedIndices)
                beatmapSets.Insert(i, sender[i].ToLive(realm));

            foreach (int i in changes.DeletedIndices.OrderDescending())
                beatmapSets.RemoveAt(i);
        }

        protected override void PopIn()
        {
            this.ResizeTo(new Vector2(1, RelativeSizeAxes.HasFlag(Axes.Y) ? 1f : PLAYLIST_HEIGHT), transition_duration, Easing.OutQuint);
            this.FadeIn(transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.ResizeTo(new Vector2(1, 0), transition_duration, Easing.OutQuint);
            this.FadeOut(transition_duration);
        }

        public void ItemSelected(Live<BeatmapSetInfo> beatmapSet)
        {
            beatmapSet.PerformRead(set =>
            {
                if (set.Equals((beatmap.Value?.BeatmapSetInfo)))
                {
                    beatmap.Value?.Track.Seek(0);
                    return;
                }

                beatmap.Value = beatmaps.GetWorkingBeatmap(set.Beatmaps.First());
                beatmap.Value.Track.Restart();
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapSubscription?.Dispose();
        }
    }
}
