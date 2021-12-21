// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    public abstract class BeatmapCardBase : OsuClickableContainer
    {
        public const float TRANSITION_DURATION = 400;
        public const float CORNER_RADIUS = 10;

        public IBindable<bool> Expanded { get; }

        protected readonly APIBeatmapSet BeatmapSet;
        protected readonly Bindable<BeatmapSetFavouriteState> FavouriteState;

        protected abstract Drawable IdleContent { get; }
        protected abstract Drawable DownloadInProgressContent { get; }

        protected readonly BeatmapDownloadTracker DownloadTracker;

        protected BeatmapCardBase(APIBeatmapSet beatmapSet, bool allowExpansion = true)
            : base(HoverSampleSet.Submit)
        {
            Expanded = new BindableBool { Disabled = !allowExpansion };

            BeatmapSet = beatmapSet;
            FavouriteState = new Bindable<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(beatmapSet.HasFavourited, beatmapSet.FavouriteCount));
            DownloadTracker = new BeatmapDownloadTracker(beatmapSet);
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapSetOverlay? beatmapSetOverlay)
        {
            Action = () => beatmapSetOverlay?.FetchAndShowBeatmapSet(BeatmapSet.OnlineID);

            AddInternal(DownloadTracker);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DownloadTracker.State.BindValueChanged(_ => UpdateState());
            Expanded.BindValueChanged(_ => UpdateState(), true);
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateState();
            base.OnHoverLost(e);
        }

        protected virtual void UpdateState()
        {
            bool showProgress = DownloadTracker.State.Value == DownloadState.Downloading || DownloadTracker.State.Value == DownloadState.Importing;

            IdleContent.FadeTo(showProgress ? 0 : 1, TRANSITION_DURATION, Easing.OutQuint);
            DownloadInProgressContent.FadeTo(showProgress ? 1 : 0, TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
