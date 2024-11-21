// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct ManiaDifficultyAttributes : IDifficultyAttributes
    {
        /// <inheritdoc/>
        public double StarRating { get; set; }

        /// <inheritdoc/>
        public int MaxCombo { get; set; }

        /// <summary>
        /// The hit window for a GREAT hit inclusive of rate-adjusting mods (DT/HT/etc).
        /// </summary>
        /// <remarks>
        /// Rate-adjusting mods do not affect the hit window at all in osu-stable.
        /// </remarks>
        [JsonProperty("great_hit_window")]
        public double GreatHitWindow { get; set; }

        public IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            yield return (IDifficultyAttributes.ATTRIB_ID_MAX_COMBO, MaxCombo);
            yield return (IDifficultyAttributes.ATTRIB_ID_DIFFICULTY, StarRating);
            yield return (IDifficultyAttributes.ATTRIB_ID_GREAT_HIT_WINDOW, GreatHitWindow);
        }

        public void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            MaxCombo = (int)values[IDifficultyAttributes.ATTRIB_ID_MAX_COMBO];
            StarRating = values[IDifficultyAttributes.ATTRIB_ID_DIFFICULTY];
            GreatHitWindow = values[IDifficultyAttributes.ATTRIB_ID_GREAT_HIT_WINDOW];
        }
    }
}
