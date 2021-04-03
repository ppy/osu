// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Desktop
{
    [Serializable]
    internal class GameState
    {
        [JsonConverter(typeof(UserActivityConverter))]
        public UserActivity Activity { get; set; }

        public RulesetInfo Ruleset { get; set; }

        public BeatmapMetadata BeatmapMetadata { get; set; }

        private class UserActivityConverter : JsonConverter<UserActivity>
        {
            public override UserActivity ReadJson(JsonReader reader, Type objectType, UserActivity existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, UserActivity value, JsonSerializer serializer)
            {
                string status = null;

                switch (value)
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
