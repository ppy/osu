// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public partial class TaikoEditorPlayfield : TaikoPlayfield
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            // This is the simplest way to extend the taiko playfield beyond the left of the drum area.
            // Required in the editor to not look weird underneath left toolbox area.
            AddInternal(new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.PlayfieldBackgroundRight), _ => new PlayfieldBackgroundRight())
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopRight,
            });
        }
    }
}
