// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Game.Online
{
    [Serializable]
    public class GameState
    {
        [JsonConverter(typeof(UserActivityConverter))]
        public Bindable<UserActivity> Activity = new Bindable<UserActivity>();

        public Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        [JsonConverter(typeof(WorkingBeatmapConverter))]
        public Bindable<WorkingBeatmap> Beatmap = new Bindable<WorkingBeatmap>();

        [JsonConverter(typeof(ModsConverter))]
        public Bindable<IReadOnlyList<Mod>> Mods = new Bindable<IReadOnlyList<Mod>>();

        private class ModsConverter : JsonConverter<Bindable<IReadOnlyList<Mod>>>
        {
            public override Bindable<IReadOnlyList<Mod>> ReadJson(JsonReader reader, Type objectType, Bindable<IReadOnlyList<Mod>> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, Bindable<IReadOnlyList<Mod>> value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value.Value.Select(m => m.Acronym));
            }
        }

        private class WorkingBeatmapConverter : JsonConverter<Bindable<WorkingBeatmap>>
        {
            public override Bindable<WorkingBeatmap> ReadJson(JsonReader reader, Type objectType, Bindable<WorkingBeatmap> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, Bindable<WorkingBeatmap> value, JsonSerializer serializer)
            {
                var beatmap = value.Value;
                serializer.Serialize(writer, beatmap.BeatmapInfo);
            }
        }

        private class UserActivityConverter : JsonConverter<Bindable<UserActivity>>
        {
            public override Bindable<UserActivity> ReadJson(JsonReader reader, Type objectType, Bindable<UserActivity> existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, Bindable<UserActivity> value, JsonSerializer serializer)
            {
                string status = null;

                switch (value.Value)
                {
                    case UserActivity.Modding _:
                        status = "modding";
                        break;

                    case UserActivity.Editing _:
                        status = "editing";
                        break;

                    case UserActivity.ChoosingBeatmap _:
                        status = "songselect";
                        break;

                    case UserActivity.InLobby _:
                        status = "inlobby";
                        break;

                    case UserActivity.SoloGame _:
                        status = "solo";
                        break;

                    case UserActivity.MultiplayerGame _:
                        status = "multi";
                        break;

                    case UserActivity.SearchingForLobby _:
                        status = "searchinglobby";
                        break;
                }

                serializer.Serialize(writer, status);
            }
        }
    }
}
