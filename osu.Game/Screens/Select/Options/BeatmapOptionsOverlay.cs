// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Options
{
    public class BeatmapOptionsOverlay : FocusedOverlayContainer
    {
        private const float transition_duration = 500;
        private const float x_position = 290;

        private const float height = 100;

        private Box holder;
        private FillFlowContainer<BeatmapOptionsButton> buttonsContainer;

        public Action OnRemoveFromUnplayed;
        public Action OnClearLocalScores;
        public Action OnEdit;
        public Action OnDelete;

        protected override void PopIn()
        {
            base.PopIn();

            if (buttonsContainer.Position.X >= DrawWidth || buttonsContainer.Alpha <= 0)
                buttonsContainer.MoveToX(-buttonsContainer.DrawWidth);

            buttonsContainer.Alpha = 1;

            holder.ScaleTo(new Vector2(1, 1), transition_duration / 2, EasingTypes.OutQuint);

            buttonsContainer.MoveToX(x_position, transition_duration, EasingTypes.OutQuint);
            buttonsContainer.TransformSpacingTo(Vector2.Zero, transition_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            holder.ScaleTo(new Vector2(1, 0), transition_duration / 2, EasingTypes.InSine);

            buttonsContainer.MoveToX(DrawWidth, transition_duration, EasingTypes.InSine);
            buttonsContainer.TransformSpacingTo(new Vector2(200f, 0f), transition_duration, EasingTypes.InSine);

            Delay(transition_duration);
            Schedule(() =>
            {
                if (State == Visibility.Hidden)
                    buttonsContainer.Alpha = 0;
            });
        }

        public BeatmapOptionsOverlay()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Children = new Drawable[]
            {
                holder = new Box
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Scale = new Vector2(1, 0),
                    Colour = Color4.Black.Opacity(0.5f),
                },
                buttonsContainer = new ButtonFlow
                {
                    Height = height,
                    AutoSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Children = new BeatmapOptionsButton[]
                    {
                        new BeatmapOptionsRemoveFromUnplayedButton
                        {
                            Action = () =>
                            {
                                Hide();
                                OnRemoveFromUnplayed?.Invoke();
                            },
                        },
                        new BeatmapOptionsClearLocalScoresButton
                        {
                            Action = () =>
                            {
                                Hide();
                                OnClearLocalScores?.Invoke();
                            },
                        },
                        new BeatmapOptionsEditButton
                        {
                            Action = () =>
                            {
                                Hide();
                                OnEdit?.Invoke();
                            },
                        },
                        new BeatmapOptionsDeleteButton
                        {
                            Action = () =>
                            {
                                Hide();
                                OnDelete?.Invoke();
                            },
                        },
                    },
                },
            };
        }

        class ButtonFlow : FillFlowContainer<BeatmapOptionsButton>
        {
            protected override IComparer<Drawable> DepthComparer => new ReverseCreationOrderDepthComparer();
            protected override IEnumerable<BeatmapOptionsButton> FlowingChildren => base.FlowingChildren.Reverse();

            public ButtonFlow()
            {
                Direction = FillDirection.Right;
            }
        }
    }
}
