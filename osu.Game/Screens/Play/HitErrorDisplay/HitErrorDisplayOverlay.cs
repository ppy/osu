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
using System.Linq;

namespace osu.Game.Screens.Play.HitErrorDisplay
{
    public class HitErrorDisplayOverlay : Container<HitErrorDisplay>
    {
        private const int fade_duration = 200;
        private const int margin = 10;

        private readonly Bindable<ScoreMeterType> type = new Bindable<ScoreMeterType>();

        public HitErrorDisplayOverlay(ScoreProcessor processor, WorkingBeatmap workingBeatmap)
        {
            float overallDifficulty = workingBeatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty;

            RelativeSizeAxes = Axes.Both;
            Children = new[]
            {
                new DefaultHitErrorDisplay(overallDifficulty, processor.CreateHitWindows())
                {
                    Margin = new MarginPadding { Left = margin }
                },
                new DefaultHitErrorDisplay(overallDifficulty, processor.CreateHitWindows(), true)
                {
                    Margin = new MarginPadding { Right = margin }
                },
            };

            Children.ForEach(t => processor.NewJudgement += t.OnNewJudgement);
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
            switch (type.NewValue)
            {
                case ScoreMeterType.None:
                    InternalChildren.ForEach(t => t.FadeOut(fade_duration, Easing.OutQuint));
                    break;

                default:
                    InternalChildren.ForEach(t => t.FadeIn(fade_duration, Easing.OutQuint));
                    break;
            }
        }
    }
}
