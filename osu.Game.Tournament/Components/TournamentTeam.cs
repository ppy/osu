// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Tournament.Components
{
    public class TournamentTeam
    {
        /// <summary>
        /// The name of this team.
        /// </summary>
        public string FullName;

        /// <summary>
        /// Name of the file containing the flag.
        /// </summary>
        public string FlagName;

        private string acronym;

        /// <summary>
        /// Short acronym which appears in the group boxes post-selection.
        /// </summary>
        public string Acronym
        {
            get { return acronym ?? FullName.Substring(0, 3); }
            set { acronym = value; }
        }
    }
}
