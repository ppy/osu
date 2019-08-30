// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play.HitErrorDisplay
{
    public class HitErrorDisplay : Container<HitErrorMeter>
    {
        private const int fade_duration = 200;
        private const int margin = 10;

        private readonly Bindable<ScoreMeterType> type = new Bindable<ScoreMeterType>();

        private HitWindows hitWindows;

        private readonly ScoreProcessor processor;

        public HitErrorDisplay(ScoreProcessor processor)
        {
            this.processor = processor;
            processor.NewJudgement += onNewJudgement;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            processor.NewJudgement -= onNewJudgement;
        }

        private void onNewJudgement(JudgementResult result)
        {
            foreach (var c in Children)
                c.OnNewJudgement(result);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, Bindable<WorkingBeatmap> workingBeatmap)
        {
            config.BindWith(OsuSetting.ScoreMeter, type);
            hitWindows = workingBeatmap.Value.Beatmap.HitObjects.First().HitWindows;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            type.BindValueChanged(typeChanged, true);
        }

        private void typeChanged(ValueChangedEvent<ScoreMeterType> type)
        {
            Children.ForEach(c => c.FadeOut(fade_duration, Easing.OutQuint));

            switch (type.NewValue)
            {
                case ScoreMeterType.HitErrorBoth:
                    createBar(false);
                    createBar(true);
                    break;

                case ScoreMeterType.HitErrorLeft:
                    createBar(false);
                    break;

                case ScoreMeterType.HitErrorRight:
                    createBar(true);
                    break;
            }
        }

        private void createBar(bool rightAligned)
        {
            var display = new BarHitErrorMeter(hitWindows, rightAligned)
            {
                Margin = new MarginPadding(margin),
                Anchor = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                Origin = rightAligned ? Anchor.CentreRight : Anchor.CentreLeft,
                Alpha = 0,
            };

            Add(display);
            display.FadeInFromZero(fade_duration, Easing.OutQuint);
        }
    }
}
