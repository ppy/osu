// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Graphics
{
    /// <summary>
    /// An interface for drawable objects whose dim colour can be adjusted.
    /// </summary>
    public interface IColouredDimmable : IDrawable
    {
        IColouredDimmable DrawColourOffsetSource
        {
            get
            {
                if (Parent is IColouredDimmable colouredDimmableParent)
                    return colouredDimmableParent.DrawColourOffsetSource;
                else if (Parent?.Parent is IColouredDimmable colouredDimmableGrandparent)
                    // Needed to skip intermediate containers.
                    return colouredDimmableGrandparent.DrawColourOffsetSource;

                return this;
            }
        }

        Colour4 DrawColourOffset
        {
            get
            {
                if (DrawColourInfo.Blending == BlendingParameters.Additive)
                    // Additive Drawables shouldn't have any offset, because things underneath them
                    // already have it added to them. This does rely on the fact that there is
                    // something non-additive (but still IColouredDimmable) behind this sprite,
                    // which seems to be the case with storyboards, as beatmap background is always enabled.
                    return Colour4.Black;

                IColouredDimmable source = DrawColourOffsetSource;

                if (source != null && source != this)
                    return source.DrawColourOffset;

                return Colour4.Black;
            }
        }
    }
}
