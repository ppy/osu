using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Mvis.Misc
{
    public class Indicator : CompositeDrawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText
        {
            Margin = new MarginPadding { Horizontal = 10, Vertical = 7 }
        };

        private Box fgBox;

        public LocalisableString Text
        {
            set => text.Text = value;
        }

        public bool IsMouseIndicator;
        private Box box;

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider colourProvider)
        {
            AutoSizeAxes = Axes.Both;
            Margin = new MarginPadding { Bottom = 10 };
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            Y = -10;
            Alpha = 0;

            InternalChildren = new[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    CornerRadius = 5,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 1.5f,
                        Colour = Color4.Black.Opacity(0.6f),
                        Offset = new Vector2(0, 1.5f)
                    },
                    Children = new Drawable[]
                    {
                        fgBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4
                        },
                        text,
                        new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.Y,
                            Child = box = new Box
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = 2,
                                Height = 0.15f,
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Alpha = IsMouseIndicator ? 1 : 0
                            }
                        }
                    }
                }
            };

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                fgBox.Colour = IsMouseIndicator ? colourProvider.Highlight1 : colourProvider.Background4;
                text.Colour = IsMouseIndicator ? Color4.Black : Color4.White;
            }, true);
        }

        public override void Show() =>
            this.FadeIn(300, Easing.OutQuint).MoveToY(0, 300, Easing.OutQuint);

        public override void Hide() =>
            this.FadeOut(300, Easing.OutQuint).MoveToY(-10, 300, Easing.OutQuint);

        private InputManager inputManager;

        protected override void LoadComplete()
        {
            inputManager = GetContainingInputManager();

            if (!IsMouseIndicator)
            {
                box.Parent.Expire();
                box = null;
            }

            base.LoadComplete();
        }

        protected override void Update()
        {
            if (inputManager != null)
                box?.MoveToX(ToLocalSpace(inputManager.CurrentState.Mouse.Position).X);

            base.Update();
        }
    }
}
