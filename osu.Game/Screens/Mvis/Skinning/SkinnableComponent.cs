// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Screens.Mvis.Skinning
{
    public class SkinnableComponent : SkinnableDrawable
    {
        public new MarginPadding Padding
        {
            get => base.Padding;
            set => base.Padding = value;
        }

        public SkinnableComponent(string textureName,
                                  Func<ISkinComponent, Drawable> defaultImplementation,
                                  Func<ISkinSource, bool> allowFallback = null,
                                  ConfineMode confineMode = ConfineMode.NoScaling,
                                  bool masking = false)
            : base(new SkinComponent(textureName), defaultImplementation, allowFallback, confineMode)
        {
            CentreComponent = false;
            OverrideChildAnchor = true;
            Masking = masking;

            ChildAnchor = Anchor.Centre;
            ChildOrigin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CentreComponent = false;
        }

        private class SkinComponent : ISkinComponent
        {
            public string LookupName { get; }

            public SkinComponent(string textureName)
            {
                LookupName = textureName;
            }
        }
    }
}
