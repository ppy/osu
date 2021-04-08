// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable
    {
        public bool PlayerLoaded => stack?.CurrentScreen is Player;

        public User User => Score.ScoreInfo.User;
        public ScoreProcessor ScoreProcessor => player?.ScoreProcessor;

        public WorkingBeatmap Beatmap { get; private set; }
        public Ruleset Ruleset { get; private set; }

        public readonly Score Score;

        private OsuScreenStack stack;
        private MultiplayerSpectatorPlayer player;

        public PlayerInstance(Score score)
        {
            Score = score;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            Beatmap = beatmapManager.GetWorkingBeatmap(Score.ScoreInfo.Beatmap, bypassCache: true);
            Ruleset = Score.ScoreInfo.Ruleset.CreateInstance();

            InternalChild = new GameplayIsolationContainer(Beatmap, Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawSizePreservingFillContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = stack = new OsuScreenStack()
                }
            };

            stack.Push(new MultiplayerSpectatorPlayerLoader(Score, () => player = new MultiplayerSpectatorPlayer(Score)));
        }
    }
}
