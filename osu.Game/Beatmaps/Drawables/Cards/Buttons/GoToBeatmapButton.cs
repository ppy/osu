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
        private readonly bool allowNavigationToBeatmap;

        public GoToBeatmapButton(APIBeatmapSet beatmapSet, bool allowNavigationToBeatmap)
        {
            this.beatmapSet = beatmapSet;
            this.allowNavigationToBeatmap = allowNavigationToBeatmap;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame? game)
        {
            Action = () => game?.PresentBeatmap(beatmapSet);
            Icon.Icon = FontAwesome.Solid.AngleDoubleRight;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            state.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        private void updateState()
        {
            bool available = state.Value == DownloadState.LocallyAvailable;
            Enabled.Value = allowNavigationToBeatmap && available;

            float alpha;

            if (available && allowNavigationToBeatmap)
            {
                TooltipText = "Go to beatmap";
                Enabled.Value = true;
                alpha = 1f;
            }
            else if (available)
            {
                TooltipText = string.Empty;
                Enabled.Value = false;
                alpha = 0.3f;
            }
            else
            {
                TooltipText = string.Empty;
                Enabled.Value = false;
                alpha = 0;
            }

            this.FadeTo(alpha, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
