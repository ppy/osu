//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelectButtonContainer : FlowContainer
    {
        private PlaySongSelectButton hoveredButton;
        public PlaySongSelectButton HoveredButton
        {
            get { return hoveredButton; }
            set
            {
                HoveredOver(value);
            }
        }

        public override IEnumerable<Drawable> Children
        {
            get
            {
                return base.Children;
            }
            set
            {
                base.Children = value;
                foreach (PlaySongSelectButton p in value)
                {
                    p.ButtonContainer = this;
                    p.On_Hovered = () => HoveredButton = p;
                }
            }
        }

        public Action On_HoveredChanged;

        public PlaySongSelectButtonContainer()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FlowDirection.HorizontalOnly;
            Spacing = new Vector2(0.2f, 0);
        }

        public void HoveredOver(PlaySongSelectButton P)
        {
            if (HoveredButton == P)
                return;
            hoveredButton = P;
            On_HoveredChanged?.Invoke();
        }
    }
}
