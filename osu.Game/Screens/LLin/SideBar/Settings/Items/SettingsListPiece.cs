using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.LLin.SideBar.Settings.Items
{
    public class SettingsListPiece<T> : SettingsPieceBasePanel, ISettingsItem<T>
    {
        public Bindable<T> Bindable { get; set; }

        public LocalisableString TooltipText { get; set; }

        private readonly CurrentValueText valueText = new CurrentValueText
        {
            RelativeSizeAxes = Axes.X
        };

        public List<T> Values { get; set; }

        protected override Drawable CreateSideDrawable() => valueText;

        protected virtual string GetValueText(T newValue) => newValue.ToString();

        [BackgroundDependencyLoader]
        private void load()
        {
            Bindable.BindValueChanged(onBindableChanged, true);
        }

        private void onBindableChanged(ValueChangedEvent<T> v)
        {
            if (v.NewValue == null) return;

            valueText.Text = GetValueText(v.NewValue);
            currentIndex = Values.IndexOf(v.NewValue);
        }

        private int currentIndex;

        protected override void OnLeftClick()
        {
            currentIndex++;
            if (currentIndex >= Values.Count) currentIndex = 0;

            Bindable.Value = Values[currentIndex];
        }

        protected override void OnRightClick()
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = Values.Count - 1;

            Bindable.Value = Values[currentIndex];
        }

        protected override void OnMiddleClick()
        {
            Bindable.Value = Bindable.Default;
            currentIndex = Values.IndexOf(Bindable.Value);
            base.OnMiddleClick();
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
