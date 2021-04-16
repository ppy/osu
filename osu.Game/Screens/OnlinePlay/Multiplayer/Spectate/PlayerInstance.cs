// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable
    {
        public bool PlayerLoaded => stack?.CurrentScreen is Player;

        public readonly int UserId;
        public readonly SpectatorCatchUpSlaveClock GameplayClock;

        public Score Score { get; private set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private readonly Container content;
        private readonly LoadingLayer loadingLayer;
        private OsuScreenStack stack;

        public PlayerInstance(int userId, SpectatorCatchUpSlaveClock gameplayClock)
        {
            UserId = userId;
            GameplayClock = gameplayClock;

            RelativeSizeAxes = Axes.Both;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                content = new DrawSizePreservingFillContainer { RelativeSizeAxes = Axes.Both },
                loadingLayer = new LoadingLayer(true) { State = { Value = Visibility.Visible } }
            };
        }

        public void LoadScore(Score score)
        {
            if (Score != null)
                throw new InvalidOperationException($"Cannot load a new score on a {nameof(PlayerInstance)} with an existing score.");

            Score = score;

            content.Child = new GameplayIsolationContainer(beatmapManager.GetWorkingBeatmap(Score.ScoreInfo.Beatmap, bypassCache: true), Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
            {
                RelativeSizeAxes = Axes.Both,
                Child = stack = new OsuScreenStack()
            };

            stack.Push(new MultiplayerSpectatorPlayerLoader(Score, () => new MultiplayerSpectatorPlayer(Score, GameplayClock)));
            loadingLayer.Hide();
        }

        // Player interferes with global input, so disable input for now.
        public override bool PropagatePositionalInputSubTree => false;
        public override bool PropagateNonPositionalInputSubTree => false;
    }
}
