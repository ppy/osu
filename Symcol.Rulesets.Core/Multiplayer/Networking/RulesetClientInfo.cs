using Symcol.Core.Networking;
using System;

namespace Symcol.Rulesets.Core.Multiplayer.Networking
{
    /// <summary>
    /// Just a client signature basically
    /// </summary>
    [Serializable]
    public class RulesetClientInfo : ClientInfo
    {
        public string Username = "";

        public int UserID = -1;

        public string UserPic;

        public string UserBackground;

        public string UserCountry;

        public string CountryFlagName;
    }
}
