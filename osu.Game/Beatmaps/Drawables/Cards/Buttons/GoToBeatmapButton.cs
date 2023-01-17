// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public partial class GoToBeatmapButton : BeatmapCardIconButton
    {
        public IBindable<DownloadState> State => state;
        private readonly Bindable<DownloadState> state = new Bindable<DownloadState>();

        private readonly APIBeatmapSet beatmapSet;

        public GoToBeatmapButton(APIBeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;

            Icon.Icon = FontAwesome.Solid.AngleDoubleRight;
            TooltipText = "Go to beatmap";
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame? game)
        {
            Action = () => game?.PresentBeatmap(beatmapSet);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            state.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            this.FadeTo(state.Value == DownloadState.LocallyAvailable ? 1 : 0, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
