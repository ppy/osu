// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Events;
using System.Linq;
using osu.Framework.Localisation;

namespace osu.Game.Screens.Select.Options
{
    public partial class BeatmapOptionsOverlay : OsuFocusedOverlayContainer
    {
        private const float transition_duration = 500;
        private const float x_position = 0.2f;
        private const float x_movement = 0.8f;

        private const float height = 100;

        private readonly Box holder;
        private readonly FillFlowContainer<BeatmapOptionsButton> buttonsContainer;

        public override bool BlockScreenWideMouse => false;

        protected override string PopInSampleName => "SongSelect/options-pop-in";
        protected override string PopOutSampleName => "SongSelect/options-pop-out";

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
                buttonsContainer = new ReverseChildIDFillFlowContainer<BeatmapOptionsButton>
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
        /// <param name="action">Binding the button does.</param>
        public void AddButton(LocalisableString firstLine, string secondLine, IconUsage icon, Color4 colour, Action action)
        {
            var button = new BeatmapOptionsButton
            {
                FirstLineText = firstLine,
                SecondLineText = secondLine,
                Icon = icon,
                ButtonColour = colour,
                Action = () =>
                {
                    Hide();
                    action?.Invoke();
                },
            };

            buttonsContainer.Add(button);
        }

        protected override void PopIn()
        {
            this.FadeIn(transition_duration, Easing.OutQuint);

            if (buttonsContainer.Position.X == 1 || Alpha == 0)
                buttonsContainer.MoveToX(x_position - x_movement);

            holder.ScaleTo(new Vector2(1, 1), transition_duration / 2, Easing.OutQuint);

            buttonsContainer.MoveToX(x_position, transition_duration, Easing.OutQuint);
            buttonsContainer.TransformSpacingTo(Vector2.Zero, transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            base.PopOut();

            holder.ScaleTo(new Vector2(1, 0), transition_duration / 2, Easing.InSine);

            buttonsContainer.MoveToX(x_position + x_movement, transition_duration, Easing.InSine);
            buttonsContainer.TransformSpacingTo(new Vector2(200f, 0f), transition_duration, Easing.InSine);

            this.FadeOut(transition_duration, Easing.InQuint);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // don't absorb control as ToolbarRulesetSelector uses control + number to navigate
            if (e.ControlPressed) return false;

            if (!e.Repeat && e.Key >= Key.Number1 && e.Key <= Key.Number9)
            {
                int requested = e.Key - Key.Number1;

                // go reverse as buttonsContainer is a ReverseChildIDFillFlowContainer
                BeatmapOptionsButton found = buttonsContainer.Children.ElementAtOrDefault((buttonsContainer.Children.Count - 1) - requested);

                if (found != null)
                {
                    found.TriggerClick();
                    return true;
                }
            }

            return base.OnKeyDown(e);
        }
    }
}
