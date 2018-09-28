// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;
using OpenTK.Input;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Play
{
    public abstract class GameplayMenuOverlay : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        private const int transition_duration = 200;
        private const int button_height = 70;
        private const float background_alpha = 0.75f;

        protected override bool BlockNonPositionalInput => true;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public Action OnRetry;
        public Action OnQuit;

        /// <summary>
        /// Action that is invoked when <see cref="GlobalAction.Back"/> is triggered.
        /// </summary>
        protected virtual Action BackAction => () => InternalButtons.Children.Last().Click();

        public abstract string Header { get; }
        public abstract string Description { get; }

        protected internal FillFlowContainer<DialogButton> InternalButtons;
        public IReadOnlyList<DialogButton> Buttons => InternalButtons;

        private FillFlowContainer retryCounterContainer;

        protected GameplayMenuOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            StateChanged += s => selectionIndex = -1;
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
                                    Font = @"Exo2.0-Medium",
                                    Spacing = new Vector2(5, 0),
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    TextSize = 30,
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
                        InternalButtons = new FillFlowContainer<DialogButton>
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

        protected override void PopIn() => this.FadeIn(transition_duration, Easing.In);
        protected override void PopOut() => this.FadeOut(transition_duration, Easing.In);

        // Don't let mouse down events through the overlay or people can click circles while paused.
        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => true;

        protected override bool OnMouseMove(InputState state) => true;

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

            button.Selected.ValueChanged += s => buttonSelectionChanged(button, s);

            InternalButtons.Add(button);
        }

        private int _selectionIndex = -1;

        private int selectionIndex
        {
            get { return _selectionIndex; }
            set
            {
                if (_selectionIndex == value)
                    return;

                // Deselect the previously-selected button
                if (_selectionIndex != -1)
                    InternalButtons[_selectionIndex].Selected.Value = false;

                _selectionIndex = value;

                // Select the newly-selected button
                if (_selectionIndex != -1)
                    InternalButtons[_selectionIndex].Selected.Value = true;
            }
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat)
            {
                switch (args.Key)
                {
                    case Key.Up:
                        if (selectionIndex == -1 || selectionIndex == 0)
                            selectionIndex = InternalButtons.Count - 1;
                        else
                            selectionIndex--;
                        return true;
                    case Key.Down:
                        if (selectionIndex == -1 || selectionIndex == InternalButtons.Count - 1)
                            selectionIndex = 0;
                        else
                            selectionIndex++;
                        return true;
                }
            }

            return base.OnKeyDown(state, args);
        }

        public bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                BackAction.Invoke();
                return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => action == GlobalAction.Back;

        private void buttonSelectionChanged(DialogButton button, bool isSelected)
        {
            if (!isSelected)
                selectionIndex = -1;
            else
                selectionIndex = InternalButtons.IndexOf(button);
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
                    TextSize = 18
                },
                new OsuSpriteText
                {
                    Text = $"{retries:n0}",
                    Font = @"Exo2.0-Bold",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                    TextSize = 18
                },
                new OsuSpriteText
                {
                    Text = $" time{(retries == 1 ? "" : "s")} in this session",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f),
                    TextSize = 18
                }
            };
        }

        private class Button : DialogButton
        {
            protected override bool OnHover(InputState state) => true;

            protected override bool OnMouseMove(InputState state)
            {
                Selected.Value = true;
                return base.OnMouseMove(state);
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (args.Repeat || args.Key != Key.Enter || !Selected)
                    return false;

                OnClick(state);
                return true;
            }
        }
    }
}
