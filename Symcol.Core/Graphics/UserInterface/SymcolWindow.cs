using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using Symcol.Core.Graphics.Containers;

namespace Symcol.Core.Graphics.UserInterface
{
    public class SymcolWindow : SymcolContainer
    {
        /// <summary>
        /// Put all your stuff in this
        /// </summary>
        public SymcolContainer WindowContent { get; set; }
        public SpriteText WindowTitle;

        private readonly SymcolContainer topBar;
        private readonly SymcolClickableContainer minimize;

        public SymcolWindow(Vector2 size)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            CornerRadius = 6;
            Masking = true;
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                topBar = new SymcolContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Height = 20,
                    Width = size.X,

                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.5f
                        },
                        WindowTitle = new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextSize = 18
                        },
                        new SymcolClickableContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 30,
                            Action = Close,

                            Child = new Box
                            {
                                Colour = Color4.Red,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.5f
                            }
                        },
                        minimize = new SymcolClickableContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 30,
                            Position = new Vector2(-30, 0),
                            Action = Minimize,

                            Child = new Box
                            {
                                Colour = Color4.White,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.5f
                            }
                        }
                    }
                },
                WindowContent = new SymcolContainer
                {
                    Size = size,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                }
            };

            WindowContent.Position = new Vector2(0, topBar.Height);
        }

        protected void Close()
        {
            this.FadeOut(200);
        }

        protected void Open()
        {
            this.FadeIn(200);
        }

        public void Toggle()
        {
            if (Alpha > 0)
                this.FadeOut(200);
            else
                this.FadeIn(200);
        }

        protected override bool OnDragStart(InputState state) => true;

        private bool drag;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button == MouseButton.Left)
                drag = true;

            return base.OnMouseDown(state, args);
        }

        protected override bool OnDrag(InputState state)
        {
            if (drag)
                Position += state.Mouse.Delta;

            return base.OnDrag(state);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button == MouseButton.Left)
                drag = false;

            return base.OnMouseUp(state, args);
        }

        public void Maximize()
        {
            WindowContent.FadeIn(200);
            WindowContent.ScaleTo(Vector2.One, 200);
            minimize.Action = Minimize;
        }

        public void Minimize()
        {
            WindowContent.FadeOut(200);
            WindowContent.ScaleTo(new Vector2(1, 0), 200);
            minimize.Action = Maximize;
        }
    }
}
