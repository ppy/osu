// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Extensions
{
    public interface IExtension
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
    }
}