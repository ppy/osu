// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Users
{
    public class User
    {
        public int Id;
        public string Username;

        /// <summary>
        /// Two-letter flag acronym (ISO 3166 standard)
        /// </summary>
        public string FlagName;
    }
}
