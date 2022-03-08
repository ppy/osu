// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Components
{
    /// <summary>
    /// Intended to be a test bed for skinning. May be removed at some point in the future.
    /// </summary>
    [UsedImplicitly]
    public class BigBlackBox : CompositeDrawable, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Spin", "Should the text spin")]
        public Bindable<bool> TextSpin { get; } = new BindableBool();

        [SettingSource("Alpha", "The alpha value of this box")]
        public BindableNumber<float> BoxAlpha { get; } = new BindableNumber<float>(1)
        {
            MinValue = 0,
            MaxValue = 1,
        };

        private readonly Box box;
        private readonly OsuSpriteText text;

        public BigBlackBox()
        {
            Size = new Vector2(150);

            Masking = true;
            CornerRadius = 20;
            CornerExponent = 5;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                text = new OsuSpriteText
                {
                    Text = "Big Black Box",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BoxAlpha.BindValueChanged(alpha => box.Alpha = alpha.NewValue, true);
            TextSpin.BindValueChanged(spin =>
            {
                if (spin.NewValue)
                    text.Spin(1000, RotationDirection.Clockwise);
                else
                    text.ClearTransforms();
            }, true);
        }
    }
}
