// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Screens.Evast
{
    public class BeatmapScreen : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap();
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();
        private readonly Vector2 backgroundBlur = new Vector2(20);

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmap.BindTo(game.Beatmap);
            beatmap.ValueChanged += OnBeatmapChange;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            beatmap.TriggerChange();
        }

        protected virtual void OnBeatmapChange(WorkingBeatmap beatmap)
        {
            var backgroundModeBeatmap = Background as BackgroundScreenBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurTo(backgroundBlur, 1000);
                backgroundModeBeatmap.FadeTo(1, 250);
            }
        }

        protected override bool OnExiting(Screen next)
        {
            beatmap.ValueChanged -= OnBeatmapChange;
            beatmap.UnbindAll();
            return base.OnExiting(next);
        }
    }
}
