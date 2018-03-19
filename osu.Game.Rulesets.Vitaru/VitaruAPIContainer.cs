using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;

namespace osu.Game.Rulesets.Vitaru
{
    public class VitaruAPIContainer : Container, IOnlineComponent
    {
        public static int PlayerID;

        public static bool Shawdooow;
        public static bool Arrcival;
        public static bool Jorolf;

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            api.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                default:
                    PlayerID = -1;
                    Shawdooow = false;
                    break;
                case APIState.Online:
                    PlayerID = (int)api.LocalUser.Value.Id;
                    Shawdooow = api.LocalUser.Value.Username == "Shawdooow";
                    Arrcival = api.LocalUser.Value.Username == "Arrcival";
                    Jorolf = api.LocalUser.Value.Username == "Jorolf";
                    break;
            }
        }
    }
}
