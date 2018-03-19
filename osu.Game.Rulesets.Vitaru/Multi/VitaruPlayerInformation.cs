using osu.Game.Rulesets.Vitaru.Objects.Characters;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Vitaru.Multi
{
    [Serializable]
    public class VitaruPlayerInformation
    {
        public string PlayerID = "0";

        public Characters Character;

        public float PlayerX;

        public float PlayerY;

        public float MouseX;

        public float MouseY;

        public float ClockSpeed;

        public Dictionary<VitaruAction, bool> Actions;

        public VitaruAction PressedAction;
    }
}
