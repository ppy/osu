// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// A wrapper over a <see cref="ScoreProcessor"/> for spectated users.
    /// This should be used when a local "playable" beatmap is unavailable or expensive to generate for the spectated user.
    /// </summary>
    public class SpectatorScoreProcessor : Component
    {
        /// <summary>
        /// The current total score.
        /// </summary>
        public readonly BindableDouble TotalScore = new BindableDouble { MinValue = 0 };

        /// <summary>
        /// The current accuracy.
        /// </summary>
        public readonly BindableDouble Accuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current combo.
        /// </summary>
        public readonly BindableInt Combo = new BindableInt();

        /// <summary>
        /// The <see cref="ScoringMode"/> used to calculate scores.
        /// </summary>
        public readonly Bindable<ScoringMode> Mode = new Bindable<ScoringMode>();

        /// <summary>
        /// The applied <see cref="Mod"/>s.
        /// </summary>
        public IReadOnlyList<Mod> Mods => scoreProcessor?.Mods.Value ?? Array.Empty<Mod>();

        private IClock? referenceClock;

        /// <summary>
        /// The clock used to determine the current score.
        /// </summary>
        public IClock ReferenceClock
        {
            get => referenceClock ?? Clock;
            set => referenceClock = value;
        }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        private readonly IBindableDictionary<int, SpectatorState> spectatorStates = new BindableDictionary<int, SpectatorState>();
        private readonly List<TimedFrame> replayFrames = new List<TimedFrame>();
        private readonly int userId;

        private SpectatorState? spectatorState;
        private ScoreProcessor? scoreProcessor;
        private ScoreInfo? scoreInfo;

        public SpectatorScoreProcessor(int userId)
        {
            this.userId = userId;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Mode.BindValueChanged(_ => UpdateScore());

            spectatorStates.BindTo(spectatorClient.WatchedUserStates);
            spectatorStates.BindCollectionChanged(onSpectatorStatesChanged, true);

            spectatorClient.OnNewFrames += onNewFrames;
        }

        private void onSpectatorStatesChanged(object? sender, NotifyDictionaryChangedEventArgs<int, SpectatorState> e)
        {
            if (!spectatorStates.TryGetValue(userId, out var userState) || userState.BeatmapID == null || userState.RulesetID == null)
            {
                scoreProcessor?.RemoveAndDisposeImmediately();
                scoreProcessor = null;
                scoreInfo = null;
                spectatorState = null;
                replayFrames.Clear();
                return;
            }

            if (scoreProcessor != null)
                return;

            Debug.Assert(scoreInfo == null);

            RulesetInfo? rulesetInfo = rulesetStore.GetRuleset(userState.RulesetID.Value);
            if (rulesetInfo == null)
                return;

            Ruleset ruleset = rulesetInfo.CreateInstance();

            spectatorState = userState;
            scoreInfo = new ScoreInfo { Ruleset = rulesetInfo };
            scoreProcessor = ruleset.CreateScoreProcessor();
            scoreProcessor.Mods.Value = userState.Mods.Select(m => m.ToMod(ruleset)).ToArray();
        }

        private void onNewFrames(int incomingUserId, FrameDataBundle bundle)
        {
            if (incomingUserId != userId)
                return;

            Schedule(() =>
            {
                if (scoreProcessor == null)
                    return;

                replayFrames.Add(new TimedFrame(bundle.Frames.First().Time, bundle.Header));
                UpdateScore();
            });
        }

        public void UpdateScore()
        {
            if (scoreInfo == null || replayFrames.Count == 0)
                return;

            Debug.Assert(spectatorState != null);
            Debug.Assert(scoreProcessor != null);

            int frameIndex = replayFrames.BinarySearch(new TimedFrame(ReferenceClock.CurrentTime));
            if (frameIndex < 0)
                frameIndex = ~frameIndex;
            frameIndex = Math.Clamp(frameIndex - 1, 0, replayFrames.Count - 1);

            TimedFrame frame = replayFrames[frameIndex];
            Debug.Assert(frame.Header != null);

            scoreInfo.MaxCombo = frame.Header.MaxCombo;
            scoreInfo.Statistics = frame.Header.Statistics;

            Accuracy.Value = frame.Header.Accuracy;
            Combo.Value = frame.Header.Combo;

            scoreProcessor.ExtractScoringValues(frame.Header, out var currentScoringValues, out _);
            TotalScore.Value = scoreProcessor.ComputeScore(Mode.Value, currentScoringValues, spectatorState.MaximumScoringValues);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient.IsNotNull())
                spectatorClient.OnNewFrames -= onNewFrames;
        }

        private class TimedFrame : IComparable<TimedFrame>
        {
            public readonly double Time;
            public readonly FrameHeader? Header;

            public TimedFrame(double time)
            {
                Time = time;
            }

            public TimedFrame(double time, FrameHeader header)
            {
                Time = time;
                Header = header;
            }

            public int CompareTo(TimedFrame other) => Time.CompareTo(other.Time);
        }
    }
}
