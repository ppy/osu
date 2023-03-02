﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.ClicksPerSecond;
using osu.Game.Screens.Play.HUD.JudgementCounter;
using osu.Game.Skinning;
using osuTK;
using osu.Game.Localisation;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Play
{
    [Cached]
    public partial class HUDOverlay : Container, IKeyBindingHandler<GlobalAction>
    {
        public const float FADE_DURATION = 300;

        public const Easing FADE_EASING = Easing.OutQuint;

        /// <summary>
        /// The total height of all the bottom of screen scoring elements.
        /// </summary>
        public float BottomScoringElementsHeight { get; private set; }

        protected override bool ShouldBeConsideredForInput(Drawable child)
        {
            // HUD uses AlwaysVisible on child components so they can be in an updated state for next display.
            // Without blocking input, this would also allow them to be interacted with in such a state.
            if (ShowHud.Value)
                return base.ShouldBeConsideredForInput(child);

            // hold to quit button should always be interactive.
            return child == bottomRightElements;
        }

        public readonly KeyCounterDisplay KeyCounter;
        public readonly ModDisplay ModDisplay;
        public readonly HoldForMenuButton HoldToQuit;
        public readonly PlayerSettingsOverlay PlayerSettingsOverlay;

        [Cached]
        private readonly ClicksPerSecondCalculator clicksPerSecondCalculator;

        [Cached]
        private readonly JudgementTally tally;

        public Bindable<bool> ShowHealthBar = new Bindable<bool>(true);

        private readonly DrawableRuleset drawableRuleset;
        private readonly IReadOnlyList<Mod> mods;

        /// <summary>
        /// Whether the elements that can optionally be hidden should be visible.
        /// </summary>
        public Bindable<bool> ShowHud { get; } = new BindableBool();

        private Bindable<HUDVisibilityMode> configVisibilityMode;

        private readonly BindableBool replayLoaded = new BindableBool();

        private static bool hasShownNotificationOnce;

        private readonly FillFlowContainer bottomRightElements;
        private readonly FillFlowContainer topRightElements;

        internal readonly IBindable<bool> IsPlaying = new Bindable<bool>();

        public IBindable<bool> HoldingForHUD => holdingForHUD;

        private readonly BindableBool holdingForHUD = new BindableBool();

        private readonly SkinComponentsContainer mainComponents;

        /// <summary>
        /// A flow which sits at the left side of the screen to house leaderboard (and related) components.
        /// Will automatically be positioned to avoid colliding with top scoring elements.
        /// </summary>
        public readonly FillFlowContainer LeaderboardFlow;

        private readonly List<Drawable> hideTargets;

        public HUDOverlay(DrawableRuleset drawableRuleset, IReadOnlyList<Mod> mods, bool alwaysShowLeaderboard = true)
        {
            Drawable rulesetComponents;

            this.drawableRuleset = drawableRuleset;
            this.mods = mods;

            RelativeSizeAxes = Axes.Both;

            Children = new[]
            {
                CreateFailingLayer(),
                //Needs to be initialized before skinnable drawables.
                tally = new JudgementTally(),
                mainComponents = new HUDComponentsContainer { AlwaysPresent = true, },
                rulesetComponents = drawableRuleset != null
                    ? new HUDComponentsContainer(drawableRuleset.Ruleset.RulesetInfo) { AlwaysPresent = true, }
                    : Empty(),
                topRightElements = new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AlwaysPresent = true,
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(10),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        ModDisplay = CreateModsContainer(),
                        PlayerSettingsOverlay = CreatePlayerSettingsOverlay(),
                    }
                },
                bottomRightElements = new FillFlowContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(10),
                    AutoSizeAxes = Axes.Both,
                    LayoutDuration = FADE_DURATION / 2,
                    LayoutEasing = FADE_EASING,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        KeyCounter = CreateKeyCounter(),
                        HoldToQuit = CreateHoldForMenuButton(),
                    }
                },
                LeaderboardFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(44), // enough margin to avoid the hit error display
                    Spacing = new Vector2(5)
                },
                clicksPerSecondCalculator = new ClicksPerSecondCalculator(),
            };

            hideTargets = new List<Drawable> { mainComponents, rulesetComponents, KeyCounter, topRightElements };

            if (!alwaysShowLeaderboard)
                hideTargets.Add(LeaderboardFlow);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, INotificationOverlay notificationOverlay)
        {
            if (drawableRuleset != null)
            {
                BindDrawableRuleset(drawableRuleset);
            }

            ModDisplay.Current.Value = mods;

            configVisibilityMode = config.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode);

            if (configVisibilityMode.Value == HUDVisibilityMode.Never && !hasShownNotificationOnce)
            {
                hasShownNotificationOnce = true;

                notificationOverlay?.Post(new SimpleNotification
                {
                    Text = NotificationsStrings.ScoreOverlayDisabled(config.LookupKeyBindings(GlobalAction.ToggleInGameInterface))
                });
            }

            // start all elements hidden
            hideTargets.ForEach(d => d.Hide());
        }

        public override void Hide() =>
            throw new InvalidOperationException($"{nameof(HUDOverlay)} should not be hidden as it will remove the ability of a user to quit. Use {nameof(ShowHud)} instead.");

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowHud.BindValueChanged(visible => hideTargets.ForEach(d => d.FadeTo(visible.NewValue ? 1 : 0, FADE_DURATION, FADE_EASING)));

            holdingForHUD.BindValueChanged(_ => updateVisibility());
            IsPlaying.BindValueChanged(_ => updateVisibility());
            configVisibilityMode.BindValueChanged(_ => updateVisibility(), true);

            replayLoaded.BindValueChanged(replayLoadedValueChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            float? lowestTopScreenSpaceLeft = null;
            float? lowestTopScreenSpaceRight = null;

            Vector2? highestBottomScreenSpace = null;

            // LINQ cast can be removed when IDrawable interface includes Anchor / RelativeSizeAxes.
            foreach (var element in mainComponents.Components.Cast<Drawable>())
            {
                // for now align some top components with the bottom-edge of the lowest top-anchored hud element.
                if (element.Anchor.HasFlagFast(Anchor.y0))
                {
                    // health bars are excluded for the sake of hacky legacy skins which extend the health bar to take up the full screen area.
                    if (element is LegacyHealthDisplay)
                        continue;

                    float bottom = element.ScreenSpaceDrawQuad.BottomRight.Y;

                    bool isRelativeX = element.RelativeSizeAxes == Axes.X;

                    if (element.Anchor.HasFlagFast(Anchor.TopRight) || isRelativeX)
                    {
                        if (lowestTopScreenSpaceRight == null || bottom > lowestTopScreenSpaceRight.Value)
                            lowestTopScreenSpaceRight = bottom;
                    }

                    if (element.Anchor.HasFlagFast(Anchor.TopLeft) || isRelativeX)
                    {
                        if (lowestTopScreenSpaceLeft == null || bottom > lowestTopScreenSpaceLeft.Value)
                            lowestTopScreenSpaceLeft = bottom;
                    }
                }
                // and align bottom-right components with the top-edge of the highest bottom-anchored hud element.
                else if (element.Anchor.HasFlagFast(Anchor.BottomRight) || (element.Anchor.HasFlagFast(Anchor.y2) && element.RelativeSizeAxes == Axes.X))
                {
                    var topLeft = element.ScreenSpaceDrawQuad.TopLeft;
                    if (highestBottomScreenSpace == null || topLeft.Y < highestBottomScreenSpace.Value.Y)
                        highestBottomScreenSpace = topLeft;
                }
            }

            if (lowestTopScreenSpaceRight.HasValue)
                topRightElements.Y = MathHelper.Clamp(ToLocalSpace(new Vector2(0, lowestTopScreenSpaceRight.Value)).Y, 0, DrawHeight - topRightElements.DrawHeight);
            else
                topRightElements.Y = 0;

            if (lowestTopScreenSpaceLeft.HasValue)
                LeaderboardFlow.Y = MathHelper.Clamp(ToLocalSpace(new Vector2(0, lowestTopScreenSpaceLeft.Value)).Y, 0, DrawHeight - LeaderboardFlow.DrawHeight);
            else
                LeaderboardFlow.Y = 0;

            if (highestBottomScreenSpace.HasValue)
                bottomRightElements.Y = BottomScoringElementsHeight = -MathHelper.Clamp(DrawHeight - ToLocalSpace(highestBottomScreenSpace.Value).Y, 0, DrawHeight - bottomRightElements.DrawHeight);
            else
                bottomRightElements.Y = 0;
        }

        private void updateVisibility()
        {
            if (ShowHud.Disabled)
                return;

            if (holdingForHUD.Value)
            {
                ShowHud.Value = true;
                return;
            }

            switch (configVisibilityMode.Value)
            {
                case HUDVisibilityMode.Never:
                    ShowHud.Value = false;
                    break;

                case HUDVisibilityMode.HideDuringGameplay:
                    // always show during replay as we want the seek bar to be visible.
                    ShowHud.Value = replayLoaded.Value || !IsPlaying.Value;
                    break;

                case HUDVisibilityMode.Always:
                    ShowHud.Value = true;
                    break;
            }
        }

        private void replayLoadedValueChanged(ValueChangedEvent<bool> e)
        {
            PlayerSettingsOverlay.ReplayLoaded = e.NewValue;

            if (e.NewValue)
            {
                PlayerSettingsOverlay.Show();
                ModDisplay.FadeIn(200);
                KeyCounter.Margin = new MarginPadding(10) { Bottom = 30 };
            }
            else
            {
                PlayerSettingsOverlay.Hide();
                ModDisplay.Delay(2000).FadeOut(200);
                KeyCounter.Margin = new MarginPadding(10);
            }

            updateVisibility();
        }

        protected virtual void BindDrawableRuleset(DrawableRuleset drawableRuleset)
        {
            if (drawableRuleset is ICanAttachHUDPieces attachTarget)
            {
                attachTarget.Attach(KeyCounter);
                attachTarget.Attach(clicksPerSecondCalculator);
            }

            replayLoaded.BindTo(drawableRuleset.HasReplayLoaded);
        }

        protected FailingLayer CreateFailingLayer() => new FailingLayer
        {
            ShowHealth = { BindTarget = ShowHealthBar }
        };

        protected KeyCounterDisplay CreateKeyCounter() => new KeyCounterDisplay
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
        };

        protected HoldForMenuButton CreateHoldForMenuButton() => new HoldForMenuButton
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
        };

        protected ModDisplay CreateModsContainer() => new ModDisplay
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
        };

        protected PlayerSettingsOverlay CreatePlayerSettingsOverlay() => new PlayerSettingsOverlay();

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.HoldForHUD:
                    holdingForHUD.Value = true;
                    return true;

                case GlobalAction.ToggleInGameInterface:
                    switch (configVisibilityMode.Value)
                    {
                        case HUDVisibilityMode.Never:
                            configVisibilityMode.Value = HUDVisibilityMode.HideDuringGameplay;
                            break;

                        case HUDVisibilityMode.HideDuringGameplay:
                            configVisibilityMode.Value = HUDVisibilityMode.Always;
                            break;

                        case HUDVisibilityMode.Always:
                            configVisibilityMode.Value = HUDVisibilityMode.Never;
                            break;
                    }

                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.HoldForHUD:
                    holdingForHUD.Value = false;
                    break;
            }
        }

        private partial class HUDComponentsContainer : SkinComponentsContainer
        {
            private Bindable<ScoringMode> scoringMode;

            [Resolved]
            private OsuConfigManager config { get; set; }

            public HUDComponentsContainer([CanBeNull] RulesetInfo ruleset = null)
                : base(new SkinComponentsContainerLookup(SkinComponentsContainerLookup.TargetArea.MainHUDComponents, ruleset))
            {
                RelativeSizeAxes = Axes.Both;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                // When the scoring mode changes, relative positions of elements may change (see DefaultSkin.GetDrawableComponent).
                // This is a best effort implementation for cases where users haven't customised layouts.
                scoringMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
                scoringMode.BindValueChanged(_ => Reload());
            }
        }
    }
}
