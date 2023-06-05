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
using osu.Game.Scoring.Legacy;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// A wrapper over a <see cref="ScoreProcessor"/> for spectated users.
    /// This should be used when a local "playable" beatmap is unavailable or expensive to generate for the spectated user.
    /// </summary>
    public partial class SpectatorScoreProcessor : Component
    {
        /// <summary>
        /// The current total score.
        /// </summary>
        public readonly BindableLong TotalScore = new BindableLong { MinValue = 0 };

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
        public IReadOnlyList<Mod> Mods => scoreInfo?.Mods ?? Array.Empty<Mod>();

        public Func<ScoringMode, long> GetDisplayScore => mode => scoreInfo?.GetDisplayScore(mode) ?? 0;

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
                scoreInfo = null;
                spectatorState = null;
                replayFrames.Clear();
                return;
            }

            if (scoreInfo != null)
                return;

            RulesetInfo? rulesetInfo = rulesetStore.GetRuleset(userState.RulesetID.Value);
            if (rulesetInfo == null)
                return;

            Ruleset ruleset = rulesetInfo.CreateInstance();

            spectatorState = userState;
            scoreInfo = new ScoreInfo
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
                if (scoreInfo == null)
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

            int frameIndex = replayFrames.BinarySearch(new TimedFrame(ReferenceClock.CurrentTime));
            if (frameIndex < 0)
                frameIndex = ~frameIndex;
            frameIndex = Math.Clamp(frameIndex - 1, 0, replayFrames.Count - 1);

            TimedFrame frame = replayFrames[frameIndex];
            Debug.Assert(frame.Header != null);

            scoreInfo.Accuracy = frame.Header.Accuracy;
            scoreInfo.MaxCombo = frame.Header.MaxCombo;
            scoreInfo.Statistics = frame.Header.Statistics;
            scoreInfo.MaximumStatistics = spectatorState.MaximumStatistics;
            scoreInfo.TotalScore = frame.Header.TotalScore;

            Accuracy.Value = frame.Header.Accuracy;
            Combo.Value = frame.Header.Combo;
            TotalScore.Value = frame.Header.TotalScore;
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
