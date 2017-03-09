// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.UserInterface.Tab;
using osu.Game.Graphics;
using osu.Game.Screens.Select.Filter;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabDropDownMenu<T> : TabDropDownMenu<T>
    {
        public override float HeaderWidth => 14;
        public override float HeaderHeight => 24;

        protected override DropDownHeader CreateHeader() => new FilterTabDropDownHeader();

        protected override IEnumerable<DropDownMenuItem<T>> GetDropDownItems(IEnumerable<KeyValuePair<string, T>> values)
            => values.Select(v => new FilterTabDropDownMenuItem<T>(v.Key, v.Value));

        public FilterTabDropDownMenu()
        {
            MaxDropDownHeight = int.MaxValue;
            ContentContainer.CornerRadius = 4;
            ContentBackground.Colour = Color4.Black.Opacity(0.9f);
            ScrollContainer.ScrollDraggerVisible = false;
            DropDownItemsContainer.Padding = new MarginPadding { Left = 5, Bottom = 7, Right = 5, Top = 7 };

            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("TabControl only supports enums as the generic type argument");

            List<KeyValuePair<string, T>> items = new List<KeyValuePair<string, T>>();
            foreach (var val in (T[])Enum.GetValues(typeof(T)))
            {
                if (!val.Equals(default(T)))
                    items.Add(new KeyValuePair<string, T>((val as Enum)?.GetDescription(), val));
            }

            Items = items;
            // TODO: ValueChanged Handling
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
            if (State == DropDownMenuState.Opened)
                ContentContainer.ResizeTo(new Vector2(1, ContentHeight), 300, EasingTypes.OutQuint);
            else
                ContentContainer.ResizeTo(new Vector2(1, 0), 300, EasingTypes.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (typeof(T) == typeof(SortMode))
                Header.Colour = colours.GreenLight;
            else
                Header.Colour = colours.Blue;
        }
    }
}
