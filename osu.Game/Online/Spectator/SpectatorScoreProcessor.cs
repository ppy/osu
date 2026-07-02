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
    public partial class SpectatorScoreProcessor : Component
    {
        /// <summary>
        /// Whether to use the the total score without mods for display purposes.
        /// </summary>
        public bool UseTotalScoreWithoutMods { get; set; }

        /// <summary>
        /// The current total score.
        /// </summary>
        public readonly BindableLong TotalScore = new BindableLong { MinValue = 0 };

        /// <summary>
        /// The total number of points awarded for the score without including mod multipliers.
        /// </summary>
        /// <remarks>
        /// The purpose of this property is to enable future lossless rebalances of mod multipliers.
        /// </remarks>
        public readonly BindableLong TotalScoreWithoutMods = new BindableLong { MinValue = 0 };

        /// <summary>
        /// The current accuracy.
        /// </summary>
        public readonly BindableDouble Accuracy = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// The current combo.
        /// </summary>
        public readonly BindableInt Combo = new BindableInt();

        /// <summary>
        /// The highest combo achieved in the score thus far.
        /// </summary>
        public readonly BindableInt HighestCombo = new BindableInt();

        /// <summary>
        /// The <see cref="ScoringMode"/> used to calculate scores.
        /// </summary>
        public readonly Bindable<ScoringMode> Mode = new Bindable<ScoringMode>();

        /// <summary>
        /// The applied <see cref="Mod"/>s.
        /// </summary>
        public IReadOnlyList<Mod> Mods => Score?.Mods ?? Array.Empty<Mod>();

        /// <summary>
        /// The score.
        /// </summary>
        public ScoreInfo? Score { get; private set; }

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
                Score = null;
                spectatorState = null;
                replayFrames.Clear();
                return;
            }

            if (Score != null)
                return;

            RulesetInfo? rulesetInfo = rulesetStore.GetRuleset(userState.RulesetID.Value);
            if (rulesetInfo == null)
                return;

            Ruleset ruleset = rulesetInfo.CreateInstance();

            spectatorState = userState;
            Score = new ScoreInfo
            {
                Ruleset = rulesetInfo,
                Mods = userState.Mods.Select(m => m.ToMod(ruleset)).ToArray()
            };
        }

        private void onNewFrames(int incomingUserId, FrameDataBundle bundle)
        {
            if (incomingUserId != userId)
                return;

            Schedule(() =>
            {
                if (Score == null)
                    return;

                replayFrames.Add(new TimedFrame(bundle.Frames.First().Time, bundle.Header));
                UpdateScore();
            });
        }

        public void UpdateScore()
        {
            if (Score == null || replayFrames.Count == 0)
                return;

            Debug.Assert(spectatorState != null);

            int frameIndex = replayFrames.BinarySearch(new TimedFrame(ReferenceClock.CurrentTime));
            if (frameIndex < 0)
                frameIndex = ~frameIndex;
            frameIndex = Math.Clamp(frameIndex - 1, 0, replayFrames.Count - 1);

            TimedFrame frame = replayFrames[frameIndex];
            Debug.Assert(frame.Header != null);

            Score.Accuracy = frame.Header.Accuracy;
            Score.MaxCombo = frame.Header.MaxCombo;
            Score.Statistics = frame.Header.Statistics;
            Score.MaximumStatistics = spectatorState.MaximumStatistics;
            Score.TotalScore = frame.Header.TotalScore;
            Score.TotalScoreWithoutMods = frame.Header.TotalScoreWithoutMods ?? 0;

            Accuracy.Value = frame.Header.Accuracy;
            Combo.Value = frame.Header.Combo;
            HighestCombo.Value = frame.Header.MaxCombo;
            TotalScore.Value = frame.Header.TotalScore;
            TotalScoreWithoutMods.Value = frame.Header.TotalScoreWithoutMods ?? 0;
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

            public int CompareTo(TimedFrame? other) => Time.CompareTo(other?.Time);
        }
    }
}
