// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Screens.Play;

namespace osu.Game.Online.Broadcasts
{
    public partial class GameplayStateBroadcaster : Broadcaster
    {
        private IBindable<JudgementResult>? result;
        private Bindable<long>? score;
        private Bindable<double>? health;
        private Bindable<double>? accuracy;
        private readonly GameplayState state;

        public GameplayStateBroadcaster(GameplayState state)
        {
            this.state = state;
        }

        protected override void LoadComplete()
        {
            broadcast(@"mods", state.Mods.Select(mod => new ModDetails
            {
                Name = mod.Acronym,
                Settings = mod.GetOrderedSettingsSourceProperties().Select(pair => new ModSettingDetails
                {
                    Name = pair.Item1.Label.ToString(),
                    Value = pair.Item2.GetValue(mod),
                }).ToArray()
            }).ToArray());

            broadcast(@"ruleset", state.Ruleset.RulesetInfo.ShortName);
            broadcast(@"beatmap", state.Beatmap.BeatmapInfo);

            score = state.ScoreProcessor.TotalScore.GetBoundCopy();
            score.BindValueChanged(value => broadcast(@"score", value.NewValue));

            health = state.HealthProcessor.Health.GetBoundCopy();
            health.BindValueChanged(value => broadcast(@"health", value.NewValue));

            result = state.LastJudgementResult.GetBoundCopy();
            result.BindValueChanged(value => broadcast(@"result", value.NewValue.Type));

            accuracy = state.ScoreProcessor.Accuracy.GetBoundCopy();
            accuracy.BindValueChanged(value => broadcast(@"accuracy", value.NewValue));
        }

        private void broadcast<T>(string type, T value)
        {
            var detail = new GameplayDetails<T>
            {
                User = new UserDetails
                {
                    Name = state.Score.ScoreInfo.User.Username,
                    Id = state.Score.ScoreInfo.UserID,
                },
                Value = value,
            };

            Broadcast(type, detail);
        }

        private struct UserDetails
        {
            [JsonProperty(@"name")]
            public string Name;

            [JsonProperty(@"id")]
            public int Id;
        }

        private struct GameplayDetails<T>
        {
            [JsonProperty(@"user")]
            public UserDetails User;

            [JsonProperty(@"value")]
            public T Value;
        }

        private struct ModDetails
        {
            [JsonProperty(@"name")]
            public string Name;

            [JsonProperty(@"settings")]
            public ModSettingDetails[] Settings;
        }

        private struct ModSettingDetails
        {
            [JsonProperty(@"name")]
            public string Name;

            [JsonProperty(@"value")]
            public object? Value;
        }
    }
}
