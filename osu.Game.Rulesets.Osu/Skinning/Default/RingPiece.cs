// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class RingPiece : CircularContainer
    {
        public RingPiece(float thickness = 9)
        {
            Size = OsuHitObject.OBJECT_DIMENSIONS;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Masking = true;
            BorderThickness = thickness;
            BorderColour = Color4.White;

            Child = new Box
            {
                AlwaysPresent = true,
                Alpha = 0,
                RelativeSizeAxes = Axes.Both
            };
        }

        [Resolved(canBeNull: true)]
        private ISkinSource? skin { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (skin != null)
            {
                skin.SourceChanged += skinChanged;
                skinChanged();
            }
        }

        private void skinChanged()
        {
            float radius = skin?.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.EditorBlueprintRadius)?.Value ?? OsuHitObject.OBJECT_RADIUS;

            Size = new Vector2(radius * 2);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (skin != null)
                skin.SourceChanged -= skinChanged;

            base.Dispose(isDisposing);
        }
    }
}
