using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public class SettingsPieceBasePanel : CompositeDrawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 20)
        };

        private readonly SpriteIcon spriteIcon = new SpriteIcon
        {
            Size = new Vector2(25)
        };

        public LocalisableString Description
        {
            get => description;
            set
            {
                description = value;
                text.Text = value;
            }
        }

        public IconUsage Icon
        {
            get => icon;
            set
            {
                icon = value;
                spriteIcon.Icon = value;
                haveIconSet = true;
            }
        }

        protected virtual Drawable CreateSideDrawable() => new PlaceHolder();
        protected virtual IconUsage DefaultIcon => FontAwesome.Regular.QuestionCircle;

        private LocalisableString description;
        private IconUsage icon;
        protected Box BgBox;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        protected CustomColourProvider ColourProvider => colourProvider;

        private bool haveIconSet;
        private Box flashBox;
        protected FillFlowContainer FillFlow;

        private Sample sampleOnClick;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Masking = true;
            CornerRadius = 7.5f;
            AutoSizeAxes = Axes.Y;
            Width = 150;
            InternalChildren = new Drawable[]
            {
                BgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 25,
                    Margin = new MarginPadding { Top = 10 },
                    Padding = new MarginPadding { Left = 10 + 25 + 5 },
                    Child = CreateSideDrawable()
                },
                FillFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        spriteIcon,
                        text,
                    }
                },
                flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                },
                new HoverSounds()
            };

            colourProvider.HueColour.BindValueChanged(_ => OnColorChanged(), true);

            if (!haveIconSet) spriteIcon.Icon = DefaultIcon;

            sampleOnClick = audio.Samples.Get("UI/default-select");
        }

        protected virtual void OnColorChanged()
        {
            BgBox.Colour = colourProvider.InActiveColor;
            FillFlow.Colour = Color4.White;
        }

        protected virtual void OnLeftClick() { }
        protected virtual void OnRightClick() { }
        protected virtual void OnMiddleClick() { }

        protected override bool OnHover(HoverEvent e)
        {
            flashBox.FadeTo(0.1f, 300);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            flashBox.FadeTo(0f, 300);
        }

        private MouseButton mouseDownButton;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            mouseDownButton = e.Button;

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button == mouseDownButton && IsHovered)
            {
                sampleOnClick?.Play();

                switch (e.Button)
                {
                    case MouseButton.Left:
                        OnLeftClick();
                        break;

                    case MouseButton.Right:
                        OnRightClick();
                        break;

                    case MouseButton.Middle:
                        OnMiddleClick();
                        break;
                }
            }

            base.OnMouseUp(e);
        }
    }
}
