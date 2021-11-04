// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class UnstableRateCounter : RollingCounter<double>, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        protected override bool IsRollingProportional => true;

        protected override double RollingDuration => 750;

        private const float alpha_when_invalid = 0.3f;

        [CanBeNull]
        [Resolved(CanBeNull = true)]
        private ScoreProcessor scoreProcessor { get; set; }

        [Resolved(CanBeNull = true)]
        [CanBeNull]
        private GameplayState gameplayState { get; set; }

        private readonly CancellationTokenSource loadCancellationSource = new CancellationTokenSource();

        public UnstableRateCounter()
        {
            Current.Value = DisplayedCount = 0.0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDifficultyCache difficultyCache)
        {
            Colour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (scoreProcessor != null)
            {
                scoreProcessor.NewJudgement += onJudgementChanged;
                scoreProcessor.JudgementReverted += onJudgementChanged;
            }
        }

        private bool isValid;

        protected bool IsValid
        {
            set
            {
                if (value == isValid)
                    return;

                isValid = value;
                DrawableCount.FadeTo(isValid ? 1 : alpha_when_invalid, 1000, Easing.OutQuint);
            }
        }

        private void onJudgementChanged(JudgementResult judgement)
        {

            if (gameplayState == null)
            {
                isValid = false;
                return;
            }

            double ur = new UnstableRate(gameplayState.Score.ScoreInfo.HitEvents).Value;
            if (double.IsNaN(ur)) // Error handling: If the user misses the first few notes, the UR is NaN.
            {
                isValid = false;
                return;
            }
            Current.Value = ur;
            IsValid = true;
        }

        protected override LocalisableString FormatCount(double count)
        {
            return count.ToLocalisableString("0.00 UR");
        }


        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => {
                s.Font = s.Font.With(size: 12f, fixedWidth: true);
                s.Alpha = alpha_when_invalid;
            });



        

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor != null)
                scoreProcessor.NewJudgement -= onJudgementChanged;

            loadCancellationSource?.Cancel();
        }


    }
}
