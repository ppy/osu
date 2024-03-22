// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    public partial class DefaultStageConfiguration : Drawable, ISerialisableDrawable
    {
        [SettingSource("Position", "The position of the playfield.")]
        public BindableFloat PlayfieldPosition { get; } = new BindableFloat(0.5f)
        {
            MinValue = 0,
            MaxValue = 1
        };

        [Resolved]
        private ManiaPlayfield playfield { get; set; } = null!;

        public DefaultStageConfiguration()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PlayfieldPosition.BindValueChanged(updatePosition, true);
        }

        private void updatePosition(ValueChangedEvent<float> pos)
        {
            playfield.StageContainer.Origin = Anchor.TopCentre;
            playfield.StageContainer.X = pos.NewValue;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
