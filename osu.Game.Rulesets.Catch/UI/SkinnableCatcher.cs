// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// The visual representation of the <see cref="Catcher"/>.
    /// It includes the body part of the catcher and the catcher plate.
    /// </summary>
    public partial class SkinnableCatcher : SkinnableDrawable
    {
        /// <summary>
        /// This is used by skin elements to determine which texture of the catcher is used.
        /// </summary>
        [Cached]
        public readonly Bindable<CatcherAnimationState> AnimationState = new Bindable<CatcherAnimationState>();

        public SkinnableCatcher()
            : base(new CatchSkinComponentLookup(CatchSkinComponents.Catcher), _ => new DefaultCatcher())
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            CentreComponent = false;
        }
    }
}
