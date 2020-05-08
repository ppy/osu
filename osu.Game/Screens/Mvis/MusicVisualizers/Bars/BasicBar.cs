using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.UI.Objects.MusicVisualizers.Bars
{
    public class BasicBar : Container
    {
        public BasicBar()
        {
            Child = CreateContent();
        }

        private Container barContainer;
        private Container brickContainer;
        private BindableBool EnableBrick = new BindableBool();

        protected virtual Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                barContainer = new Container
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Child = new Box
                    {
                        EdgeSmoothness = Vector2.One,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    },
                },
                brickContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Masking = true,
                    Child = new Box
                    {
                        EdgeSmoothness = Vector2.One,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                    }
                }
            }
        };

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            config.BindWith(MfSetting.MvisEnableBrick, EnableBrick);

            EnableBrick.ValueChanged += _ => UpdateVisuals();

            UpdateVisuals();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            barContainer.CornerRadius = Width / 2;
            brickContainer.CornerRadius = Width / 2;
            brickContainer.Height = Width;
        }

        private void UpdateVisuals()
        {
            switch ( EnableBrick.Value )
            {
                case false:
                    brickContainer.FadeOut(500, Easing.OutQuint);
                    break;

                case true:
                    brickContainer.FadeIn(500, Easing.OutQuint);
                    break;
            }
        }

        public virtual void SetValue(float amplitudeValue, float valueMultiplier, int softness)
        {
            var newHeight = ValueFormula(amplitudeValue, valueMultiplier);

            // Don't allow resize if new height less than current
            if (newHeight <= Height)
                return;

            this.ResizeHeightTo(newHeight).Then().ResizeHeightTo(Width, softness);
            brickContainer.MoveToY( -newHeight ).Then().MoveToY(-Width , softness*3, Easing.OutQuint);
        }

        protected virtual float ValueFormula(float amplitudeValue, float valueMultiplier) => amplitudeValue * valueMultiplier;
    }
}
