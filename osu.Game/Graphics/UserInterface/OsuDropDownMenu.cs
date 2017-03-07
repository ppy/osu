// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuDropDownMenu<U> : DropDownMenu<U>
    {
        protected override DropDownHeader CreateHeader() => new OsuDropDownHeader();

        protected override IEnumerable<DropDownMenuItem<U>> GetDropDownItems(IEnumerable<KeyValuePair<string, U>> values)
            => values.Select(v => new OsuDropDownMenuItem<U>(v.Key, v.Value));

        public OsuDropDownMenu()
        {
            ContentContainer.CornerRadius = 4;
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
            ContentContainer.ResizeTo(State == DropDownMenuState.Opened ? new Vector2(1, ContentHeight) : new Vector2(1, 0), 300, EasingTypes.OutQuint);
        }
    }
}