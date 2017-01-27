//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Select
{
    public class SongSelectOptionsContainer : FlowContainer, IStateful<SongSelectOptionsState>
    {
        private const int transition_length = 400;
        private readonly Vector2 visibleSpacing = new Vector2(0.6f, 0);

        private static readonly Vector2 shearing = new Vector2(0.15f, 0);

        private const float VisiblePosX = 0.1f;
        private const float HiddenPosX = 2f;

        private SongSelectOptionsState state = SongSelectOptionsState.Hidden;
        public SongSelectOptionsState State
        {
            get { return state; }
            set
            {
                if (state == value)
                    return;
                state = value;
                switch (state)
                {
                    case SongSelectOptionsState.Hidden:
                        TransformSpacingTo(new Vector2(40, 0), transition_length * 1.5, EasingTypes.Out);
                        Delay(transition_length);
                        MoveToX(HiddenPosX, transition_length, EasingTypes.Out);
                        break;
                    case SongSelectOptionsState.Visible:
                        Position = new Vector2(-1.2f, Position.Y);
                        Spacing = new Vector2(40, 0);
                        TransformSpacingTo(visibleSpacing, transition_length * 2, EasingTypes.In);
                        Delay(transition_length);
                        MoveToX(VisiblePosX, transition_length, EasingTypes.In);
                        break;
                }
            }
        }

        public override bool HandleInput => (Transforms.Count == 0);

        public override void Add(Drawable drawable)
        {
            base.Add(drawable);
            SongSelectOptionsButton s = drawable as SongSelectOptionsButton;
            if (s != null)
                s.On_Clicked += ToggleState;
        }

        public SongSelectOptionsContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Spacing = visibleSpacing;
            Direction = FlowDirection.HorizontalOnly;
            AutoSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.X;
            Shear = shearing;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Position = new Vector2(HiddenPosX, Position.Y);
        }

        public void ToggleState()
        {
            if (Transforms.Count > 0)
                return;
            if (State == SongSelectOptionsState.Hidden)
                State = SongSelectOptionsState.Visible;
            else
                State = SongSelectOptionsState.Hidden;
        }
    }

    public enum SongSelectOptionsState
    {
        Hidden,
        Visible,
    }
}
