// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using Humanizer;
using osu.Framework.Graphics.Effects;

namespace osu.Game.Screens.Play
{
    public abstract class GameplayMenuOverlay : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        protected const int TRANSITION_DURATION = 200;

        private const int button_height = 70;
        private const float background_alpha = 0.75f;

        protected override bool BlockNonPositionalInput => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public Action OnRetry;
        public Action OnQuit;

        /// <summary>
        /// Action that is invoked when <see cref="GlobalAction.Back"/> is triggered.
        /// </summary>
        protected virtual Action BackAction => () => InternalButtons.Children.LastOrDefault()?.Click();

        /// <summary>
        /// Action that is invoked when <see cref="GlobalAction.Select"/> is triggered.
        /// </summary>
        protected virtual Action SelectAction => () => InternalButtons.Children.FirstOrDefault(f => f.Selected.Value)?.Click();

        public abstract string Header { get; }

        public abstract string Description { get; }

        protected ButtonContainer InternalButtons;
        public IReadOnlyList<DialogButton> Buttons => InternalButtons;

        private FillFlowContainer retryCounterContainer;

        protected GameplayMenuOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            State.ValueChanged += s => InternalButtons.Deselect();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = background_alpha,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 50),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 20),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = Header,
                                    Font = OsuFont.GetFont(size: 30),
                                    Spacing = new Vector2(5, 0),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Colour = colours.Yellow,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                },
                                new OsuSpriteText
                                {
                                    Text = Description,
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Shadow = true,
                                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                                }
                            }
                        },
                        InternalButtons = new ButtonContainer
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Masking = true,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.6f),
                                Radius = 50
                            },
                        },
                        retryCounterContainer = new FillFlowContainer
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                },
            };

            updateRetryCount();
        }

        private int retries;

        public int Retries
        {
            set
            {
                if (value == retries)
                    return;

                retries = value;
                if (retryCounterContainer != null)
                    updateRetryCount();
            }
        }

        protected override void PopIn() => this.FadeIn(TRANSITION_DURATION, Easing.In);
        protected override void PopOut() => this.FadeOut(TRANSITION_DURATION, Easing.In);

        // Don't let mouse down events through the overlay or people can click circles while paused.
        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnMouseMove(MouseMoveEvent e) => true;

        protected void AddButton(string text, Color4 colour, Action action)
        {
            var button = new Button
            {
                Text = text,
                ButtonColour = colour,
                Origin = Anchor.TopCentre,
                Anchor = Anchor.TopCentre,
                Height = button_height,
                Action = delegate
                {
                    action?.Invoke();
                    Hide();
                }
            };

            button.Selected.ValueChanged += selected => buttonSelectionChanged(button, selected.NewValue);

            InternalButtons.Add(button);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.SelectPrevious:
                    InternalButtons.SelectPrevious();
                    return true;

                case GlobalAction.SelectNext:
                    InternalButtons.SelectNext();
                    return true;

                case GlobalAction.Back:
                    BackAction.Invoke();
                    return true;

                case GlobalAction.Select:
                    SelectAction.Invoke();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        private void buttonSelectionChanged(DialogButton button, bool isSelected)
        {
            if (!isSelected)
                InternalButtons.Deselect();
            else
                InternalButtons.Select(button);
        }

        private void updateRetryCount()
        {
            // "You've retried 1,065 times in this session"
            // "You've retried 1 time in this session"

            retryCounterContainer.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "You've retried ",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                    Font = OsuFont.GetFont(size: 18),
                },
                new OsuSpriteText
                {
                    Text = "time".ToQuantity(retries),
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 18),
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                },
                new OsuSpriteText
                {
                    Text = " in this session",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                    Font = OsuFont.GetFont(size: 18),
                }
            };
        }

        protected class ButtonContainer : FillFlowContainer<DialogButton>
        {
            private int selectedIndex = -1;

            private void setSelected(int value)
            {
                if (selectedIndex == value)
                    return;

                // Deselect the previously-selected button
                if (selectedIndex != -1)
                    this[selectedIndex].Selected.Value = false;

                selectedIndex = value;

                // Select the newly-selected button
                if (selectedIndex != -1)
                    this[selectedIndex].Selected.Value = true;
            }

            public void SelectNext()
            {
                if (selectedIndex == -1 || selectedIndex == Count - 1)
                    setSelected(0);
                else
                    setSelected(selectedIndex + 1);
            }

            public void SelectPrevious()
            {
                if (selectedIndex == -1 || selectedIndex == 0)
                    setSelected(Count - 1);
                else
                    setSelected(selectedIndex - 1);
            }

            public void Deselect() => setSelected(-1);
            public void Select(DialogButton button) => setSelected(IndexOf(button));
        }

        private class Button : DialogButton
        {
            // required to ensure keyboard navigation always starts from an extremity (unless the cursor is moved)
            protected override bool OnHover(HoverEvent e) => true;

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                Selected.Value = true;
                return base.OnMouseMove(e);
            }
        }

        [Resolved]
        private GlobalActionContainer globalAction { get; set; }

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                case ScrollEvent _:
                    if (ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                        return globalAction.TriggerEvent(e);

                    break;
            }

            return base.Handle(e);
        }
    }
}
