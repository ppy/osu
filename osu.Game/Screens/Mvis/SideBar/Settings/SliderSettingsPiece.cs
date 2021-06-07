using System;
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

namespace osu.Game.Screens.Mvis.SideBar.Settings
{
    public class SliderSettingsPiece<T> : CompositeDrawable, ISettingsItem<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public Bindable<T> Bindable { get; set; }

        public string TooltipText => "点此还原";

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
            }
        }

        public bool DisplayAsPercentage;

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
                new SettingsSlider<T>
                {
                    Width = 0.5f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Current = Bindable,
                    DisplayAsPercentage = DisplayAsPercentage
                },
                flashBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0,
                },
                new HoverClickSounds()
            };

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.InActiveColor;
            }, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Bindable.Value = Bindable.Default;
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
