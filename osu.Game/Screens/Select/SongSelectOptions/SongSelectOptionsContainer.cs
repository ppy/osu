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

namespace osu.Game.Screens.Select
{
    public class SongSelectOptionsContainer : FlowContainer, IStateful<SongSelectOptionsState>
    {
        private const int transition_length = 400;
        private readonly Vector2 visibleSpacing = new Vector2(0.6f, 0);

        public Vector2 VisiblePos => new Vector2(ScreenSpaceDrawQuad.Width * 0.3125f, Position.Y); // 5/16ths the width of the screen
        public Vector2 HiddenPos => new Vector2(ScreenSpaceDrawQuad.Width * 2, Position.Y);

        private SongSelectOptionsState state = SongSelectOptionsState.Visible;
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
                        Delay(transition_length );
                        MoveTo(HiddenPos, transition_length, EasingTypes.Out);
                        break;
                    case SongSelectOptionsState.Visible:
                        Position = new Vector2(-ScreenSpaceDrawQuad.Width * 2, Position.Y);
                        Spacing = new Vector2(40, 0);
                        TransformSpacingTo(visibleSpacing, transition_length * 2, EasingTypes.In);
                        Delay(transition_length / 1);
                        MoveTo(VisiblePos, transition_length, EasingTypes.In);
                        break;
                }
            }
        }

        public override bool HandleInput => (Transforms.Count == 0);

        public override IEnumerable<Drawable> Children
        {
            get
            {
                return base.Children;
            }
            set
            {
                base.Children = value;
                foreach (SongSelectOptionsButton s in value)
                {
                    s.On_Clicked += ToggleState;
                }
            }
        }

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
            State = SongSelectOptionsState.Hidden;
        }

        public void ToggleState()
        {
            if (State == SongSelectOptionsState.Hidden)
                State = SongSelectOptionsState.Visible;
            else
                State = SongSelectOptionsState.Hidden;
        }

        protected override void Update()
        {
            base.Update();

            if (Transforms.Count == 0)
            {
                switch (State)
                {
                    case SongSelectOptionsState.Hidden:
                        Position = HiddenPos;
                        break;
                    case SongSelectOptionsState.Visible:
                        Position = VisiblePos;
                        break;
                }
            }
        }
    }

    public enum SongSelectOptionsState
    {
        Hidden,
        Visible,
    }
}
