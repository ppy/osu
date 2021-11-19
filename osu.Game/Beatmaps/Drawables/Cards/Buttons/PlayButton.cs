// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public class PlayButton : BeatmapCardIconButton
    {
        private readonly APIBeatmapSet beatmapSet;
        private readonly Bindable<DownloadState> downloadState = new Bindable<DownloadState>();

        public PlayButton(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;

            Icon.Icon = FontAwesome.Solid.AngleDoubleRight;
            TooltipText = "Go to beatmap";
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame? game, BeatmapDownloadTracker downloadTracker)
        {
            Action = () => game?.PresentBeatmap(beatmapSet);

            ((IBindable<DownloadState>)downloadState).BindTo(downloadTracker.State);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            downloadState.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            this.FadeTo(downloadState.Value == DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
