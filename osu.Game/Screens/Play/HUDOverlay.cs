// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.ClicksPerSecond;
using osu.Game.Screens.Play.HUD.JudgementCounter;
using osu.Game.Skinning;
using osuTK;

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

        public readonly ModDisplay ModDisplay;
        public readonly HoldForMenuButton HoldToQuit;
        public readonly PlayerSettingsOverlay PlayerSettingsOverlay;

        [Cached]
        private readonly ClicksPerSecondController clicksPerSecondController;

        [Cached]
        public readonly InputCountController InputCountController;

        [Cached]
        private readonly JudgementCountController judgementCountController;

        public Bindable<bool> ShowHealthBar = new Bindable<bool>(true);

        [CanBeNull]
        private readonly DrawableRuleset drawableRuleset;

        private readonly IReadOnlyList<Mod> mods;

        /// <summary>
        /// Whether the elements that can optionally be hidden should be visible.
        /// </summary>
        public Bindable<bool> ShowHud { get; } = new BindableBool();

        private Bindable<HUDVisibilityMode> configVisibilityMode;
        private Bindable<bool> configLeaderboardVisibility;
        private Bindable<bool> configSettingsOverlay;

        private readonly BindableBool replayLoaded = new BindableBool();

        private static bool hasShownNotificationOnce;

        private readonly FillFlowContainer bottomRightElements;
        private readonly FillFlowContainer topRightElements;

        internal readonly IBindable<bool> IsPlaying = new Bindable<bool>();

        public IBindable<bool> HoldingForHUD => holdingForHUD;

        private readonly BindableBool holdingForHUD = new BindableBool();

        private readonly SkinComponentsContainer mainComponents;

        [CanBeNull]
        private readonly SkinComponentsContainer rulesetComponents;

        /// <summary>
        /// A flow which sits at the left side of the screen to house leaderboard (and related) components.
        /// Will automatically be positioned to avoid colliding with top scoring elements.
        /// </summary>
        public readonly FillFlowContainer LeaderboardFlow;

        private readonly List<Drawable> hideTargets;

        private readonly Drawable playfieldComponents;

        public HUDOverlay([CanBeNull] DrawableRuleset drawableRuleset, IReadOnlyList<Mod> mods, bool alwaysShowLeaderboard = true)
        {
            this.drawableRuleset = drawableRuleset;
            this.mods = mods;

            RelativeSizeAxes = Axes.Both;

            Children = new[]
            {
                CreateFailingLayer(),
                //Needs to be initialized before skinnable drawables.
                judgementCountController = new JudgementCountController(),
                clicksPerSecondController = new ClicksPerSecondController(),
                InputCountController = new InputCountController(),
                mainComponents = new HUDComponentsContainer { AlwaysPresent = true, },
                drawableRuleset != null
                    ? (rulesetComponents = new HUDComponentsContainer(drawableRuleset.Ruleset.RulesetInfo) { AlwaysPresent = true, })
                    : Empty(),
                playfieldComponents = drawableRuleset != null
                    ? new SkinComponentsContainer(new SkinComponentsContainerLookup(SkinComponentsContainerLookup.TargetArea.Playfield, drawableRuleset.Ruleset.RulesetInfo)) { AlwaysPresent = true, }
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
            };

            hideTargets = new List<Drawable> { mainComponents, topRightElements };

            if (rulesetComponents != null)
                hideTargets.Add(rulesetComponents);

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
            configLeaderboardVisibility = config.GetBindable<bool>(OsuSetting.GameplayLeaderboard);
            configSettingsOverlay = config.GetBindable<bool>(OsuSetting.ReplaySettingsOverlay);

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
            configVisibilityMode.BindValueChanged(_ => updateVisibility());
            configSettingsOverlay.BindValueChanged(_ => updateVisibility());

            replayLoaded.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    ModDisplay.FadeIn(200);
                    InputCountController.Margin = new MarginPadding(10) { Bottom = 30 };
                }
                else
                {
                    ModDisplay.Delay(2000).FadeOut(200);
                    InputCountController.Margin = new MarginPadding(10);
                }

                updateVisibility();
            }, true);
        }

        protected override void Update()
        {
            base.Update();

            if (drawableRuleset != null)
            {
                Quad playfieldScreenSpaceDrawQuad = drawableRuleset.Playfield.SkinnableComponentScreenSpaceDrawQuad;

                playfieldComponents.Position = ToLocalSpace(playfieldScreenSpaceDrawQuad.TopLeft);
                playfieldComponents.Width = (ToLocalSpace(playfieldScreenSpaceDrawQuad.TopRight) - ToLocalSpace(playfieldScreenSpaceDrawQuad.TopLeft)).Length;
                playfieldComponents.Height = (ToLocalSpace(playfieldScreenSpaceDrawQuad.BottomLeft) - ToLocalSpace(playfieldScreenSpaceDrawQuad.TopLeft)).Length;
                playfieldComponents.Rotation = drawableRuleset.Playfield.Rotation;
            }

            float? lowestTopScreenSpaceLeft = null;
            float? lowestTopScreenSpaceRight = null;

            Vector2? highestBottomScreenSpace = null;

            processDrawables(mainComponents);

            if (rulesetComponents != null)
                processDrawables(rulesetComponents);

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

            void processDrawables(SkinComponentsContainer components)
            {
                // Avoid using foreach due to missing GetEnumerator implementation.
                // See https://github.com/ppy/osu-framework/blob/e10051e6643731e393b09de40a3a3d209a545031/osu.Framework/Bindables/IBindableList.cs#L41-L44.
                for (int i = 0; i < components.Components.Count; i++)
                    processDrawable(components.Components[i]);
            }

            void processDrawable(ISerialisableDrawable element)
            {
                // Cast can be removed when IDrawable interface includes Anchor / RelativeSizeAxes.
                Drawable drawable = (Drawable)element;

                // for now align some top components with the bottom-edge of the lowest top-anchored hud element.
                if (drawable.Anchor.HasFlagFast(Anchor.y0))
                {
                    // health bars are excluded for the sake of hacky legacy skins which extend the health bar to take up the full screen area.
                    if (element is LegacyHealthDisplay)
                        return;

                    float bottom = drawable.ScreenSpaceDrawQuad.BottomRight.Y;

                    bool isRelativeX = drawable.RelativeSizeAxes == Axes.X;

                    if (drawable.Anchor.HasFlagFast(Anchor.TopRight) || isRelativeX)
                    {
                        if (lowestTopScreenSpaceRight == null || bottom > lowestTopScreenSpaceRight.Value)
                            lowestTopScreenSpaceRight = bottom;
                    }

                    if (drawable.Anchor.HasFlagFast(Anchor.TopLeft) || isRelativeX)
                    {
                        if (lowestTopScreenSpaceLeft == null || bottom > lowestTopScreenSpaceLeft.Value)
                            lowestTopScreenSpaceLeft = bottom;
                    }
                }
                // and align bottom-right components with the top-edge of the highest bottom-anchored hud element.
                else if (drawable.Anchor.HasFlagFast(Anchor.BottomRight) || (drawable.Anchor.HasFlagFast(Anchor.y2) && drawable.RelativeSizeAxes == Axes.X))
                {
                    var topLeft = element.ScreenSpaceDrawQuad.TopLeft;
                    if (highestBottomScreenSpace == null || topLeft.Y < highestBottomScreenSpace.Value.Y)
                        highestBottomScreenSpace = topLeft;
                }
            }
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

            if (configSettingsOverlay.Value && replayLoaded.Value)
                PlayerSettingsOverlay.Show();
            else
                PlayerSettingsOverlay.Hide();

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

        protected virtual void BindDrawableRuleset(DrawableRuleset drawableRuleset)
        {
            if (drawableRuleset is ICanAttachHUDPieces attachTarget)
            {
                attachTarget.Attach(InputCountController);
                attachTarget.Attach(clicksPerSecondController);
            }

            replayLoaded.BindTo(drawableRuleset.HasReplayLoaded);
        }

        protected FailingLayer CreateFailingLayer() => new FailingLayer
        {
            ShowHealth = { BindTarget = ShowHealthBar }
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
                case GlobalAction.ToggleReplaySettings:
                    configSettingsOverlay.Value = !configSettingsOverlay.Value;
                    return true;

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

                case GlobalAction.ToggleInGameLeaderboard:
                    configLeaderboardVisibility.Value = !configLeaderboardVisibility.Value;
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
