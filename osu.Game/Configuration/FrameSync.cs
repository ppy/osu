using System;
namespace osu.Game.Configuration
{
    public enum FrameSync
    {
        VSync = 1,
        Limit120 = 0,
        Unlimited = 2,
        CompletelyUnlimited = 4,
        Custom = 5
    }
}