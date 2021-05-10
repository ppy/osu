// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Extensions;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableElementTargetContainer : SkinReloadableDrawable, ISkinnableTarget
    {
        private SkinnableTargetWrapper content;

        public SkinnableTarget Target { get; }

        public SkinnableElementTargetContainer(SkinnableTarget target)
        {
            Target = target;
        }

        public IEnumerable<SkinnableInfo> CreateSerialisedChildren() =>
            content.Select(d => d.CreateSerialisedInformation());

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            content = skin.GetDrawableComponent(new SkinnableTargetComponent(Target)) as SkinnableTargetWrapper;

            ClearInternal();

            if (content != null)
                LoadComponentAsync(content, AddInternal);
        }
    }
}
