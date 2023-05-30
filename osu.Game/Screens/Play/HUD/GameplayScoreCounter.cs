// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class GameplayScoreCounter : ScoreCounter
    {
        private Bindable<ScoringMode> scoreDisplayMode = null!;

        private Bindable<long> totalScoreBindable = null!;

        protected GameplayScoreCounter()
            : base(6)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, ScoreProcessor scoreProcessor)
        {
            totalScoreBindable = scoreProcessor.TotalScore.GetBoundCopy();
            totalScoreBindable.BindValueChanged(_ => updateDisplayScore());

            scoreDisplayMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
            scoreDisplayMode.BindValueChanged(scoreMode =>
            {
                switch (scoreMode.NewValue)
                {
                    case ScoringMode.Standardised:
                        RequiredDisplayDigits.Value = 6;
                        break;

                    case ScoringMode.Classic:
                        RequiredDisplayDigits.Value = 8;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(scoreMode));
                }

                updateDisplayScore();
            }, true);

            void updateDisplayScore() => Current.Value = scoreProcessor.GetDisplayScore(scoreDisplayMode.Value);
        }
    }
}
