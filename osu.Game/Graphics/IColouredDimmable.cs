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
        Colour4 DrawColourOffset
        {
            // Default implementation for Drawables that inherit DrawColourOffset from their parent.
            get
            {
                if (DrawColourInfo.Blending == BlendingParameters.Additive)
                    // Additive Drawables shouldn't have any offset, because things underneath them
                    // already have it added to them. This does rely on the fact that there is
                    // something non-additive behind this sprite, which seems to be the case
                    // with storyboards, as beatmap background is always enabled.
                    return Colour4.Black;

                if (Parent is IColouredDimmable colouredDimmableParent)
                    return colouredDimmableParent.DrawColourOffset;
                else if (Parent?.Parent is IColouredDimmable colouredDimmableGrandparent)
                    // Needed to skip intermediate containers.
                    return colouredDimmableGrandparent.DrawColourOffset;

                return Colour4.Black;
            }
        }
    }
}
