// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Online.WebSockets
{
    public class PlayerStateBroadcaster : GameStateBroadcaster<PlayerState>
    {
        public override string Type => @"Player";
        public override PlayerState Message { get; } = new PlayerState();

        private readonly ScoreProcessor scoreProcessor;
        private readonly HealthProcessor healthProcessor;

        public PlayerStateBroadcaster(ScoreProcessor scoreProcessor, HealthProcessor healthProcessor)
        {
            this.scoreProcessor = scoreProcessor;
            this.healthProcessor = healthProcessor;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Message.TotalScore.BindTo(scoreProcessor.TotalScore);
            Message.TotalScore.ValueChanged += _ => Broadcast();

            Message.Accuracy.BindTo(scoreProcessor.Accuracy);
            Message.Accuracy.ValueChanged += _ => Broadcast();

            Message.Combo.BindTo(scoreProcessor.Combo);
            Message.Combo.ValueChanged += _ => Broadcast();

            Message.Mods.BindTo(scoreProcessor.Mods);
            Message.Mods.ValueChanged += _ => Broadcast();

            Message.HighestCombo.BindTo(scoreProcessor.HighestCombo);
            Message.HighestCombo.ValueChanged += _ => Broadcast();

            Message.Rank.BindTo(scoreProcessor.Rank);
            Message.Rank.ValueChanged += _ => Broadcast();

            Message.Health.BindTo(healthProcessor.Health);
            Message.Health.ValueChanged += _ => Broadcast();
        }
    }

    public class PlayerState
    {
        public readonly BindableDouble TotalScore = new BindableDouble();
        public readonly BindableDouble Accuracy = new BindableDouble();
        public readonly BindableInt Combo = new BindableInt();
        public readonly Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>();
        public readonly BindableInt HighestCombo = new BindableInt();
        public readonly Bindable<ScoreRank> Rank = new Bindable<ScoreRank>();
        public readonly BindableDouble Health = new BindableDouble();
    }
}
