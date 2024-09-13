// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// Provides an area for and manages the hierarchy of a spectated player within a <see cref="MultiSpectatorScreen"/>.
    /// </summary>
    public partial class PlayerArea : CompositeDrawable
    {
        /// <summary>
        /// Raised after <see cref="Player.StartGameplay"/> is called on <see cref="Player"/>.
        /// </summary>
        public event Action? OnGameplayStarted;

        /// <summary>
        /// Whether a <see cref="Player"/> is loaded in the area.
        /// </summary>
        public bool PlayerLoaded => stack?.CurrentScreen is Player { IsLoaded: true };

        /// <summary>
        /// The user id this <see cref="PlayerArea"/> corresponds to.
        /// </summary>
        public readonly int UserId;

        /// <summary>
        /// The <see cref="Spectate.SpectatorPlayerClock"/> used to control the gameplay running state of a loaded <see cref="Player"/>.
        /// </summary>
        public readonly SpectatorPlayerClock SpectatorPlayerClock;

        /// <summary>
        /// The clock adjustments applied by the <see cref="Player"/> loaded in this area.
        /// </summary>
        public IAggregateAudioAdjustment ClockAdjustmentsFromMods => clockAdjustmentsFromMods;

        /// <summary>
        /// The currently-loaded score.
        /// </summary>
        public Score? Score { get; private set; }

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        private readonly AudioAdjustments clockAdjustmentsFromMods = new AudioAdjustments();
        private readonly BindableDouble volumeAdjustment = new BindableDouble();
        private readonly Container gameplayContent;
        private readonly LoadingLayer loadingLayer;
        private OsuScreenStack? stack;

        public PlayerArea(int userId, SpectatorPlayerClock clock)
        {
            UserId = userId;
            SpectatorPlayerClock = clock;

            RelativeSizeAxes = Axes.Both;

            AudioContainer audioContainer;
            InternalChildren = new Drawable[]
            {
                audioContainer = new AudioContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = gameplayContent = new DrawSizePreservingFillContainer { RelativeSizeAxes = Axes.Both },
                },
                loadingLayer = new LoadingLayer(true) { State = { Value = Visibility.Visible } }
            };

            audioContainer.AddAdjustment(AdjustableProperty.Volume, volumeAdjustment);
        }

        public void LoadScore(Score score)
        {
            if (Score != null)
                throw new InvalidOperationException($"Cannot load a new score on a {nameof(PlayerArea)} that has an existing score.");

            Score = score;

            gameplayContent.Child = new PlayerIsolationContainer(beatmap.Value, Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
            {
                RelativeSizeAxes = Axes.Both,
                Child = stack = new OsuScreenStack
                {
                    Name = nameof(PlayerArea),
                }
            };

            stack.Push(new MultiSpectatorPlayerLoader(Score, () =>
            {
                var player = new MultiSpectatorPlayer(Score, SpectatorPlayerClock);
                player.OnGameplayStarted += () => OnGameplayStarted?.Invoke();

                clockAdjustmentsFromMods.BindAdjustments(player.ClockAdjustmentsFromMods);

                return player;
            }));

            loadingLayer.Hide();
        }

        private bool mute = true;

        public bool Mute
        {
            get => mute;
            set
            {
                mute = value;
                volumeAdjustment.Value = value ? 0 : 1;
            }
        }

        // Player interferes with global input, so disable input for now.
        public override bool PropagatePositionalInputSubTree => false;
        public override bool PropagateNonPositionalInputSubTree => false;

        /// <summary>
        /// Isolates each player instance from the game-wide ruleset/beatmap/mods (to allow for different players having different settings).
        /// </summary>
        private partial class PlayerIsolationContainer : Container
        {
            [Cached]
            private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

            [Cached]
            private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<WorkingBeatmap>();

            [Cached]
            private readonly Bindable<IReadOnlyList<Mod>> mods = new Bindable<IReadOnlyList<Mod>>();

            public PlayerIsolationContainer(WorkingBeatmap beatmap, RulesetInfo ruleset, IReadOnlyList<Mod> mods)
            {
                this.beatmap.Value = beatmap;
                this.ruleset.Value = ruleset;
                this.mods.Value = mods;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs(ruleset.BeginLease(false));
                dependencies.CacheAs(beatmap.BeginLease(false));
                dependencies.CacheAs(mods.BeginLease(false));
                return dependencies;
            }
        }
    }
}
