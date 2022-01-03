// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play
{
    [Cached]
    public class HUDOverlay : Container, IKeyBindingHandler<GlobalAction>
    {
        public const float FADE_DURATION = 300;

        public const Easing FADE_EASING = Easing.OutQuint;

        /// <summary>
        /// The total height of all the top of screen scoring elements.
        /// </summary>
        public float TopScoringElementsHeight { get; private set; }

        /// <summary>
        /// The total height of all the bottom of screen scoring elements.
        /// </summary>
        public float BottomScoringElementsHeight { get; private set; }

        public readonly KeyCounterDisplay KeyCounter;
        public readonly ModDisplay ModDisplay;
        public readonly HoldForMenuButton HoldToQuit;
        public readonly PlayerSettingsOverlay PlayerSettingsOverlay;

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

        internal readonly IBindable<bool> IsBreakTime = new Bindable<bool>();

        private bool holdingForHUD;

        private readonly SkinnableTargetContainer mainComponents;

        private IEnumerable<Drawable> hideTargets => new Drawable[] { mainComponents, KeyCounter, topRightElements };

        public HUDOverlay(DrawableRuleset drawableRuleset, IReadOnlyList<Mod> mods)
        {
            this.drawableRuleset = drawableRuleset;
            this.mods = mods;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                CreateFailingLayer(),
                mainComponents = new SkinnableTargetContainer(SkinnableTarget.MainHUDComponents)
                {
                    RelativeSizeAxes = Axes.Both,
                },
                topRightElements = new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
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
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuConfigManager config, NotificationOverlay notificationOverlay)
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
                    Text = $"The score overlay is currently disabled. You can toggle this by pressing {config.LookupKeyBindings(GlobalAction.ToggleInGameInterface)}."
                });
            }

            // start all elements hidden
            hideTargets.ForEach(d => d.Hide());
        }

        public override void Hide() => throw new InvalidOperationException($"{nameof(HUDOverlay)} should not be hidden as it will remove the ability of a user to quit. Use {nameof(ShowHud)} instead.");

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowHud.BindValueChanged(visible => hideTargets.ForEach(d => d.FadeTo(visible.NewValue ? 1 : 0, FADE_DURATION, FADE_EASING)));

            IsBreakTime.BindValueChanged(_ => updateVisibility());
            configVisibilityMode.BindValueChanged(_ => updateVisibility(), true);

            replayLoaded.BindValueChanged(replayLoadedValueChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            Vector2? lowestTopScreenSpace = null;
            Vector2? highestBottomScreenSpace = null;

            // LINQ cast can be removed when IDrawable interface includes Anchor / RelativeSizeAxes.
            foreach (var element in mainComponents.Components.Cast<Drawable>())
            {
                // for now align top-right components with the bottom-edge of the lowest top-anchored hud element.
                if (element.Anchor.HasFlagFast(Anchor.TopRight) || (element.Anchor.HasFlagFast(Anchor.y0) && element.RelativeSizeAxes == Axes.X))
                {
                    // health bars are excluded for the sake of hacky legacy skins which extend the health bar to take up the full screen area.
                    if (element is LegacyHealthDisplay)
                        continue;

                    var bottomRight = element.ScreenSpaceDrawQuad.BottomRight;
                    if (lowestTopScreenSpace == null || bottomRight.Y > lowestTopScreenSpace.Value.Y)
                        lowestTopScreenSpace = bottomRight;
                }
                // and align bottom-right components with the top-edge of the highest bottom-anchored hud element.
                else if (element.Anchor.HasFlagFast(Anchor.BottomRight) || (element.Anchor.HasFlagFast(Anchor.y2) && element.RelativeSizeAxes == Axes.X))
                {
                    var topLeft = element.ScreenSpaceDrawQuad.TopLeft;
                    if (highestBottomScreenSpace == null || topLeft.Y < highestBottomScreenSpace.Value.Y)
                        highestBottomScreenSpace = topLeft;
                }
            }

            if (lowestTopScreenSpace.HasValue)
                topRightElements.Y = TopScoringElementsHeight = MathHelper.Clamp(ToLocalSpace(lowestTopScreenSpace.Value).Y, 0, DrawHeight - topRightElements.DrawHeight);
            else
                topRightElements.Y = 0;

            if (highestBottomScreenSpace.HasValue)
                bottomRightElements.Y = BottomScoringElementsHeight = -MathHelper.Clamp(DrawHeight - ToLocalSpace(highestBottomScreenSpace.Value).Y, 0, DrawHeight - bottomRightElements.DrawHeight);
            else
                bottomRightElements.Y = 0;
        }

        private void updateVisibility()
        {
            if (ShowHud.Disabled)
                return;

            if (holdingForHUD)
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
                    ShowHud.Value = replayLoaded.Value || IsBreakTime.Value;
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
            (drawableRuleset as ICanAttachKeyCounter)?.Attach(KeyCounter);

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
                    holdingForHUD = true;
                    updateVisibility();
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
                    holdingForHUD = false;
                    updateVisibility();
                    break;
            }
        }
    }
}
