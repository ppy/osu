// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays
{
    public class BeatmapSetOverlay : FullscreenOverlay
    {
        public const float X_PADDING = 40;
        public const float TOP_PADDING = 25;
        public const float RIGHT_WIDTH = 275;
        protected readonly Header Header;

        private RulesetStore rulesets;
        private readonly ScoresContainer scores;
        private GetScoresRequest getScoresRequest;

        private readonly Bindable<BeatmapSetInfo> beatmapSet = new Bindable<BeatmapSetInfo>();

        [Resolved]
        private IAPIProvider api { get; set; }

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public BeatmapSetOverlay()
        {
            OsuScrollContainer scroll;
            Info info;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                scroll = new OsuScrollContainer
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
                            Header = new Header(),
                            info = new Info(),
                            scores = new ScoresContainer(),
                        },
                    },
                },
            };

            Header.BeatmapSet.BindTo(beatmapSet);
            info.BeatmapSet.BindTo(beatmapSet);

            Header.Picker.Beatmap.ValueChanged += b =>
            {
                info.Beatmap = b.NewValue;
                getScores(b.NewValue);

                scroll.ScrollToStart();
            };
        }

        private void getScores(BeatmapInfo b)
        {
            getScoresRequest?.Cancel();

            if (b?.OnlineBeatmapID.HasValue != true)
            {
                scores.Scores = null;
                return;
            }

            scores.Loading = true;

            getScoresRequest = new GetScoresRequest(b, b.Ruleset);
            getScoresRequest.Success += r => Schedule(() =>
            {
                scores.Scores = r;
                scores.Loading = false;
            });
            api.Queue(getScoresRequest);
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            this.rulesets = rulesets;
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            beatmapSet.Value = null;
        }

        protected override bool OnClick(ClickEvent e)
        {
            Hide();
            return true;
        }

        public void FetchAndShowBeatmap(int beatmapId)
        {
            beatmapSet.Value = null;

            var req = new GetBeatmapSetRequest(beatmapId, BeatmapSetLookupType.BeatmapId);
            req.Success += res =>
            {
                beatmapSet.Value = res.ToBeatmapSet(rulesets);
                Header.Picker.Beatmap.Value = Header.BeatmapSet.Value.Beatmaps.First(b => b.OnlineBeatmapID == beatmapId);
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
