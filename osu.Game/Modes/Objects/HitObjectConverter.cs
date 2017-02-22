// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Objects
{
    public abstract class HitObjectConverter<T>
        where T : HitObject
    {
        public abstract List<T> Convert(Beatmap beatmap);
    }

    public class HitObjectConvertException : Exception
    {
        public HitObject Input { get; }
        public HitObjectConvertException(string modeName, HitObject input)
            : base($@"Can't convert from {input.GetType().Name} to {modeName} HitObject!")
        {
            Input = input;
        }
    }
}
