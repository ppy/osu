// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
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

        private BarHitErrorDisplay leftDisplay;

        private BarHitErrorDisplay rightDisplay;

        public HitErrorDisplayOverlay(ScoreProcessor processor)
        {
            this.processor = processor;
            hitWindows = processor.CreateHitWindows();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, Bindable<WorkingBeatmap> workingBeatmap)
        {
            config.BindWith(OsuSetting.ScoreMeter, type);
            hitWindows.SetDifficulty(workingBeatmap.Value.BeatmapInfo.BaseDifficulty.OverallDifficulty);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            type.BindValueChanged(typeChanged, true);
        }

        private void typeChanged(ValueChangedEvent<ScoreMeterType> type)
        {
            switch (type.NewValue)
            {
                case ScoreMeterType.None:
                    removeLeftDisplay();
                    removeRightDisplay();
                    break;

                case ScoreMeterType.HitErrorBoth:
                    addLeftDisplay();
                    addRightDisplay();
                    break;

                case ScoreMeterType.HitErrorLeft:
                    addLeftDisplay();
                    removeRightDisplay();
                    break;

                case ScoreMeterType.HitErrorRight:
                    addRightDisplay();
                    removeLeftDisplay();
                    break;
            }
        }

        private void addLeftDisplay()
        {
            if (leftDisplay != null)
                return;

            leftDisplay = createNew();
        }

        private void addRightDisplay()
        {
            if (rightDisplay != null)
                return;

            rightDisplay = createNew(true);
        }

        private void removeRightDisplay()
        {
            if (rightDisplay == null)
                return;

            processor.NewJudgement -= rightDisplay.OnNewJudgement;

            rightDisplay.FadeOut(fade_duration, Easing.OutQuint).Expire();
            rightDisplay = null;
        }

        private void removeLeftDisplay()
        {
            if (leftDisplay == null)
                return;

            processor.NewJudgement -= leftDisplay.OnNewJudgement;

            leftDisplay.FadeOut(fade_duration, Easing.OutQuint).Expire();
            leftDisplay = null;
        }

        private BarHitErrorDisplay createNew(bool reversed = false)
        {
            var display = new BarHitErrorDisplay(hitWindows, reversed)
            {
                Margin = new MarginPadding(margin),
                Anchor = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                Origin = reversed ? Anchor.CentreRight : Anchor.CentreLeft,
                Alpha = 0,
            };

            processor.NewJudgement += display.OnNewJudgement;
            Add(display);
            display.FadeInFromZero(fade_duration, Easing.OutQuint);
            return display;
        }
    }
}
