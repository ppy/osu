// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Tournament.Components
{
    [Serializable]
    public class TournamentTeam
    {
        /// <summary>
        /// The name of this team.
        /// </summary>
        public string FullName;

        private string flagName;

        /// <summary>
        /// Name of the file containing the flag.
        /// </summary>
        public string FlagName
        {
            get => flagName ?? Acronym?.Substring(0, 2);
            set => flagName = value;
        }

        private string acronym;

        /// <summary>
        /// Short acronym which appears in the group boxes post-selection.
        /// </summary>
        public string Acronym
        {
            get => acronym ?? FullName?.Substring(0, 3);
            set => acronym = value;
        }

        [JsonProperty]
        public List<User> Players { get; set; } = new List<User>();

        public override string ToString() => FullName ?? Acronym;
    }
}
