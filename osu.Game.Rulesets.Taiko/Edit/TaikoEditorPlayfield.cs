// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
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

            AddRangeInternal([poolHitEditorMode]);
        }

        /// <summary>
        /// <see cref="Hit"/> to <see cref="DrawableHit"/> pool that
        /// returns <see cref="DrawableHit"/> in editor mode (it will recreates its drawable hierarchy each <c>OnApply</c>).
        /// </summary>
        private readonly HitPool poolHitEditorMode = new HitPool(50, editorMode: true);

        protected override IDrawablePool? PropertyBasedDrawableHitObjectPool(HitObject hitObject)
        {
            switch (hitObject)
            {
                // We should to return the editor pool, and suppress non-editor pools.
                case Hit: return poolHitEditorMode;
                default: return null;
            }
        }
    }
}
