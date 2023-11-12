// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonWedgePiece : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Inverted shear")]
        public BindableBool InvertShear { get; } = new BindableBool();

        public ArgonWedgePiece()
        {
            CornerRadius = 10f;

            Size = new Vector2(400, 100);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            Shear = new Vector2(0.8f, 0f);

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#66CCFF").Opacity(0.0f), Color4Extensions.FromHex("#66CCFF").Opacity(0.25f)),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InvertShear.BindValueChanged(v => Shear = new Vector2(0.8f, 0f) * (v.NewValue ? -1 : 1), true);
        }
    }
}
