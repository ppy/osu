// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select.Options
{
    public class BeatmapOptionsOverlay : FocusedOverlayContainer
    {
        private const float transition_duration = 500;
        private const float x_position = 0.2f;
        private const float x_movement = 0.8f;

        private const float height = 100;

        private readonly Box holder;
        private readonly FillFlowContainer<BeatmapOptionsButton> buttonsContainer;

        protected override void PopIn()
        {
            base.PopIn();

            FadeIn(transition_duration, EasingTypes.OutQuint);

            if (buttonsContainer.Position.X == 1 || Alpha == 0)
                buttonsContainer.MoveToX(x_position - x_movement);

            holder.ScaleTo(new Vector2(1, 1), transition_duration / 2, EasingTypes.OutQuint);

            buttonsContainer.MoveToX(x_position, transition_duration, EasingTypes.OutQuint);
            buttonsContainer.TransformSpacingTo(Vector2.Zero, transition_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            holder.ScaleTo(new Vector2(1, 0), transition_duration / 2, EasingTypes.InSine);

            buttonsContainer.MoveToX(x_position + x_movement, transition_duration, EasingTypes.InSine);
            buttonsContainer.TransformSpacingTo(new Vector2(200f, 0f), transition_duration, EasingTypes.InSine);

            FadeOut(transition_duration, EasingTypes.InQuint);
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
                    RelativePositionAxes = Axes.X,
                    AutoSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                },
            };
        }

        /// <param name="firstLine">Text in the first line.</param>
        /// <param name="secondLine">Text in the second line.</param>
        /// <param name="colour">Colour of the button.</param>
        /// <param name="icon">Icon of the button.</param>
        /// <param name="hotkey">Hotkey of the button.</param>
        /// <param name="action">Action the button does.</param>
        /// <param name="depth">
        /// <para>Lower depth to be put on the left, and higher to be put on the right.</para>
        /// <para>Notice this is different to <see cref="Footer"/>!</para>
        /// </param>
        public void AddButton(string firstLine, string secondLine, FontAwesome icon, Color4 colour, Action action, Key? hotkey = null, float depth = 0)
        {
            buttonsContainer.Add(new BeatmapOptionsButton
            {
                FirstLineText = firstLine,
                SecondLineText = secondLine,
                Icon = icon,
                ButtonColour = colour,
                Depth = depth,
                Action = () =>
                {
                    Hide();
                    action?.Invoke();
                },
                HotKey = hotkey
            });
        }

        private class ButtonFlow : FillFlowContainer<BeatmapOptionsButton>
        {
            protected override IComparer<Drawable> DepthComparer => new ReverseCreationOrderDepthComparer();
            protected override IEnumerable<BeatmapOptionsButton> FlowingChildren => base.FlowingChildren.Reverse();

            public ButtonFlow()
            {
                Direction = FillDirection.Horizontal;
            }
        }
    }
}
