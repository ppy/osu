// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Screens.Play
{
    public abstract partial class GameplayMenuOverlay : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        protected const int TRANSITION_DURATION = 200;

        private const int button_height = 70;
        private const float background_alpha = 0.75f;

        protected override bool BlockNonPositionalInput => true;

        protected override bool BlockScrollInput => false;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        public Action? OnResume;
        public Action? OnRetry;
        public Action? OnQuit;

        /// <summary>
        /// Action that is invoked when <see cref="GlobalAction.Back"/> is triggered.
        /// </summary>
        protected virtual Action BackAction => () =>
        {
            // We prefer triggering the button click as it will animate...
            // but sometimes buttons aren't present (see FailOverlay's constructor as an example).
            if (Buttons.Any())
                Buttons.Last().TriggerClick();
            else
                OnQuit?.Invoke();
        };

        /// <summary>
        /// Action that is invoked when <see cref="GlobalAction.Select"/> is triggered.
        /// </summary>
        protected virtual Action SelectAction => () => InternalButtons.Selected?.TriggerClick();

        public abstract LocalisableString Header { get; }

        protected SelectionCycleFillFlowContainer<DialogButton> InternalButtons = null!;
        public IReadOnlyList<DialogButton> Buttons => InternalButtons;

        private TextFlowContainer playInfoText = null!;

        [Resolved]
        private GlobalActionContainer globalAction { get; set; } = null!;

        protected GameplayMenuOverlay()
        {
            RelativeSizeAxes = Axes.Both;
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
                        new OsuSpriteText
                        {
                            Text = Header,
                            Font = OsuFont.GetFont(size: 48),
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Colour = colours.Yellow,
                        },
                        InternalButtons = new SelectionCycleFillFlowContainer<DialogButton>
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
                        playInfoText = new OsuTextFlowContainer(cp => cp.Font = OsuFont.GetFont(size: 18))
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                },
            };

            if (OnResume != null)
                AddButton(GameplayMenuOverlayStrings.Continue, colours.Green, () => OnResume.Invoke());

            if (OnRetry != null)
                AddButton(GameplayMenuOverlayStrings.Retry, colours.YellowDark, () => OnRetry.Invoke());

            if (OnQuit != null)
                AddButton(GameplayMenuOverlayStrings.Quit, new Color4(170, 27, 39, 255), () => OnQuit.Invoke());

            State.ValueChanged += _ => InternalButtons.Deselect();

            updateInfoText();
        }

        private int retries;

        public int Retries
        {
            set
            {
                if (value == retries)
                    return;

                retries = value;

                if (IsLoaded)
                    updateInfoText();
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(TRANSITION_DURATION, Easing.In);
            updateInfoText();
        }

        protected override void PopOut() => this.FadeOut(TRANSITION_DURATION, Easing.In);

        // Don't let mouse down events through the overlay or people can click circles while paused.
        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnMouseMove(MouseMoveEvent e) => true;

        protected void AddButton(LocalisableString text, Color4 colour, Action? action)
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

            InternalButtons.Add(button);
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
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

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        [Resolved]
        private IGameplayClock? gameplayClock { get; set; }

        [Resolved]
        private GameplayState? gameplayState { get; set; }

        private void updateInfoText()
        {
            playInfoText.Clear();
            playInfoText.AddText(GameplayMenuOverlayStrings.RetryCount);
            playInfoText.AddText(retries.ToString(), cp => cp.Font = cp.Font.With(weight: FontWeight.Bold));

            if (getSongProgress() is int progress)
            {
                playInfoText.NewLine();
                playInfoText.AddText(GameplayMenuOverlayStrings.SongProgress);
                playInfoText.AddText($"{progress}%", cp => cp.Font = cp.Font.With(weight: FontWeight.Bold));
            }
        }

        private int? getSongProgress()
        {
            if (gameplayClock == null || gameplayState == null)
                return null;

            (double firstHitTime, double lastHitTime) = gameplayState.Beatmap.CalculatePlayableBounds();

            double playableLength = (lastHitTime - firstHitTime);

            if (playableLength == 0)
                return 0;

            return (int)Math.Clamp(((gameplayClock.CurrentTime - firstHitTime) / playableLength) * 100, 0, 100);
        }

        private partial class Button : DialogButton
        {
            // required to ensure keyboard navigation always starts from an extremity (unless the cursor is moved)
            protected override bool OnHover(HoverEvent e) => true;

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                State = SelectionState.Selected;
                return base.OnMouseMove(e);
            }
        }

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                case ScrollEvent:
                    if (ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                        return globalAction.TriggerEvent(e);

                    break;
            }

            return base.Handle(e);
        }
    }
}
