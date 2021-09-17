using System.ComponentModel;

namespace osu.Game.Overlays.Login
{
    public enum UserAction
    {
        Online,

        [Description(@"Do not disturb")]
        DoNotDisturb,

        [Description(@"Appear offline")]
        AppearOffline,

        [Description(@"Sign out")]
        SignOut,
    }
}