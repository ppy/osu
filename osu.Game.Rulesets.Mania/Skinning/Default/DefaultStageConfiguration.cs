// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    public partial class DefaultStageConfiguration : Drawable, ISerialisableDrawable
    {
        [Resolved]
        private ManiaPlayfield playfield { get; set; } = null!;

        public DefaultStageConfiguration()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.Y;
        }

        protected override void Update()
        {
            base.Update();

            playfield.StageContainer.Anchor = Anchor;
            playfield.StageContainer.Origin = Origin;
            playfield.StageContainer.Position = Position;
            playfield.StageContainer.Scale = Scale;
            playfield.StageContainer.Rotation = Rotation;

            Size = playfield.StageContainer.Size;
        }

        public bool UsesFixedAnchor { get; set; }

        public bool IsToolboxVisible => false;
    }
}
