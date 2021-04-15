// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable
    {
        public bool PlayerLoaded => stack?.CurrentScreen is Player;

        public User User => Score.ScoreInfo.User;

        public WorkingBeatmap Beatmap { get; private set; }

        public readonly Score Score;
        public readonly SpectatorCatchUpSlaveClock GameplayClock;

        private OsuScreenStack stack;

        public PlayerInstance(Score score, SpectatorCatchUpSlaveClock gameplayClock)
        {
            Score = score;
            GameplayClock = gameplayClock;

            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            Beatmap = beatmapManager.GetWorkingBeatmap(Score.ScoreInfo.Beatmap, bypassCache: true);

            InternalChild = new GameplayIsolationContainer(Beatmap, Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawSizePreservingFillContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = stack = new OsuScreenStack()
                }
            };

            stack.Push(new MultiplayerSpectatorPlayerLoader(Score, () => new MultiplayerSpectatorPlayer(Score, GameplayClock)));
        }

        // Player interferes with global input, so disable input for now.
        public override bool PropagatePositionalInputSubTree => false;
        public override bool PropagateNonPositionalInputSubTree => false;
    }
}
