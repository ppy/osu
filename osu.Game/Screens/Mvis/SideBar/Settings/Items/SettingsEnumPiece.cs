using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
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
    public class SettingsEnumPiece<T> : CompositeDrawable, ISettingsItem<T>
        where T : struct, Enum
    {
        public Bindable<T> Bindable { get; set; }

        public string TooltipText { get; set; }

        private readonly OsuSpriteText text = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 20)
        };

        private readonly CurrentValueText valueText = new CurrentValueText
        {
            RelativeSizeAxes = Axes.X
        };

        private readonly SpriteIcon spriteIcon = new SpriteIcon
        {
            Size = new Vector2(25),
            Icon = FontAwesome.Regular.QuestionCircle
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

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private Box flashBox;

        public SettingsEnumPiece()
        {
            var valueArray = (T[])Enum.GetValues(typeof(T));
            values = valueArray.ToList();
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
                    Child = valueText,
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

            Bindable.BindValueChanged(onBindableChanged, true);

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox.Colour = colourProvider.InActiveColor;
            }, true);
        }

        private void onBindableChanged(ValueChangedEvent<T> v)
        {
            valueText.Text = v.NewValue.GetDescription();
            currentIndex = values.IndexOf(v.NewValue);
        }

        private readonly List<T> values;
        private int currentIndex;
        private Box bgBox;

        protected override bool OnClick(ClickEvent e)
        {
            currentIndex++;
            if (currentIndex >= values.Count) currentIndex = 0;

            Bindable.Value = values[currentIndex];
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

        private class CurrentValueText : CompositeDrawable
        {
            public LocalisableString Text
            {
                get => text;
                set
                {
                    lastText?.MoveToY(5, 200, Easing.OutQuint)
                            .FadeOut(200, Easing.OutQuint).Then().Expire();

                    var currentText = new OsuSpriteText
                    {
                        Text = value,
                        Alpha = 0,
                        Font = OsuFont.GetFont(size: 20),
                        Y = -5,
                        RelativeSizeAxes = Axes.X,
                        Truncate = true
                    };
                    AddInternal(currentText);

                    currentText.MoveToY(0, 200, Easing.OutQuint)
                               .FadeIn(200, Easing.OutQuint);
                    lastText = currentText;

                    text = value;
                }
            }

            private LocalisableString text;
            private OsuSpriteText lastText;
        }
    }
}
