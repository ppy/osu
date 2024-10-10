// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModSelectFooterContent : VisibilityContainer
    {
        private readonly ModSelectOverlay overlay;

        private RankingInformationDisplay? rankingInformationDisplay;
        private BeatmapAttributesDisplay? beatmapAttributesDisplay;
        private FillFlowContainer<ShearedButton> buttonFlow = null!;
        private FillFlowContainer contentFlow = null!;

        public DeselectAllModsButton? DeselectAllModsButton { get; set; }

        public readonly IBindable<WorkingBeatmap?> Beatmap = new Bindable<WorkingBeatmap?>();
        public readonly IBindable<IReadOnlyList<Mod>> ActiveMods = new Bindable<IReadOnlyList<Mod>>();

        /// <summary>
        /// Whether the effects (on score multiplier, on or beatmap difficulty) of the current selected set of mods should be shown.
        /// </summary>
        protected virtual bool ShowModEffects => true;

        /// <summary>
        /// Whether the ranking information and beatmap attributes displays are stacked vertically due to small space.
        /// </summary>
        public bool DisplaysStackedVertically { get; private set; }

        public ModSelectFooterContent(ModSelectOverlay overlay)
        {
            this.overlay = overlay;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = buttonFlow = new FillFlowContainer<ShearedButton>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Padding = new MarginPadding { Horizontal = 20 },
                Spacing = new Vector2(10),
                ChildrenEnumerable = CreateButtons(),
            };

            if (ShowModEffects)
            {
                AddInternal(contentFlow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(30, 10),
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Horizontal = 20 },
                    Children = new Drawable[]
                    {
                        rankingInformationDisplay = new RankingInformationDisplay
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight
                        },
                        beatmapAttributesDisplay = new BeatmapAttributesDisplay
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            BeatmapInfo = { Value = Beatmap.Value?.BeatmapInfo },
                        },
                    }
                });
            }
        }

        private ModSettingChangeTracker? modSettingChangeTracker;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.BindValueChanged(b =>
            {
                if (beatmapAttributesDisplay != null)
                    beatmapAttributesDisplay.BeatmapInfo.Value = b.NewValue?.BeatmapInfo;
            }, true);

            ActiveMods.BindValueChanged(m =>
            {
                updateInformation();

                modSettingChangeTracker?.Dispose();

                // Importantly, use ActiveMods.Value here (and not the ValueChanged NewValue) as the latter can
                // potentially be stale, due to complexities in the way change trackers work.
                //
                // See https://github.com/ppy/osu/pull/23284#issuecomment-1529056988
                modSettingChangeTracker = new ModSettingChangeTracker(ActiveMods.Value);
                modSettingChangeTracker.SettingChanged += _ => updateInformation();
            }, true);
        }

        private void updateInformation()
        {
            if (rankingInformationDisplay != null)
            {
                double multiplier = 1.0;

                foreach (var mod in ActiveMods.Value)
                    multiplier *= mod.ScoreMultiplier;

                rankingInformationDisplay.ModMultiplier.Value = multiplier;
                rankingInformationDisplay.Ranked.Value = ActiveMods.Value.All(m => m.Ranked);
            }

            if (beatmapAttributesDisplay != null)
                beatmapAttributesDisplay.Mods.Value = ActiveMods.Value;
        }

        protected override void Update()
        {
            base.Update();

            if (beatmapAttributesDisplay != null)
            {
                float rightEdgeOfLastButton = buttonFlow[^1].ScreenSpaceDrawQuad.TopRight.X;

                // this is cheating a bit; the 640 value is hardcoded based on how wide the expanded panel _generally_ is.
                // due to the transition applied, the raw screenspace quad of the panel cannot be used, as it will trigger an ugly feedback cycle of expanding and collapsing.
                float projectedLeftEdgeOfExpandedBeatmapAttributesDisplay = buttonFlow.ToScreenSpace(buttonFlow.DrawSize - new Vector2(640, 0)).X;

                DisplaysStackedVertically = rightEdgeOfLastButton > projectedLeftEdgeOfExpandedBeatmapAttributesDisplay;

                // only update preview panel's collapsed state after we are fully visible, to ensure all the buttons are where we expect them to be.
                if (Alpha == 1)
                    beatmapAttributesDisplay.Collapsed.Value = DisplaysStackedVertically;

                contentFlow.LayoutDuration = 200;
                contentFlow.LayoutEasing = Easing.OutQuint;
                contentFlow.Direction = DisplaysStackedVertically ? FillDirection.Vertical : FillDirection.Horizontal;
            }
        }

        protected virtual IEnumerable<ShearedButton> CreateButtons() => new[]
        {
            DeselectAllModsButton = new DeselectAllModsButton(overlay)
        };

        protected override void PopIn()
        {
            this.MoveToY(0, 400, Easing.OutQuint)
                .FadeIn(400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.MoveToY(-20f, 200, Easing.OutQuint)
                .FadeOut(200, Easing.OutQuint);
        }
    }
}
