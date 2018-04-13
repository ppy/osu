// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Beatmaps.Legacy
{
    [Flags]
    public enum LegacyMods
    {
        None = 0,
        NoFail = 1,
        Easy = 2,
        TouchDevice = 4,
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        NightCore = 512,
        FlashLight = 1024,
        AutoPlay = 2048,
        SpunOut = 4096,
        AutoPilot = 8192,
        Perfect = 16384,
        Key4 = 32768,
        Key5 = 65536,
        Key6 = 131072,
        Key7 = 262144,
        Key8 = 524288,
        keyMod = 1015808,// k4+k5+k6+k7+k8
        FadeIn = 1048576,
        Random = 2097152,
        Cinema = 4194304,
        TargetPractice = 8388608,
        Key9 = 16777216,
        Coop = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456,
    }
}
