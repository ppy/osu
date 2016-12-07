using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class SliderOption<T> : FlowContainer where T : struct,
        IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        private SliderBar<T> slider;
        private SpriteText text;
    
        public string LabelText
        {
            get { return text.Text; }
            set
            {
                text.Text = value;
                text.Alpha = string.IsNullOrEmpty(value) ? 0 : 1;
            }
        }
        
        public BindableNumber<T> Bindable
        {
            get { return slider.Bindable; }
            set { slider.Bindable = value; }
        }

        public SliderOption()
        {
            Direction = FlowDirection.VerticalOnly;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new SpriteText { Alpha = 0 },
                slider = new OsuSliderBar<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    RelativeSizeAxes = Axes.X,
                }
            };
        }

        private class OsuSliderBar<U> : SliderBar<U> where U : struct,
            IComparable, IFormattable, IConvertible, IComparable<U>, IEquatable<U>
        {
            private Container nub;
        
            public OsuSliderBar()
            {
                Height = 22;
                Color = Color4.White;
                SelectionColor = new Color4(255, 102, 170, 255);
                Add(nub = new Container
                {
                    Width = Height,
                    Height = Height,
                    CornerRadius = Height / 2,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.None,
                    RelativeSizeAxes = Axes.None,
                    RelativePositionAxes = Axes.X,
                    Masking = true,
                    BorderColour = new Color4(255, 102, 170, 255),
                    BorderThickness = 2,
                    Children = new[]
                    {
                        new Box { Colour = Color4.Transparent, RelativeSizeAxes = Axes.Both }
                    }
                });
                Box.Height = SelectionBox.Height = 2;
                Box.RelativePositionAxes = Axes.None;
                Box.RelativeSizeAxes = SelectionBox.RelativeSizeAxes = Axes.None;
                Box.Position = SelectionBox.Position = new Vector2(0, Height / 2 - 1);
                Box.Colour = new Color4(255, 102, 170, 100);
            }

            protected override void UpdateAmount(float amt)
            {
                nub.MoveToX(amt, 300, EasingTypes.OutQuint);
                SelectionBox.ScaleTo(
                    new Vector2(DrawWidth * amt - Height / 2 + 1, 1),
                    300, EasingTypes.OutQuint);
                Box.MoveToX(DrawWidth * amt + Height / 2 - 1,
                    300, EasingTypes.OutQuint);
                Box.ScaleTo(
                    new Vector2(DrawWidth * (1 - amt) - Height / 2 + 1, 1),
                    300, EasingTypes.OutQuint);
            }
        }
    }
}