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
        public PlaySongSelectButton SelectedButton;

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
                    p.ButtonContainer = this;
            }
        }

        public Action On_SelectionChanged;

        public PlaySongSelectButtonContainer()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FlowDirection.HorizontalOnly;
            Spacing = new Vector2(0.2f, 0);
        }

        public void Select(PlaySongSelectButton P)
        {
            if (SelectedButton == P)
                return;
            if (SelectedButton != null)
            {
                SelectedButton.OnDeselected();
            }
            SelectedButton = P;
            SelectedButton.OnSelected();
            On_SelectionChanged?.Invoke();
        }
    }
}
