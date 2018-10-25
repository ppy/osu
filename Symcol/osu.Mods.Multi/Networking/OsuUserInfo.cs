using System;

namespace osu.Mods.Multi.Networking
{
    /// <summary>
    /// Includes osu User information
    /// </summary>
    [Serializable]
    public class OsuUserInfo
    {
        public string Username = "";

        public long ID = -1;

        public string Colour = "#ffffff";

        public string Pic;

        public string Background;

        public string Country;

        public string CountryFlagName;
    }
}
