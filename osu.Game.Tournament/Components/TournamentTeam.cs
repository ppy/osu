// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Users;

namespace osu.Game.Tournament.Components
{
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
            get { return flagName ?? Acronym.Substring(0, 2); }
            set { flagName = value; }
        }

        private string acronym;

        /// <summary>
        /// Short acronym which appears in the group boxes post-selection.
        /// </summary>
        public string Acronym
        {
            get { return acronym ?? FullName.Substring(0, 3); }
            set { acronym = value; }
        }

        public List<User> Players { get; set; }
    }
}
