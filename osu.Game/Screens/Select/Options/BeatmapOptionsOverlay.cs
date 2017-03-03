// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

        private Container background;
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

            background.FadeIn(transition_duration, EasingTypes.OutQuint);
            buttonsContainer.MoveToX(x_position, transition_duration, EasingTypes.OutQuint);
            buttonsContainer.TransformSpacingTo(Vector2.Zero, transition_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            background.FadeOut(transition_duration, EasingTypes.InSine);
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
            Children = new Drawable[]
            {
                background = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f),
                        },
                    },
                },
                buttonsContainer = new FillFlowContainer<BeatmapOptionsButton>
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Direction = FillDirection.Left,
                    Padding = new MarginPadding
                    {
                        Left = BeatmapOptionsButton.SIZE.X, // For some reason autosize on this flow container is one button too short
                    },
                    Children = new BeatmapOptionsButton[]
                    {
                        new BeatmapOptionsDeleteButton
                        {
                            Action = () =>
                            {
                                Hide();
                                OnDelete?.Invoke();
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
                        new BeatmapOptionsClearLocalScoresButton
                        {
                            Action = () =>
                            {
                                Hide();
                                OnClearLocalScores?.Invoke();
                            },
                        },
                        new BeatmapOptionsRemoveFromUnplayedButton
                        {
                            Action = () =>
                            {
                                Hide();
                                OnRemoveFromUnplayed?.Invoke();
                            },
                        },
                    },
                },
            };
        }
    }
}
