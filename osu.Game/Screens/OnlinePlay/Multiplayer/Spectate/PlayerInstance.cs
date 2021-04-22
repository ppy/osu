// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable, IAdjustableAudioComponent
    {
        public bool PlayerLoaded => stack?.CurrentScreen is Player;

        public readonly int UserId;
        public readonly CatchUpSlaveClock GameplayClock;

        public Score Score { get; private set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private readonly Container gameplayContent;
        private readonly LoadingLayer loadingLayer;
        private readonly AudioContainer audioContainer;
        private OsuScreenStack stack;

        public PlayerInstance(int userId, CatchUpSlaveClock gameplayClock)
        {
            UserId = userId;
            GameplayClock = gameplayClock;

            RelativeSizeAxes = Axes.Both;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                audioContainer = new AudioContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = gameplayContent = new DrawSizePreservingFillContainer { RelativeSizeAxes = Axes.Both },
                },
                loadingLayer = new LoadingLayer(true) { State = { Value = Visibility.Visible } }
            };
        }

        public void LoadScore(Score score)
        {
            if (Score != null)
                throw new InvalidOperationException($"Cannot load a new score on a {nameof(PlayerInstance)} with an existing score.");

            Score = score;

            gameplayContent.Child = new GameplayIsolationContainer(beatmapManager.GetWorkingBeatmap(Score.ScoreInfo.Beatmap), Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
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

        #region IAdjustableAudioComponent

        public IBindable<double> AggregateVolume => audioContainer.AggregateVolume;

        public IBindable<double> AggregateBalance => audioContainer.AggregateBalance;

        public IBindable<double> AggregateFrequency => audioContainer.AggregateFrequency;

        public IBindable<double> AggregateTempo => audioContainer.AggregateTempo;

        public void BindAdjustments(IAggregateAudioAdjustment component)
        {
            audioContainer.BindAdjustments(component);
        }

        public void UnbindAdjustments(IAggregateAudioAdjustment component)
        {
            audioContainer.UnbindAdjustments(component);
        }

        public void AddAdjustment(AdjustableProperty type, IBindable<double> adjustBindable)
        {
            audioContainer.AddAdjustment(type, adjustBindable);
        }

        public void RemoveAdjustment(AdjustableProperty type, IBindable<double> adjustBindable)
        {
            audioContainer.RemoveAdjustment(type, adjustBindable);
        }

        public void RemoveAllAdjustments(AdjustableProperty type)
        {
            audioContainer.RemoveAllAdjustments(type);
        }

        public BindableNumber<double> Volume => audioContainer.Volume;

        public BindableNumber<double> Balance => audioContainer.Balance;

        public BindableNumber<double> Frequency => audioContainer.Frequency;

        public BindableNumber<double> Tempo => audioContainer.Tempo;

        #endregion
    }
}
