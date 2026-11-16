// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.RankingV2.Legacy
{
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L662-L669"/>
    public partial class LegacyRankingBackgroundOverlay : CompositeDrawable, ISerialisableDrawable
    {
        private Sprite sprite = null!;

        // Note that `IBindable<IScoreInfo>` is resolved as required but not used
        // in order to constrain the ability to place this skinnable component to the results screen.
        [BackgroundDependencyLoader]
        private void load(IBindable<IScoreInfo> _, SkinManager skinManager)
        {
            InternalChild = sprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                // This texture is not skinnable on stable.
                // See `SkinSource.Osu` specification in https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L663.
                Texture = skinManager.DefaultClassicSkin.GetTexture(@"ranking-background-overlay"),
                Blending = BlendingParameters.Additive,
            };

            // The fixed size is transferred manually from the sprite instead of using `AutoSizeAxes = Axes.Both`
            // so that the bounding box of this component does not pulsate back and forth in the skin editor
            // due to the sprite spinning.
            // TODO: prevent resizing this sprite later.
            Size = sprite.Size;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sprite.RotateTo(0).Then().RotateTo(360, 20000).Loop();
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the legacy skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion
    }
}
