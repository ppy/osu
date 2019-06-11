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
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays
{
    public class BeatmapSetOverlay : FullscreenOverlay
    {
        private const int fade_duration = 300;

        public const float X_PADDING = 40;
        public const float RIGHT_WIDTH = 275;

        private readonly Header header;

        private RulesetStore rulesets;

        private readonly ScrollContainer scroll;

        private readonly Bindable<BeatmapSetInfo> beatmapSet = new Bindable<BeatmapSetInfo>();

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public BeatmapSetOverlay()
        {
            Info info;
            ScoresContainer scores;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                scroll = new ScrollContainer
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
                            header = new Header(),
                            info = new Info(),
                            scores = new ScoresContainer(),
                        },
                    },
                },
            };

            header.BeatmapSet.BindTo(beatmapSet);
            info.BeatmapSet.BindTo(beatmapSet);

            header.Picker.Beatmap.ValueChanged += b =>
            {
                info.Beatmap = b.NewValue;
                scores.Beatmap = b.NewValue;
            };
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
                header.Picker.Beatmap.Value = header.BeatmapSet.Value.Beatmaps.First(b => b.OnlineBeatmapID == beatmapId);
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

        public void ShowBeatmapSet(BeatmapSetInfo set)
        {
            beatmapSet.Value = set;
            Show();
            scroll.ScrollTo(0);
        }
    }
}
