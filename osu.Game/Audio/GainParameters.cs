// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using ManagedBass;

namespace osu.Game.Audio
{
    [StructLayout(LayoutKind.Sequential)]
    public class GainParameters : IEffectParameter
    {
        public float fTarget; //new volume to reach
        public float fCurrent; //current volume
        public float fTime; //time to reach fTarget
        public int lCurve; //curve used to reach fTarget
        public EffectType FXType => (EffectType)9;

        public GainParameters()
        {
            fTarget = 1;
            fCurrent = 1;
            fTime = 0;
            lCurve = 0;
        }
    }
}
