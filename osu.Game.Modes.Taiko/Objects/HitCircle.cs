// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Samples;

namespace osu.Game.Modes.Taiko.Objects
{
    public class HitCircle : TaikoHitObject
    {
        public override TaikoHitType Type
        {
            get
            {
                SampleType st = Sample?.Type ?? SampleType.None;

                return
                    // Don/Katsu
                    ((st & ~(SampleType.Finish | SampleType.Normal)) == 0 ? TaikoHitType.Don : TaikoHitType.Katsu)
                    // Finisher
                    | ((st & SampleType.Finish) > 0 ? TaikoHitType.Finisher : TaikoHitType.None);
            }
        }
    }
}
