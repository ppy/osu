// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropDownMenu<T> : DropDownMenu<T>
    {
        protected override DropDownHeader CreateHeader() => new OsuDropDownHeader { AccentColour = AccentColour };

        private Color4? accentColour;
        public virtual Color4 AccentColour
        {
            get { return accentColour.GetValueOrDefault(); }
            set
            {
                accentColour = value;
                if (Header != null)
                    ((OsuDropDownHeader)Header).AccentColour = value;
                foreach (var item in ItemList.OfType<OsuDropDownMenuItem<T>>())
                    item.AccentColour = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (accentColour == null)
                AccentColour = colours.PinkDarker;
        }

        public OsuDropDownMenu()
        {
            ContentContainer.CornerRadius = 4;
            ContentBackground.Colour = Color4.Black.Opacity(0.5f);

            DropDownItemsContainer.Padding = new MarginPadding(5);
        }

        protected override void AnimateOpen() => ContentContainer.FadeIn(300, EasingTypes.OutQuint);

        protected override void AnimateClose() => ContentContainer.FadeOut(300, EasingTypes.OutQuint);

        protected override void UpdateContentHeight()
        {
            var actualHeight = (RelativeSizeAxes & Axes.Y) > 0 ? 1 : ContentHeight;
            ContentContainer.ResizeTo(new Vector2(1, State == DropDownMenuState.Opened ? actualHeight : 0), 300, EasingTypes.OutQuint);
        }

        protected override DropDownMenuItem<T> CreateDropDownItem(string key, T value) => new OsuDropDownMenuItem<T>(key, value) { AccentColour = AccentColour };
    }
}