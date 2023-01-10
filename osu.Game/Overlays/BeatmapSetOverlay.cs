// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Overlays.Comments;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Details;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class BeatmapSetOverlay : OnlineOverlay<BeatmapSetHeader>
    {
        public const float X_PADDING = 40;
        public const float Y_PADDING = 25;
        public const float RIGHT_WIDTH = 275;

        private readonly Bindable<APIBeatmapSet> beatmapSet = new Bindable<APIBeatmapSet>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private IBindable<APIUser> apiUser;

        private int? lastRequestedBeatmapId;
        private BeatmapSetLookupType? lastBeatmapSetLookupType;

        /// <remarks>
        /// Isolates the beatmap set overlay from the game-wide selected mods bindable
        /// to avoid affecting the beatmap details section (i.e. <see cref="AdvancedStats.StatisticRow"/>).
        /// </remarks>
        [Cached]
        [Cached(typeof(IBindable<IReadOnlyList<Mod>>))]
        protected readonly Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public BeatmapSetOverlay()
            : base(OverlayColourScheme.Blue)
        {
            Info info;
            CommentsSection comments;

            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    info = new Info(),
                    new ScoresContainer
                    {
                        Beatmap = { BindTarget = Header.HeaderContent.Picker.Beatmap }
                    },
                    comments = new CommentsSection()
                }
            };

            Header.BeatmapSet.BindTo(beatmapSet);
            info.BeatmapSet.BindTo(beatmapSet);
            comments.BeatmapSet.BindTo(beatmapSet);

            Header.HeaderContent.Picker.Beatmap.ValueChanged += b =>
            {
                info.BeatmapInfo = b.NewValue;
                ScrollFlow.ScrollToStart();
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            apiUser = api.LocalUser.GetBoundCopy();
            apiUser.BindValueChanged(_ => Schedule(() =>
            {
                if (api.IsLoggedIn)
                    fetchAndSetLastRequestedBeatmap();
            }));
        }

        protected override BeatmapSetHeader CreateHeader() => new BeatmapSetHeader();

        protected override Color4 BackgroundColour => ColourProvider.Background6;

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            beatmapSet.Value = null;
        }

        public void FetchAndShowBeatmap(int beatmapId)
        {
            lastRequestedBeatmapId = beatmapId;
            lastBeatmapSetLookupType = BeatmapSetLookupType.BeatmapId;

            beatmapSet.Value = null;

            fetchAndSetBeatmap(beatmapId);

            Show();
        }

        public void FetchAndShowBeatmapSet(int beatmapSetId)
        {
            lastRequestedBeatmapId = beatmapSetId;
            lastBeatmapSetLookupType = BeatmapSetLookupType.SetId;

            beatmapSet.Value = null;

            fetchAndSetBeatmapSet(beatmapSetId);

            Show();
        }

        /// <summary>
        /// Show an already fully-populated beatmap set.
        /// </summary>
        /// <param name="set">The set to show.</param>
        public void ShowBeatmapSet(APIBeatmapSet set)
        {
            beatmapSet.Value = set;
            Show();
        }

        private void fetchAndSetBeatmap(int beatmapId)
        {
            if (!api.IsLoggedIn)
                return;

            var req = new GetBeatmapSetRequest(beatmapId, BeatmapSetLookupType.BeatmapId);
            req.Success += res =>
            {
                beatmapSet.Value = res;
                Header.HeaderContent.Picker.Beatmap.Value = Header.BeatmapSet.Value.Beatmaps.First(b => b.OnlineID == beatmapId);
            };
            API.Queue(req);
        }

        private void fetchAndSetBeatmapSet(int beatmapSetId)
        {
            if (!api.IsLoggedIn)
                return;

            var req = new GetBeatmapSetRequest(beatmapSetId);
            req.Success += res => beatmapSet.Value = res;
            API.Queue(req);
        }

        private void fetchAndSetLastRequestedBeatmap()
        {
            if (lastRequestedBeatmapId == null)
                return;

            switch (lastBeatmapSetLookupType)
            {
                case BeatmapSetLookupType.SetId:
                    fetchAndSetBeatmapSet(lastRequestedBeatmapId.Value);
                    break;

                case BeatmapSetLookupType.BeatmapId:
                    fetchAndSetBeatmap(lastRequestedBeatmapId.Value);
                    break;
            }
        }

        private partial class CommentsSection : BeatmapSetLayoutSection
        {
            public readonly Bindable<APIBeatmapSet> BeatmapSet = new Bindable<APIBeatmapSet>();

            public CommentsSection()
            {
                CommentsContainer comments;

                Add(comments = new CommentsContainer());

                BeatmapSet.BindValueChanged(beatmapSet =>
                {
                    if (beatmapSet.NewValue?.OnlineID > 0)
                    {
                        Show();
                        comments.ShowComments(CommentableType.Beatmapset, beatmapSet.NewValue.OnlineID);
                    }
                    else
                    {
                        Hide();
                    }
                }, true);
            }
        }
    }
}
