// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning
{
    public partial class LegacyRankDisplay : GameplayRankDisplay, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private ISkinSource source { get; set; } = null!;

        private readonly Sprite rankSprite;

        public LegacyRankDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddInternal(rankSprite = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(rank =>
            {
                var texture = source.GetTexture($"ranking-{rank.NewValue}-small");

                rankSprite.Texture = texture;

                if (texture != null)
                {
                    var transientRank = new Sprite
                    {
                        Texture = texture,
                        Blending = BlendingParameters.Additive,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        BypassAutoSizeAxes = Axes.Both,
                    };
                    AddInternal(transientRank);
                    transientRank.FadeOutFromOne(500, Easing.Out)
                                 .ScaleTo(new Vector2(1.625f), 500, Easing.Out)
                                 .Expire();
                }
            }, true);
        }
    }
}
