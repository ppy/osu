using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Mvis.SideBar.Settings.Items
{
    public class SettingsEnumPiece<T> : SettingsPieceBasePanel, ISettingsItem<T>
        where T : struct, Enum
    {
        public Bindable<T> Bindable { get; set; }

        public string TooltipText { get; set; }

        private readonly CurrentValueText valueText = new CurrentValueText
        {
            RelativeSizeAxes = Axes.X
        };

        public SettingsEnumPiece()
        {
            var valueArray = (T[])Enum.GetValues(typeof(T));
            values = valueArray.ToList();
        }

        protected override Drawable CreateSideDrawable() => valueText;

        [BackgroundDependencyLoader]
        private void load()
        {
            Bindable.BindValueChanged(onBindableChanged, true);
        }

        private void onBindableChanged(ValueChangedEvent<T> v)
        {
            valueText.Text = v.NewValue.GetDescription();
            currentIndex = values.IndexOf(v.NewValue);
        }

        private readonly List<T> values;
        private int currentIndex;

        protected override bool OnClick(ClickEvent e)
        {
            currentIndex++;
            if (currentIndex >= values.Count) currentIndex = 0;

            Bindable.Value = values[currentIndex];
            return base.OnClick(e);
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
