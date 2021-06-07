using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

namespace osu.Game.Screens.Mvis.SideBar.Settings.Items
{
    public class SettingsTogglePiece : CompositeDrawable, ISettingsItem<bool>
    {
        public Bindable<bool> Bindable { get; set; }

        public string TooltipText { get; set; }

        private readonly OsuSpriteText text = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 20)
        };

        private readonly SpriteIcon spriteIcon = new SpriteIcon
        {
            Size = new Vector2(25),
            Icon = FontAwesome.Solid.ToggleOn
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
            }
        }

        private LocalisableString description;
        private IconUsage icon;
        private Box bgBox;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private Box flashBox;

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = 7.5f;
            AutoSizeAxes = Axes.Y;
            Width = 150;
            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
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
                new HoverClickSounds()
            };

            Bindable.BindValueChanged(onBindableChanged);

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = Bindable.Value ? colourProvider.ActiveColor : colourProvider.InActiveColor;
                text.Colour = Bindable.Value ? Color4.Black : Color4.White;
                spriteIcon.Colour = Bindable.Value ? Color4.Black : Color4.White;
            }, true);
        }

        private void onBindableChanged(ValueChangedEvent<bool> v)
        {
            switch (v.NewValue)
            {
                case true:
                    bgBox.FadeColour(colourProvider.ActiveColor, 300, Easing.OutQuint);
                    text.FadeColour(Color4.Black, 300, Easing.OutQuint);
                    spriteIcon.FadeColour(Color4.Black, 300, Easing.OutQuint);
                    break;

                case false:
                    bgBox.FadeColour(colourProvider.InActiveColor, 300, Easing.OutQuint);
                    text.FadeColour(Color4.White, 300, Easing.OutQuint);
                    spriteIcon.FadeColour(Color4.White, 300, Easing.OutQuint);
                    break;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            Bindable.Value = !Bindable.Value;
            return base.OnClick(e);
        }

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
    }
}
