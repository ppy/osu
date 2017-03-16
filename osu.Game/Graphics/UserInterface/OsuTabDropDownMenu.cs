// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.UserInterface.Tab;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabDropDownMenu<T> : TabDropDownMenu<T>
    {
        public override float HeaderWidth => 14;
        public override float HeaderHeight => 24;

        private Color4? accentColour;
        public Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                Header.Colour = value;
                foreach (var item in ItemList.OfType<OsuTabDropDownMenuItem<T>>())
                    item.AccentColour = value;
            }
        }

        protected override DropDownHeader CreateHeader() => new OsuTabDropDownHeader
        {
            Colour = AccentColour
        };

        protected override DropDownMenuItem<T> CreateDropDownItem(string key, T value) =>
            new OsuTabDropDownMenuItem<T>(key, value) { AccentColour = AccentColour };

        public OsuTabDropDownMenu()
        {
            MaxDropDownHeight = int.MaxValue;
            ContentContainer.CornerRadius = 4;
            ContentBackground.Colour = Color4.Black.Opacity(0.9f);
            ScrollContainer.ScrollDraggerVisible = false;
            DropDownItemsContainer.Padding = new MarginPadding { Left = 5, Bottom = 7, Right = 5, Top = 7 };
        }

        protected override void AnimateOpen()
        {
            ContentContainer.FadeIn(300, EasingTypes.OutQuint);
        }

        protected override void AnimateClose()
        {
            ContentContainer.FadeOut(300, EasingTypes.OutQuint);
        }

        protected override void UpdateContentHeight()
        {
            ContentContainer.ResizeTo(new Vector2(1, State == DropDownMenuState.Opened ? ContentHeight : 0), 300, EasingTypes.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;
        }
    }
}
