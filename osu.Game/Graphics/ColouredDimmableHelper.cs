// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Layout;

namespace osu.Game.Graphics
{
    public class ColouredDimmableHelper
    {
        private Drawable owner;

        public readonly LayoutValue<IColouredDimmableSource?> SourceLayout = new LayoutValue<IColouredDimmableSource?>(Invalidation.Parent);

        private IShader? originalShader;

        private IShader? colouredShader;

        public ColouredDimmableHelper(Drawable owner)
        {
            this.owner = owner;
        }

        public void SetShaders(IShader originalShader, IShader colouredShader)
        {
            this.originalShader = originalShader;
            this.colouredShader = colouredShader;
        }

        public IColouredDimmableSource? Source
        {
            get
            {
                if (!SourceLayout.IsValid)
                    SourceLayout.Value = owner.FindClosestParent<IColouredDimmableSource>();

                return SourceLayout.Value;
            }
        }

        public ColouredDimmableSharedData? SharedData => Source?.ColouredDimmableSharedData;

        public IShader? GetShader(BlendingParameters blending)
        {
            // Additive Drawables shouldn't have any offset, because things underneath them
            // already have it added to them. This does rely on the fact that there is
            // something non-additive (but still coloured dimmable) behind this sprite,
            // which seems to be the case with storyboards, as beatmap background is always enabled.
            return blending == BlendingParameters.Additive || SharedData == null ? originalShader : colouredShader;
        }
    }
}
