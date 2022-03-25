// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Overlays.Comments;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class BeatmapSetOverlay : OnlineOverlay<BeatmapSetHeader>
    {
        public const float X_PADDING = 40;
        public const float Y_PADDING = 25;
        public const float RIGHT_WIDTH = 275;

        private readonly Bindable<APIBeatmapSet> beatmapSet = new Bindable<APIBeatmapSet>();

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

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

        protected override BeatmapSetHeader CreateHeader() => new BeatmapSetHeader();

        protected override Color4 BackgroundColour => ColourProvider.Background6;

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
                beatmapSet.Value = res;
                Header.HeaderContent.Picker.Beatmap.Value = Header.BeatmapSet.Value.Beatmaps.First(b => b.OnlineID == beatmapId);
            };
            API.Queue(req);

            Show();
        }

        public void FetchAndShowBeatmapSet(int beatmapSetId)
        {
            beatmapSet.Value = null;

            var req = new GetBeatmapSetRequest(beatmapSetId);
            req.Success += res => beatmapSet.Value = res;
            API.Queue(req);

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

        private class CommentsSection : BeatmapSetLayoutSection
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
