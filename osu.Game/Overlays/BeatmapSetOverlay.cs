// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
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
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class BeatmapSetOverlay : WaveOverlayContainer
    {
        private const int fade_duration = 300;

        public const float X_PADDING = 40;
        public const float RIGHT_WIDTH = 275;

        private readonly Header header;

        private IAPIProvider api;
        private RulesetStore rulesets;

        private readonly ScrollContainer scroll;

        private readonly Bindable<BeatmapSetInfo> beatmapSet = new Bindable<BeatmapSetInfo>();

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public BeatmapSetOverlay()
        {
            Info info;
            ScoresContainer scores;
            Waves.FirstWaveColour = OsuColour.Gray(0.4f);
            Waves.SecondWaveColour = OsuColour.Gray(0.3f);
            Waves.ThirdWaveColour = OsuColour.Gray(0.2f);
            Waves.FourthWaveColour = OsuColour.Gray(0.1f);

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.Both;
            Width = 0.85f;

            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0),
                Type = EdgeEffectType.Shadow,
                Radius = 3,
                Offset = new Vector2(0f, 1f),
            };

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
        private void load(IAPIProvider api, RulesetStore rulesets)
        {
            this.api = api;
            this.rulesets = rulesets;
        }

        protected override void PopIn()
        {
            base.PopIn();
            FadeEdgeEffectTo(0.25f, WaveContainer.APPEAR_DURATION, Easing.In);
        }

        protected override void PopOut()
        {
            base.PopOut();
            FadeEdgeEffectTo(0, WaveContainer.DISAPPEAR_DURATION, Easing.Out).OnComplete(_ => beatmapSet.Value = null);
        }

        protected override bool OnClick(ClickEvent e)
        {
            State = Visibility.Hidden;
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
            api.Queue(req);
            Show();
        }

        public void FetchAndShowBeatmapSet(int beatmapSetId)
        {
            beatmapSet.Value = null;
            var req = new GetBeatmapSetRequest(beatmapSetId);
            req.Success += res => beatmapSet.Value = res.ToBeatmapSet(rulesets);
            api.Queue(req);
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
