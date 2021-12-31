using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ManagedBass;

namespace osu.Game.Audio
{
    [StructLayout(LayoutKind.Sequential)]
    public class GainParameters : IEffectParameter
    {
        public float fTarget; //new volume to reach
        public float fCurrent;  //current volume
        public float fTime; //time to reach fTarget
        public int lCurve;  //curve used to reach fTarget
        public EffectType FXType => (EffectType) 9;

        public GainParameters()
        {
            fTarget = 1;
            fCurrent = 1;
            fTime = 0;
            lCurve = 0;
        }
    }
}
