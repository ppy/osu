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

namespace osu.Game.Screens.Mvis.SideBar.Settings.Items
{
    public class SettingsSliderPiece<T> : CompositeDrawable, ISettingsItem<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public Bindable<T> Bindable { get; set; }

        public string TooltipText { get; set; }

        private readonly OsuSpriteText text = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 20)
        };

        private readonly SpriteIcon spriteIcon = new SpriteIcon
        {
            Size = new Vector2(25),
            Icon = FontAwesome.Solid.SlidersH
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
        public bool TransferValueOnCommit;

        private LocalisableString description;
        private IconUsage icon;
        private Box bgBox;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private Box flashBox;

        public SettingsSliderPiece()
        {
            TooltipText = TooltipText + "点击重置";
        }

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
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 25,
                    Margin = new MarginPadding { Top = 10 },
                    Padding = new MarginPadding { Left = 10 + 25 + 5 },
                    Child = new SettingsSlider<T>
                    {
                        RelativeSizeAxes = Axes.Both,
                        Current = Bindable,
                        DisplayAsPercentage = DisplayAsPercentage,
                        TransferValueOnCommit = TransferValueOnCommit,
                    }
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
