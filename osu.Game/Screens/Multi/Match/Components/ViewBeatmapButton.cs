// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class ViewBeatmapButton : HeaderButton
    {
        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [Resolved(CanBeNull = true)]
        private OsuGame osuGame { get; set; }

        public ViewBeatmapButton()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(200, 1);

            Text = "View beatmap";
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (osuGame != null)
                Beatmap.BindValueChanged(beatmap => updateAction(beatmap.NewValue), true);
        }

        private void updateAction(BeatmapInfo beatmap)
        {
            if (beatmap == null)
            {
                Enabled.Value = false;
                return;
            }

            Action = () => osuGame.ShowBeatmap(beatmap.OnlineBeatmapID ?? 0);
            Enabled.Value = true;
        }
    }
}
