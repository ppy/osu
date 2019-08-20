// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play.HitErrorDisplay
{
    public class HitErrorDisplayOverlay : Container<HitErrorDisplay>
    {
        private const int fade_duration = 200;
        private const int margin = 10;

        private readonly Bindable<ScoreMeterType> type = new Bindable<ScoreMeterType>();
        private readonly HitWindows hitWindows;
        private readonly ScoreProcessor processor;
        private readonly float overallDifficulty;

        public HitErrorDisplayOverlay(ScoreProcessor processor, WorkingBeatmap workingBeatmap)
        {
            this.processor = processor;

            overallDifficulty = workingBeatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty;
            hitWindows = processor.CreateHitWindows();

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ScoreMeter, type);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            type.BindValueChanged(onTypeChanged, true);
        }

        private void onTypeChanged(ValueChangedEvent<ScoreMeterType> type)
        {
            clear();

            switch (type.NewValue)
            {
                case ScoreMeterType.None:
                    break;

                case ScoreMeterType.HitErrorBoth:
                    createNew();
                    createNew(true);
                    break;

                case ScoreMeterType.HitErrorLeft:
                    createNew();
                    break;

                case ScoreMeterType.HitErrorRight:
                    createNew(true);
                    break;
            }
        }

        private void clear()
        {
            Children.ForEach(t =>
            {
                processor.NewJudgement -= t.OnNewJudgement;
                t.FadeOut(fade_duration, Easing.OutQuint).Expire();
            });
        }

        private void createNew(bool reversed = false)
        {
            var display = new BarHitErrorDisplay(overallDifficulty, hitWindows, reversed)
            {
                Margin = new MarginPadding(margin),
                Anchor = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                Origin = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                Alpha = 0,
            };

            processor.NewJudgement += display.OnNewJudgement;
            Add(display);
            display.FadeInFromZero(fade_duration, Easing.OutQuint);
        }
    }
}
