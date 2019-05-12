// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Storyboards
{
    public interface IStoryboardElement
    {
        string Path { get; }
        bool IsDrawable { get; }

        double StartTime { get; }

        Drawable CreateDrawable();
    }
}
