using Symcol.Rulesets.Core.Multiplayer.Networking;
using System;

namespace osu.Game.Rulesets.Vitaru.Multi
{
    [Serializable]
    public class VitaruClientInfo : RulesetClientInfo
    {
        public VitaruPlayerInformation PlayerInformation;
    }
}
