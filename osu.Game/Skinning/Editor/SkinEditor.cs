// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Skinning.Editor
{
    public class SkinEditor : CompositeDrawable
    {
        private readonly Drawable target;

        public SkinEditor(Drawable target)
        {
            this.target = target;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    target,
                    new SkinBlueprintContainer(target),
                }
            };
        }
    }
}
