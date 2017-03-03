// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Screens.Tournament.Teams
{
    public class Team
    {
        /// <summary>
        /// The name of this team.
        /// </summary>
        public string FullName;

        /// <summary>
        /// Short acronym which appears in the group boxes post-selection.
        /// </summary>
        public string Acronym;

        /// <summary>
        /// Two-letter flag acronym (ISO 3166 standard)
        /// </summary>
        public string FlagName;
    }
}
