//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropDownMenu<U> : DropDownMenu<U>
    {
        protected override DropDownHeader CreateHeader() => new OsuDropDownHeader();

        protected override IEnumerable<DropDownMenuItem<U>> GetDropDownItems(IEnumerable<U> values)
        {
            return values.Select(v =>
            {
                var field = typeof(U).GetField(Enum.GetName(typeof(U), v));
                return new OsuDropDownMenuItem<U>(
                    field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? field.Name, v);
            });

        }

        public OsuDropDownMenu()
        {
            //TODO: breaks padding; figure why.
            //ContentContainer.CornerRadius = 4;
            ContentBackground.Colour = Color4.Black.Opacity(0.5f);
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
    }
}