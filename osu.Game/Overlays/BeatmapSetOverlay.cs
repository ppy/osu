// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Rulesets;
using osu.Game.Overlays.BeatmapSet.Scores;

namespace osu.Game.Overlays
{
    public class BeatmapSetOverlay : WaveOverlayContainer
    {
        public const float X_PADDING = 40;
        public const float RIGHT_WIDTH = 275;

        private readonly Header header;
        private readonly Info info;
        private readonly ScoresContainer scores;

        private APIAccess api;
        private RulesetStore rulesets;
        private GetScoresRequest getScoresRequest;

        private readonly ScrollContainer scroll;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        public BeatmapSetOverlay()
        {
            FirstWaveColour = OsuColour.Gray(0.4f);
            SecondWaveColour = OsuColour.Gray(0.3f);
            ThirdWaveColour = OsuColour.Gray(0.2f);
            FourthWaveColour = OsuColour.Gray(0.1f);

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

            header.Picker.Beatmap.ValueChanged += b =>
            {
                info.Beatmap = b;
                updateScores(b);
            };
        }

        private void updateScores(BeatmapInfo beatmap)
        {
            getScoresRequest?.Cancel();

            if (!beatmap.OnlineBeatmapID.HasValue)
            {
                scores.CleanAllScores();
                return;
            }

            scores.IsLoading = true;

            getScoresRequest = new GetScoresRequest(beatmap, beatmap.Ruleset);
            getScoresRequest.Success += r =>
            {
                scores.Scores = r.Scores;
                scores.IsLoading = false;
            };
            api.Queue(getScoresRequest);
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, RulesetStore rulesets)
        {
            this.api = api;
            this.rulesets = rulesets;
        }

        protected override void PopIn()
        {
            base.PopIn();
            FadeEdgeEffectTo(0.25f, APPEAR_DURATION, Easing.In);
        }

        protected override void PopOut()
        {
            base.PopOut();
            header.Details.StopPreview();
            FadeEdgeEffectTo(0, DISAPPEAR_DURATION, Easing.Out);
        }

        protected override bool OnClick(InputState state)
        {
            State = Visibility.Hidden;
            return true;
        }

        public void ShowBeatmapSet(int beatmapSetId)
        {
            // todo: display the overlay while we are loading here. we need to support setting BeatmapSet to null for this to work.
            var req = new GetBeatmapSetRequest(beatmapSetId);
            req.Success += res => ShowBeatmapSet(res.ToBeatmapSet(rulesets));
            api.Queue(req);
        }

        public void ShowBeatmapSet(BeatmapSetInfo set)
        {
            header.BeatmapSet = info.BeatmapSet = set;
            Show();
            scroll.ScrollTo(0);
        }
    }
}
