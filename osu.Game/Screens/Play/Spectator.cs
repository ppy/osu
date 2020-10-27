// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Spectator;
using osu.Game.Replays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    public class Spectator : OsuScreen
    {
        private readonly User targetUser;

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; }

        [Resolved]
        private SpectatorStreamingClient spectatorStreaming { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private Replay replay;

        public Spectator([NotNull] User targetUser)
        {
            this.targetUser = targetUser ?? throw new ArgumentNullException(nameof(targetUser));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = $"Watching {targetUser}",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            spectatorStreaming.OnUserBeganPlaying += userBeganPlaying;
            spectatorStreaming.OnUserFinishedPlaying += userFinishedPlaying;
            spectatorStreaming.OnNewFrames += userSentFrames;

            spectatorStreaming.WatchUser((int)targetUser.Id);
        }

        private void userSentFrames(int userId, FrameDataBundle data)
        {
            if (userId != targetUser.Id)
                return;

            // this should never happen as the server sends the user's state on watching,
            // but is here as a safety measure.
            if (replay == null)
                return;

            var rulesetInstance = ruleset.Value.CreateInstance();

            foreach (var frame in data.Frames)
            {
                IConvertibleReplayFrame convertibleFrame = rulesetInstance.CreateConvertibleReplayFrame();
                convertibleFrame.FromLegacy(frame, beatmap.Value.Beatmap, null);

                var convertedFrame = (ReplayFrame)convertibleFrame;
                convertedFrame.Time = frame.Time;

                replay.Frames.Add(convertedFrame);
            }
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (userId != targetUser.Id)
                return;

            replay ??= new Replay { HasReceivedAllFrames = false };

            var resolvedRuleset = rulesets.AvailableRulesets.FirstOrDefault(r => r.ID == state.RulesetID)?.CreateInstance();

            // ruleset not available
            if (resolvedRuleset == null)
                return;

            var resolvedBeatmap = beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == state.BeatmapID);

            if (resolvedBeatmap == null)
                return;

            var scoreInfo = new ScoreInfo
            {
                Beatmap = resolvedBeatmap,
                Mods = state.Mods.Select(m => m.ToMod(resolvedRuleset)).ToArray(),
                Ruleset = resolvedRuleset.RulesetInfo,
            };

            this.MakeCurrent();

            ruleset.Value = resolvedRuleset.RulesetInfo;
            beatmap.Value = beatmaps.GetWorkingBeatmap(resolvedBeatmap);

            this.Push(new SpectatorPlayerLoader(new Score
            {
                ScoreInfo = scoreInfo,
                Replay = replay,
            }));
        }

        private void userFinishedPlaying(int userId, SpectatorState state)
        {
            if (replay == null) return;

            replay.HasReceivedAllFrames = true;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorStreaming != null)
            {
                spectatorStreaming.OnUserBeganPlaying -= userBeganPlaying;
                spectatorStreaming.OnUserFinishedPlaying -= userFinishedPlaying;
                spectatorStreaming.OnNewFrames -= userSentFrames;

                spectatorStreaming.StopWatchingUser((int)targetUser.Id);
            }
        }
    }
}
