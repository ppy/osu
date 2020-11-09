// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Mvis.Modules
{
    public class ModifiedSkinnableSprite : SkinnableSprite
    {
        protected override Vector2 DrawScale => new Vector2(Parent.DrawHeight / 768);

        public ModifiedSkinnableSprite(string textureName, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : base(textureName, allowFallback, confineMode)
        {
            Size = new Vector2(1366, 768);
            CentreComponent = false;
            OverrideChildAnchor = true;

            ChildAnchor = Anchor.Centre;
            ChildOrigin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.None;
            CentreComponent = false;
        }
    }
}
