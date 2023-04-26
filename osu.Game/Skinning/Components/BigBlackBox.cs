// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Components
{
    /// <summary>
    /// Intended to be a test bed for skinning. May be removed at some point in the future.
    /// </summary>
    [UsedImplicitly]
    public partial class BigBlackBox : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Spinning text", "Whether the big text should spin")]
        public Bindable<bool> TextSpin { get; } = new BindableBool();

        [SettingSource("Alpha", "The alpha value of this box")]
        public BindableNumber<float> BoxAlpha { get; } = new BindableNumber<float>(1)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01f,
        };

        private readonly Box box;
        private readonly OsuSpriteText text;
        private readonly OsuTextFlowContainer disclaimer;

        public BigBlackBox()
        {
            Size = new Vector2(250);

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
                    Font = OsuFont.Default.With(size: 40)
                },
                disclaimer = new OsuTextFlowContainer(st => st.Font = OsuFont.Default.With(size: 10))
                {
                    Text = "This is intended to be a test component and may disappear in the future!",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding(10),
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    TextAnchor = Anchor.TopCentre,
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

            disclaimer.FadeOutFromOne(5000, Easing.InQuint);
        }
    }
}
