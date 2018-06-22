// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Storyboards
{
    public interface IStoryboardElement
    {
        string Path { get; }
        bool IsDrawable { get; }

        Drawable CreateDrawable();
    }
}
