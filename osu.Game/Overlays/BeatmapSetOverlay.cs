// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays
{
    public class BeatmapSetOverlay : WebOverlay
    {
        public const float X_PADDING = 40;
        public const float TOP_PADDING = 25;
        public const float RIGHT_WIDTH = 275;
        protected readonly BeatmapSetHeader Header;

        private RulesetStore rulesets;

        private readonly Bindable<BeatmapSetInfo> beatmapSet = new Bindable<BeatmapSetInfo>();
        private readonly OsuScrollContainer scroll;
        private readonly Info info;

        public BeatmapSetOverlay()
            : base(OverlayColourScheme.Blue)
        {
            Add(scroll = new OsuScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                Child = new ReverseChildIDFillFlowContainer<Drawable>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        Header = new BeatmapSetHeader(ColourScheme)
                        {
                            BeatmapSet = { BindTarget = beatmapSet }
                        },
                        info = new Info
                        {
                            BeatmapSet = { BindTarget = beatmapSet }
                        },
                        new ScoresContainer
                        {
                            Beatmap = { BindTarget = Header.HeaderContent.Picker.Beatmap }
                        }
                    },
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Header.HeaderContent.Picker.Beatmap.ValueChanged += b =>
            {
                info.Beatmap = b.NewValue;
                scroll.ScrollToStart();
            };
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            beatmapSet.Value = null;
        }

        public void FetchAndShowBeatmap(int beatmapId)
        {
            beatmapSet.Value = null;

            var req = new GetBeatmapSetRequest(beatmapId, BeatmapSetLookupType.BeatmapId);
            req.Success += res =>
            {
                beatmapSet.Value = res.ToBeatmapSet(rulesets);
                Header.HeaderContent.Picker.Beatmap.Value = beatmapSet.Value.Beatmaps.First(b => b.OnlineBeatmapID == beatmapId);
            };
            API.Queue(req);

            Show();
        }

        public void FetchAndShowBeatmapSet(int beatmapSetId)
        {
            beatmapSet.Value = null;

            var req = new GetBeatmapSetRequest(beatmapSetId);
            req.Success += res => beatmapSet.Value = res.ToBeatmapSet(rulesets);
            API.Queue(req);

            Show();
        }

        /// <summary>
        /// Show an already fully-populated beatmap set.
        /// </summary>
        /// <param name="set">The set to show.</param>
        public void ShowBeatmapSet(BeatmapSetInfo set)
        {
            beatmapSet.Value = set;
            Show();
        }
    }
}
