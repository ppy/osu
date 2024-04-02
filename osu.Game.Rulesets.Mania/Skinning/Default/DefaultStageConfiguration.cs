// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    public partial class DefaultStageConfiguration : Drawable, ISerialisableDrawable
    {
        public DefaultStageConfiguration()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Y;
        }

        public bool UsesFixedAnchor { get; set; }

        public bool IsToolboxVisible => false;
    }
}
