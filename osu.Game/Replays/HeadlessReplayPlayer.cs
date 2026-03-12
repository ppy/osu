// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;

namespace osu.Game.Replays
{
    public partial class HeadlessReplayPlayer : CompositeDrawable
    {
        public Action<ScoreInfo>? PlaybackCompleted;

        private readonly Score score;
        private readonly IBeatmap playableBeatmap;
        private readonly Ruleset ruleset;

        private readonly ManualAdjustableClock clock = new ManualAdjustableClock();

        private DrawableRuleset drawableRuleset = null!;
        private GameplayClockContainer gameplayClockContainer = null!;

        private ScoreProcessor scoreProcessor = null!;

        private HealthProcessor healthProcessor = null!;

        private DependencyContainer dependencies = null!;

        public HeadlessReplayPlayer(Score score, IWorkingBeatmap workingBeatmap)
        {
            this.score = score;

            ruleset = workingBeatmap.BeatmapInfo.Ruleset.CreateInstance();
            playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, score.ScoreInfo.Mods);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            drawableRuleset = ruleset.CreateDrawableRulesetWith(playableBeatmap, score.ScoreInfo.Mods);
            dependencies.CacheAs(drawableRuleset);

            if (drawableRuleset is IDrawableScrollingRuleset scrollingRuleset)
                dependencies.CacheAs(scrollingRuleset.ScrollingInfo);

            scoreProcessor = ruleset.CreateScoreProcessor();
            scoreProcessor.Mods.Value = score.ScoreInfo.Mods;
            scoreProcessor.ApplyBeatmap(playableBeatmap);

            dependencies.CacheAs(scoreProcessor);

            healthProcessor =
                score.ScoreInfo.Mods.OfType<IApplicableHealthProcessor>().FirstOrDefault()?.CreateHealthProcessor(playableBeatmap.HitObjects[0].StartTime)
                ?? ruleset.CreateHealthProcessor(playableBeatmap.HitObjects[0].StartTime);
            healthProcessor.ApplyBeatmap(playableBeatmap);

            dependencies.CacheAs(healthProcessor);

            var rulesetSkinProvider = new HeadlessRulesetSkinProvidingContainer(ruleset, playableBeatmap, null);
            InternalChild = gameplayClockContainer = new GameplayClockContainer(clock, false, false);

            gameplayClockContainer.Add(rulesetSkinProvider);
            rulesetSkinProvider.Add(createGameplayComponents());

            dependencies.CacheAs(drawableRuleset.FrameStableClock);
            dependencies.CacheAs<IGameplayClock>(drawableRuleset.FrameStableClock);

            drawableRuleset.NewResult += result =>
            {
                healthProcessor.ApplyResult(result);
                scoreProcessor.ApplyResult(result);
            };

            scoreProcessor.OnLoadComplete += _ =>
            {
                foreach (var mod in score.ScoreInfo.Mods.OfType<IApplicableToScoreProcessor>())
                    mod.ApplyToScoreProcessor(scoreProcessor);
            };

            healthProcessor.OnLoadComplete += _ =>
            {
                foreach (var mod in score.ScoreInfo.Mods.OfType<IApplicableToHealthProcessor>())
                    mod.ApplyToHealthProcessor(healthProcessor);
            };

            scoreProcessor.HasCompleted.BindValueChanged(_ => checkScoreCompleted());
            healthProcessor.Failed += onFail;
        }

        private Drawable createGameplayComponents() => new ScalingContainer(ScalingMode.Gameplay)
        {
            Children = new Drawable[]
            {
                drawableRuleset.With(r =>
                    r.FrameStableComponents.Children = new Drawable[]
                    {
                        scoreProcessor,
                        healthProcessor,
                    }),
            },
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableRuleset.SetReplayScore(score);

            scoreProcessor.NewJudgement += _ => scoreProcessor.PopulateScore(score.ScoreInfo);
            scoreProcessor.OnResetFromReplayFrame += () => scoreProcessor.PopulateScore(score.ScoreInfo);

            startPlayback();
        }

        private void checkScoreCompleted()
        {
            if (!scoreProcessor.HasCompleted.Value)
                return;

            PlaybackCompleted?.Invoke(score.ScoreInfo);
        }

        private bool onFail()
        {
            if (!score.ScoreInfo.Mods.OfType<IApplicableFailOverride>().All(m => m.PerformFail()))
                return false;

            Schedule(() =>
            {
                scoreProcessor.FailScore(score.ScoreInfo);
                PlaybackCompleted?.Invoke(score.ScoreInfo);
            });

            return true;
        }

        private void startPlayback()
        {
            if (!IsLoaded)
                return;

            double? lastFrameTime = score.Replay.Frames.LastOrDefault()?.Time;

            if (lastFrameTime == null)
                return;

            clock.Seek(lastFrameTime.Value);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
        }

        private class ManualAdjustableClock : IAdjustableClock
        {
            public double CurrentTime { get; set; }

            public void Reset()
            {
            }

            public void Start()
            {
                IsRunning = true;
            }

            public void Stop()
            {
                IsRunning = false;
            }

            public bool Seek(double position)
            {
                CurrentTime = position;
                return true;
            }

            public void ResetSpeedAdjustments()
            {
                rate = 1;
            }

            private double rate;

            double IAdjustableClock.Rate
            {
                get => rate;
                set => rate = value;
            }

            double IClock.Rate => rate;

            public bool IsRunning { get; private set; } = true;
        }

        private partial class HeadlessRulesetSkinProvidingContainer : RulesetSkinProvidingContainer
        {
            protected override bool AllowSampleLookup(ISampleInfo sampleInfo) => false;

            public HeadlessRulesetSkinProvidingContainer(Ruleset ruleset, IBeatmap beatmap, ISkin? beatmapSkin)
                : base(ruleset, beatmap, beatmapSkin)
            {
            }
        }
    }
}
