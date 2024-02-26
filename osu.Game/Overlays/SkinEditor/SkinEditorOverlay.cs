// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    /// <summary>
    /// A container which handles loading a skin editor on user request for a specified target.
    /// This also handles the scaling / positioning adjustment of the target.
    /// </summary>
    public partial class SkinEditorOverlay : OverlayContainer, IKeyBindingHandler<GlobalAction>
    {
        private readonly ScalingContainer scalingContainer;

        protected override bool BlockNonPositionalInput => true;

        private SkinEditor? skinEditor;

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Cached]
        public readonly EditorClipboard Clipboard = new EditorClipboard();

        [Resolved]
        private OsuGame game { get; set; } = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        private OsuScreen? lastTargetScreen;
        private InvokeOnDisposal? nestedInputManagerDisable;

        private Vector2 lastDrawSize;

        public SkinEditorOverlay(ScalingContainer scalingContainer)
        {
            this.scalingContainer = scalingContainer;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.BeatmapSkins, beatmapSkins);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (skinEditor?.State.Value != Visibility.Visible)
                        break;

                    Hide();
                    return true;
            }

            return false;
        }

        protected override void PopIn()
        {
            globallyDisableBeatmapSkinSetting();

            if (skinEditor != null)
            {
                disableNestedInputManagers();
                skinEditor.Show();

                if (lastTargetScreen is MainMenu)
                    PresentGameplay();

                return;
            }

            var editor = new SkinEditor();

            editor.State.BindValueChanged(_ => updateComponentVisibility());

            skinEditor = editor;

            LoadComponentAsync(editor, _ =>
            {
                if (editor != skinEditor)
                    return;

                AddInternal(editor);

                if (lastTargetScreen is MainMenu)
                    PresentGameplay();

                Debug.Assert(lastTargetScreen != null);

                SetTarget(lastTargetScreen);
            });
        }

        protected override void PopOut()
        {
            skinEditor?.Save(false);
            skinEditor?.Hide();
            nestedInputManagerDisable?.Dispose();
            nestedInputManagerDisable = null;

            globallyReenableBeatmapSkinSetting();
        }

        public void PresentGameplay() => presentGameplay(false);

        private void presentGameplay(bool attemptedBeatmapSwitch)
        {
            performer?.PerformFromScreen(screen =>
            {
                if (State.Value != Visibility.Visible)
                    return;

                if (beatmap.Value is DummyWorkingBeatmap)
                {
                    // presume we don't have anything good to play and just bail.
                    return;
                }

                // If we're playing the intro, switch away to another beatmap.
                if (beatmap.Value.BeatmapSetInfo.Protected)
                {
                    if (!attemptedBeatmapSwitch)
                    {
                        music.NextTrack();
                        Schedule(() => presentGameplay(true));
                    }

                    return;
                }

                if (screen is Player)
                    return;

                // the validity of the current game-wide beatmap + ruleset combination is enforced by song select.
                // if we're anywhere else, the state is unknown and may not make sense, so forcibly set something that does.
                if (screen is not PlaySongSelect)
                    ruleset.Value = beatmap.Value.BeatmapInfo.Ruleset;
                var replayGeneratingMod = ruleset.Value.CreateInstance().GetAutoplayMod();

                IReadOnlyList<Mod> usableMods = mods.Value;

                if (replayGeneratingMod != null)
                    usableMods = usableMods.Append(replayGeneratingMod).ToArray();

                if (!ModUtils.CheckCompatibleSet(usableMods, out var invalid))
                    mods.Value = mods.Value.Except(invalid).ToArray();

                if (replayGeneratingMod != null)
                    screen.Push(new EndlessPlayer((beatmap, mods) => replayGeneratingMod.CreateScoreFromReplayData(beatmap, mods)));
            }, new[] { typeof(Player), typeof(PlaySongSelect) });
        }

        protected override void Update()
        {
            base.Update();

            if (game.DrawSize != lastDrawSize)
            {
                lastDrawSize = game.DrawSize;
                updateScreenSizing();
            }
        }

        private void updateScreenSizing()
        {
            if (skinEditor?.State.Value != Visibility.Visible) return;

            const float padding = 10;

            float relativeSidebarWidth = (EditorSidebar.WIDTH + padding) / DrawWidth;
            float relativeToolbarHeight = (SkinEditorSceneLibrary.HEIGHT + SkinEditor.MENU_HEIGHT + padding) / DrawHeight;

            var rect = new RectangleF(
                relativeSidebarWidth,
                relativeToolbarHeight,
                1 - relativeSidebarWidth * 2,
                1f - relativeToolbarHeight - padding / DrawHeight);

            scalingContainer.SetCustomRect(rect, true);
        }

        private void updateComponentVisibility()
        {
            Debug.Assert(skinEditor != null);

            if (skinEditor.State.Value == Visibility.Visible)
            {
                Scheduler.AddOnce(updateScreenSizing);

                game.Toolbar.Hide();
                game.CloseAllOverlays();
            }
            else
            {
                scalingContainer.SetCustomRect(null);

                if (lastTargetScreen?.HideOverlaysOnEnter != true)
                    game.Toolbar.Show();
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        /// <summary>
        /// Set a new target screen which will be used to find skinnable components.
        /// </summary>
        public void SetTarget(OsuScreen screen)
        {
            nestedInputManagerDisable?.Dispose();
            nestedInputManagerDisable = null;

            lastTargetScreen = screen;

            if (skinEditor == null) return;

            // ensure the toolbar is re-hidden even if a new screen decides to try and show it.
            updateComponentVisibility();

            // AddOnce with parameter will ensure the newest target is loaded if there is any overlap.
            Scheduler.AddOnce(setTarget, screen);
        }

        private void setTarget(OsuScreen? target)
        {
            if (target == null)
                return;

            Debug.Assert(skinEditor != null);

            if (!target.IsLoaded || !skinEditor.IsLoaded)
            {
                Scheduler.AddOnce(setTarget, target);
                return;
            }

            if (skinEditor.State.Value == Visibility.Visible)
            {
                skinEditor.Save(false);
                skinEditor.UpdateTargetScreen(target);
                disableNestedInputManagers();
            }
            else
            {
                skinEditor.Hide();
                skinEditor.Expire();
                skinEditor = null;
            }
        }

        private void disableNestedInputManagers()
        {
            if (lastTargetScreen == null)
                return;

            var nestedInputManagers = lastTargetScreen.ChildrenOfType<PassThroughInputManager>().Where(manager => manager.UseParentInput).ToArray();
            foreach (var inputManager in nestedInputManagers)
                inputManager.UseParentInput = false;
            nestedInputManagerDisable = new InvokeOnDisposal(() =>
            {
                foreach (var inputManager in nestedInputManagers)
                    inputManager.UseParentInput = true;
            });
        }

        private readonly Bindable<bool> beatmapSkins = new Bindable<bool>();
        private LeasedBindable<bool>? leasedBeatmapSkins;

        private void globallyDisableBeatmapSkinSetting()
        {
            if (beatmapSkins.Disabled)
                return;

            // The skin editor doesn't work well if beatmap skins are being applied to the player screen.
            // To keep things simple, disable the setting game-wide while using the skin editor.
            //
            // This causes a full reload of the skin, which is pretty ugly.
            // TODO: Investigate if we can avoid this when a beatmap skin is not being applied by the current beatmap.
            leasedBeatmapSkins = beatmapSkins.BeginLease(true);
            leasedBeatmapSkins.Value = false;
        }

        private void globallyReenableBeatmapSkinSetting()
        {
            leasedBeatmapSkins?.Return();
            leasedBeatmapSkins = null;
        }

        private partial class EndlessPlayer : ReplayPlayer
        {
            protected override UserActivity? InitialActivity => null;

            public override bool DisallowExternalBeatmapRulesetChanges => true;

            public override bool? AllowGlobalTrackControl => false;

            public EndlessPlayer(Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore)
                : base(createScore, new PlayerConfiguration
                {
                    ShowResults = false,
                    AutomaticallySkipIntro = true,
                })
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (!LoadedBeatmapSuccessfully)
                    Scheduler.AddDelayed(this.Exit, 1000);
            }

            protected override void Update()
            {
                base.Update();

                if (!LoadedBeatmapSuccessfully)
                    return;

                if (GameplayState.HasPassed)
                    GameplayClockContainer.Seek(0);
            }
        }
    }
}
