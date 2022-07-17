// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedProfileScoreSubsection : ProfileSubsection
    {
        /// <summary>
        /// The number of items displayed per page.
        /// </summary>
        protected virtual int ItemsPerPage => 50;

        /// <summary>
        /// The number of items displayed initially.
        /// </summary>
        protected virtual int InitialItemsCount => 5;

        [Resolved]
        private IAPIProvider api { get; set; }

        protected PaginationParameters? CurrentPage { get; private set; }

        protected ReverseChildIDFillFlowContainer<Drawable> ItemsContainer { get; private set; }

        private APIRequest<List<SoloScoreInfo>> scoreRetrievalRequest;
        private APIRequest<GetBeatmapsResponse> beatmapRetrievalRequest;
        private CancellationTokenSource loadCancellation;

        protected List<SoloScoreInfo> CurrentScores { get; private set; } = new List<SoloScoreInfo>();

        private ShowMoreButton moreButton;
        private OsuSpriteText missing;
        private readonly LocalisableString? missingText;

        protected PaginatedProfileScoreSubsection(Bindable<APIUser> user, LocalisableString? headerText = null, LocalisableString? missingText = null)
            : base(user, headerText, CounterVisibilityState.AlwaysVisible)
        {
            this.missingText = missingText;
        }

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                // reverse ID flow is required for correct Z-ordering of the items (last item should be front-most).
                // particularly important in PaginatedBeatmapContainer, as it uses beatmap cards, which have expandable overhanging content.
                ItemsContainer = new ReverseChildIDFillFlowContainer<Drawable>
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(0, 2),
                    // ensure the container and its contents are in front of the "more" button.
                    Depth = float.MinValue
                },
                moreButton = new ShowMoreButton
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Alpha = 0,
                    Margin = new MarginPadding { Top = 10 },
                    Action = showMore,
                },
                missing = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 15),
                    Text = missingText ?? string.Empty,
                    Alpha = 0,
                }
            }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(onUserChanged, true);
        }

        private void onUserChanged(ValueChangedEvent<APIUser> e)
        {
            loadCancellation?.Cancel();
            scoreRetrievalRequest?.Cancel();
            beatmapRetrievalRequest?.Cancel();

            CurrentPage = null;
            ItemsContainer.Clear();
            CurrentScores.Clear();

            if (e.NewValue != null)
            {
                showMore();
                SetCount(GetCount(e.NewValue));
            }
        }

        private void showMore()
        {
            loadCancellation = new CancellationTokenSource();

            CurrentPage = CurrentPage?.TakeNext(ItemsPerPage) ?? new PaginationParameters(InitialItemsCount);

            scoreRetrievalRequest = CreateScoreRequest(CurrentPage.Value);
            scoreRetrievalRequest.Success += requestBeatmaps;

            api.Queue(scoreRetrievalRequest);
        }

        private void requestBeatmaps(List<SoloScoreInfo> items)
        {
            CurrentScores = items;

            beatmapRetrievalRequest = CreateBeatmapsRequest(items);
            beatmapRetrievalRequest.Success += UpdateItems;

            api.Queue(beatmapRetrievalRequest);
        }

        protected virtual APIRequest<GetBeatmapsResponse> CreateBeatmapsRequest(List<SoloScoreInfo> items) => new GetBeatmapsRequest(items.Select(i => i.BeatmapID).ToArray());

        protected virtual void UpdateItems(GetBeatmapsResponse beatmaps) => Schedule(() =>
        {
            var scoreBeatmapPairs = new List<Tuple<SoloScoreInfo, APIBeatmap>>();

            foreach (var score in CurrentScores)
            {
                var beatmap = beatmaps.Beatmaps.Find(m => m.OnlineID == score.BeatmapID);
                scoreBeatmapPairs.Add(new Tuple<SoloScoreInfo, APIBeatmap>(score, beatmap));
            }

            OnItemsReceived(scoreBeatmapPairs);

            if (!scoreBeatmapPairs.Any() && CurrentPage?.Offset == 0)
            {
                moreButton.Hide();
                moreButton.IsLoading = false;

                if (missingText.HasValue)
                    missing.Show();

                return;
            }

            LoadComponentsAsync(scoreBeatmapPairs.Select(CreateDrawableItem).Where(d => d != null), drawables =>
            {
                missing.Hide();

                moreButton.FadeTo(scoreBeatmapPairs.Count == CurrentPage?.Limit ? 1 : 0);
                moreButton.IsLoading = false;

                ItemsContainer.AddRange(drawables);
            }, loadCancellation.Token);
        });

        protected virtual int GetCount(APIUser user) => 0;

        protected virtual void OnItemsReceived(List<Tuple<SoloScoreInfo, APIBeatmap>> scoreBeatmapPairs)
        {
        }

        protected abstract APIRequest<List<SoloScoreInfo>> CreateScoreRequest(PaginationParameters pagination);

        protected abstract Drawable CreateDrawableItem(Tuple<SoloScoreInfo, APIBeatmap> scoreBeatmapPair);

        protected override void Dispose(bool isDisposing)
        {
            scoreRetrievalRequest?.Cancel();
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
