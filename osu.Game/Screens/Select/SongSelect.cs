// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Options;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Select
{
    public abstract class SongSelect : OsuScreen
    {
        private static readonly Vector2 wedged_container_size = new Vector2(0.5f, 245);
        protected const float BACKGROUND_BLUR = 20;
        private const float left_area_padding = 20;

        public readonly FilterControl FilterControl;

        protected virtual bool ShowFooter => true;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected readonly BeatmapOptionsOverlay BeatmapOptions;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected readonly Footer Footer;

        /// <summary>
        /// Contains any panel which is triggered by a footer button.
        /// Helps keep them located beneath the footer itself.
        /// </summary>
        protected readonly Container FooterPanels;

        protected override BackgroundScreen CreateBackground()
        {
            var background = new BackgroundScreenBeatmap();
            return background;
        }

        protected readonly BeatmapCarousel Carousel;
        private readonly BeatmapInfoWedge beatmapInfoWedge;
        private DialogOverlay dialogOverlay;
        private BeatmapManager beatmaps;

        protected readonly ModSelectOverlay ModSelect;

        protected SampleChannel SampleConfirm;
        private SampleChannel sampleChangeDifficulty;
        private SampleChannel sampleChangeBeatmap;

        protected readonly BeatmapDetailArea BeatmapDetails;

        private readonly Bindable<RulesetInfo> decoupledRuleset = new Bindable<RulesetInfo>();

        [Cached]
        [Cached(Type = typeof(IBindable<IEnumerable<Mod>>))]
        protected readonly Bindable<IEnumerable<Mod>> SelectedMods = new Bindable<IEnumerable<Mod>>(new Mod[] { });

        protected SongSelect()
        {
            const float carousel_width = 640;

            AddRangeInternal(new Drawable[]
            {
                new ParallaxContainer
                {
                    Masking = true,
                    ParallaxAmount = 0.005f,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new WedgeBackground
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = carousel_width * 0.76f },
                        }
                    }
                },
                new Container
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(wedged_container_size.X, 1),
                    Padding = new MarginPadding
                    {
                        Bottom = 50,
                        Top = wedged_container_size.Y + left_area_padding,
                        Left = left_area_padding,
                        Right = left_area_padding * 2,
                    },
                    Child = BeatmapDetails = new BeatmapDetailArea
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 10, Right = 5 },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 2, //avoid horizontal masking so the panels don't clip when screen stack is pushed.
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 0.5f,
                        Children = new Drawable[]
                        {
                            Carousel = new BeatmapCarousel
                            {
                                Masking = false,
                                RelativeSizeAxes = Axes.Y,
                                Size = new Vector2(carousel_width, 1),
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                SelectionChanged = updateSelectedBeatmap,
                                BeatmapSetsChanged = carouselBeatmapsLoaded,
                            },
                            FilterControl = new FilterControl
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 100,
                                FilterChanged = c => Carousel.Filter(c),
                                Background = { Width = 2 },
                                Exit = () =>
                                {
                                    if (this.IsCurrentScreen())
                                        this.Exit();
                                },
                            },
                        }
                    },
                },
                beatmapInfoWedge = new BeatmapInfoWedge
                {
                    Size = wedged_container_size,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding
                    {
                        Top = left_area_padding,
                        Right = left_area_padding,
                    },
                },
                new ResetScrollContainer(() => Carousel.ScrollToSelected())
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 250,
                }
            });

            if (ShowFooter)
            {
                AddInternal(FooterPanels = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding
                    {
                        Bottom = Footer.HEIGHT,
                    },
                });
                AddInternal(Footer = new Footer
                {
                    OnBack = ExitFromBack,
                });

                FooterPanels.AddRange(new Drawable[]
                {
                    BeatmapOptions = new BeatmapOptionsOverlay(),
                    ModSelect = new ModSelectOverlay
                    {
                        RelativeSizeAxes = Axes.X,
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                    }
                });
            }

            BeatmapDetails.Leaderboard.ScoreSelected += s => this.Push(new SoloResults(s));
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapManager beatmaps, AudioManager audio, DialogOverlay dialog, OsuColour colours, SkinManager skins, Bindable<IEnumerable<Mod>> selectedMods)
        {
            if (selectedMods != null)
                SelectedMods.BindTo(selectedMods);

            if (Footer != null)
            {
                Footer.AddButton(@"mods", colours.Yellow, ModSelect, Key.F1);
                Footer.AddButton(@"random", colours.Green, triggerRandom, Key.F2);
                Footer.AddButton(@"options", colours.Blue, BeatmapOptions, Key.F3);

                BeatmapOptions.AddButton(@"Delete", @"all difficulties", FontAwesome.Solid.Trash, colours.Pink, () => delete(Beatmap.Value.BeatmapSetInfo), Key.Number4, float.MaxValue);
                BeatmapOptions.AddButton(@"Remove", @"from unplayed", FontAwesome.Regular.TimesCircle, colours.Purple, null, Key.Number1);
                BeatmapOptions.AddButton(@"Clear", @"local scores", FontAwesome.Solid.Eraser, colours.Purple, () => clearScores(Beatmap.Value.BeatmapInfo), Key.Number2);
            }

            if (this.beatmaps == null)
                this.beatmaps = beatmaps;

            this.beatmaps.ItemAdded += onBeatmapSetAdded;
            this.beatmaps.ItemRemoved += onBeatmapSetRemoved;
            this.beatmaps.BeatmapHidden += onBeatmapHidden;
            this.beatmaps.BeatmapRestored += onBeatmapRestored;

            dialogOverlay = dialog;

            sampleChangeDifficulty = audio.Sample.Get(@"SongSelect/select-difficulty");
            sampleChangeBeatmap = audio.Sample.Get(@"SongSelect/select-expand");
            SampleConfirm = audio.Sample.Get(@"SongSelect/confirm-selection");

            Carousel.LoadBeatmapSetsFromManager(this.beatmaps);

            if (dialogOverlay != null)
            {
                Schedule(() =>
                {
                    // if we have no beatmaps but osu-stable is found, let's prompt the user to import.
                    if (!beatmaps.GetAllUsableBeatmapSets().Any() && beatmaps.StableInstallationAvailable)
                        dialogOverlay.Push(new ImportFromStablePopup(() =>
                        {
                            beatmaps.ImportFromStableAsync();
                            skins.ImportFromStableAsync();
                        }));
                });
            }
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(this);
            dependencies.CacheAs(decoupledRuleset);
            dependencies.CacheAs<IBindable<RulesetInfo>>(decoupledRuleset);

            return dependencies;
        }

        protected virtual void ExitFromBack()
        {
            if (ModSelect.State == Visibility.Visible)
            {
                ModSelect.Hide();
                return;
            }

            this.Exit();
        }

        public void Edit(BeatmapInfo beatmap = null)
        {
            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap ?? beatmapNoDebounce);
            this.Push(new Editor());
        }

        /// <summary>
        /// Call to make a selection and perform the default action for this SongSelect.
        /// </summary>
        /// <param name="beatmap">An optional beatmap to override the current carousel selection.</param>
        /// <param name="performStartAction">Whether to trigger <see cref="OnStart"/>.</param>
        public void FinaliseSelection(BeatmapInfo beatmap = null, bool performStartAction = true)
        {
            // This is very important as we have not yet bound to screen-level bindables before the carousel load is completed.
            if (!Carousel.BeatmapSetsLoaded)
                return;

            // if we have a pending filter operation, we want to run it now.
            // it could change selection (ie. if the ruleset has been changed).
            Carousel.FlushPendingFilterOperations();

            // avoid attempting to continue before a selection has been obtained.
            // this could happen via a user interaction while the carousel is still in a loading state.
            if (Carousel.SelectedBeatmap == null) return;

            if (beatmap != null)
                Carousel.SelectBeatmap(beatmap);

            if (selectionChangedDebounce?.Completed == false)
            {
                selectionChangedDebounce.RunTask();
                selectionChangedDebounce.Cancel(); // cancel the already scheduled task.
                selectionChangedDebounce = null;
            }

            if (performStartAction)
                OnStart();
        }

        /// <summary>
        /// Called when a selection is made.
        /// </summary>
        /// <returns>If a resultant action occurred that takes the user away from SongSelect.</returns>
        protected abstract bool OnStart();

        private ScheduledDelegate selectionChangedDebounce;

        private void workingBeatmapChanged(ValueChangedEvent<WorkingBeatmap> e)
        {
            if (e.NewValue is DummyWorkingBeatmap) return;

            if (this.IsCurrentScreen() && !Carousel.SelectBeatmap(e.NewValue?.BeatmapInfo, false))
                // If selecting new beatmap without bypassing filters failed, there's possibly a ruleset mismatch
                if (e.NewValue?.BeatmapInfo?.Ruleset != null && e.NewValue.BeatmapInfo.Ruleset != decoupledRuleset.Value)
                {
                    Ruleset.Value = e.NewValue.BeatmapInfo.Ruleset;
                    Carousel.SelectBeatmap(e.NewValue.BeatmapInfo);
                }
        }

        // We need to keep track of the last selected beatmap ignoring debounce to play the correct selection sounds.
        private BeatmapInfo beatmapNoDebounce;
        private RulesetInfo rulesetNoDebounce;

        private void updateSelectedBeatmap(BeatmapInfo beatmap)
        {
            if (beatmap?.Equals(beatmapNoDebounce) == true)
                return;

            beatmapNoDebounce = beatmap;
            performUpdateSelected();
        }

        private void updateSelectedRuleset(RulesetInfo ruleset)
        {
            if (ruleset?.Equals(rulesetNoDebounce) == true)
                return;

            rulesetNoDebounce = ruleset;
            performUpdateSelected();
        }

        /// <summary>
        /// selection has been changed as the result of a user interaction.
        /// </summary>
        private void performUpdateSelected()
        {
            var beatmap = beatmapNoDebounce;
            var ruleset = rulesetNoDebounce;

            selectionChangedDebounce?.Cancel();

            if (beatmap == null)
                run();
            else
                selectionChangedDebounce = Scheduler.AddDelayed(run, 200);

            void run()
            {
                Logger.Log($"updating selection with beatmap:{beatmap?.ID.ToString() ?? "null"} ruleset:{ruleset?.ID.ToString() ?? "null"}");

                bool preview = false;

                if (ruleset?.Equals(decoupledRuleset.Value) == false)
                {
                    Logger.Log($"ruleset changed from \"{decoupledRuleset.Value}\" to \"{ruleset}\"");

                    Beatmap.Value.Mods.Value = Enumerable.Empty<Mod>();
                    decoupledRuleset.Value = ruleset;

                    // force a filter before attempting to change the beatmap.
                    // we may still be in the wrong ruleset as there is a debounce delay on ruleset changes.
                    Carousel.Filter(null, false);

                    // Filtering only completes after the carousel runs Update.
                    // If we also have a pending beatmap change we should delay it one frame.
                    selectionChangedDebounce = Schedule(run);
                    return;
                }

                // We may be arriving here due to another component changing the bindable Beatmap.
                // In these cases, the other component has already loaded the beatmap, so we don't need to do so again.
                if (!Equals(beatmap, Beatmap.Value.BeatmapInfo))
                {
                    Logger.Log($"beatmap changed from \"{Beatmap.Value.BeatmapInfo}\" to \"{beatmap}\"");

                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap, Beatmap.Value);

                    if (beatmap != null)
                    {
                        if (beatmap.BeatmapSetInfoID == beatmapNoDebounce?.BeatmapSetInfoID)
                            sampleChangeDifficulty.Play();
                        else
                            sampleChangeBeatmap.Play();
                    }
                }

                if (this.IsCurrentScreen())
                    ensurePlayingSelected();
                UpdateBeatmap(Beatmap.Value);
            }
        }

        private void triggerRandom()
        {
            if (GetContainingInputManager().CurrentState.Keyboard.ShiftPressed)
                Carousel.SelectPreviousRandom();
            else
                Carousel.SelectNextRandom();
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            this.FadeInFromZero(250);
            FilterControl.Activate();
        }

        private const double logo_transition = 250;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            Vector2 position = new Vector2(0.95f, 0.96f);

            if (logo.Alpha > 0.8f)
            {
                logo.MoveTo(position, 500, Easing.OutQuint);
            }
            else
            {
                logo.Hide();
                logo.ScaleTo(0.2f);
                logo.MoveTo(position);
            }

            logo.FadeIn(logo_transition, Easing.OutQuint);
            logo.ScaleTo(0.4f, logo_transition, Easing.OutQuint);

            logo.Action = () =>
            {
                FinaliseSelection();
                return false;
            };
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            logo.ScaleTo(0.2f, logo_transition / 2, Easing.Out);
            logo.FadeOut(logo_transition / 2, Easing.Out);
        }

        public override void OnResuming(IScreen last)
        {
            BeatmapDetails.Leaderboard.RefreshScores();

            Beatmap.Value.Track.Looping = true;

            if (Beatmap != null && !Beatmap.Value.BeatmapSetInfo.DeletePending)
            {
                UpdateBeatmap(Beatmap.Value);
                ensurePlayingSelected();
            }

            base.OnResuming(last);

            this.FadeIn(250);

            this.ScaleTo(1, 250, Easing.OutSine);

            FilterControl.Activate();
        }

        public override void OnSuspending(IScreen next)
        {
            ModSelect.Hide();

            this.ScaleTo(1.1f, 250, Easing.InSine);

            this.FadeOut(250);

            FilterControl.Deactivate();
            base.OnSuspending(next);
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            beatmapInfoWedge.State = Visibility.Hidden;

            this.FadeOut(100);

            FilterControl.Deactivate();

            if (Beatmap.Value.Track != null)
                Beatmap.Value.Track.Looping = false;

            SelectedMods.UnbindAll();
            Beatmap.Value.Mods.Value = new Mod[] { };

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            decoupledRuleset.UnbindAll();

            if (beatmaps != null)
            {
                beatmaps.ItemAdded -= onBeatmapSetAdded;
                beatmaps.ItemRemoved -= onBeatmapSetRemoved;
                beatmaps.BeatmapHidden -= onBeatmapHidden;
                beatmaps.BeatmapRestored -= onBeatmapRestored;
            }
        }

        /// <summary>
        /// Allow components in SongSelect to update their loaded beatmap details.
        /// This is a debounced call (unlike directly binding to WorkingBeatmap.ValueChanged).
        /// </summary>
        /// <param name="beatmap">The working beatmap.</param>
        protected virtual void UpdateBeatmap(WorkingBeatmap beatmap)
        {
            beatmap.Mods.BindTo(SelectedMods);

            Logger.Log($"working beatmap updated to {beatmap}");

            if (Background is BackgroundScreenBeatmap backgroundModeBeatmap)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurAmount.Value = BACKGROUND_BLUR;
                backgroundModeBeatmap.FadeColour(Color4.White, 250);
            }

            beatmapInfoWedge.Beatmap = beatmap;

            BeatmapDetails.Beatmap = beatmap;

            if (beatmap.Track != null)
                beatmap.Track.Looping = true;
        }

        private void ensurePlayingSelected(bool restart = false)
        {
            Track track = Beatmap.Value.Track;

            if (!track.IsRunning || restart)
            {
                // Ensure the track is added to the TrackManager, since it is removed after the player finishes the map.
                // Using AddItemToList rather than AddItem so that it doesn't attempt to register adjustment dependencies more than once.
                Game.Audio.Track.AddItemToList(track);
                track.RestartPoint = Beatmap.Value.Metadata.PreviewTime;
                track.Restart();
            }
        }

        private void onBeatmapSetAdded(BeatmapSetInfo s, bool existing) => Carousel.UpdateBeatmapSet(s);
        private void onBeatmapSetRemoved(BeatmapSetInfo s) => Carousel.RemoveBeatmapSet(s);
        private void onBeatmapRestored(BeatmapInfo b) => Carousel.UpdateBeatmapSet(beatmaps.QueryBeatmapSet(s => s.ID == b.BeatmapSetInfoID));
        private void onBeatmapHidden(BeatmapInfo b) => Carousel.UpdateBeatmapSet(beatmaps.QueryBeatmapSet(s => s.ID == b.BeatmapSetInfoID));

        private void carouselBeatmapsLoaded()
        {
            bindBindables();

            if (!Beatmap.IsDefault && Beatmap.Value.BeatmapSetInfo?.DeletePending == false && Beatmap.Value.BeatmapSetInfo?.Protected == false
                && Carousel.SelectBeatmap(Beatmap.Value.BeatmapInfo, false))
                return;

            if (Carousel.SelectedBeatmapSet == null && !Carousel.SelectNextRandom())
            {
                // in the case random selection failed, we want to trigger selectionChanged
                // to show the dummy beatmap (we have nothing else to display).
                performUpdateSelected();
            }
        }

        private bool boundLocalBindables;

        private void bindBindables()
        {
            if (boundLocalBindables)
                return;

            // manual binding to parent ruleset to allow for delayed load in the incoming direction.
            rulesetNoDebounce = decoupledRuleset.Value = Ruleset.Value;
            Ruleset.ValueChanged += r => updateSelectedRuleset(r.NewValue);

            decoupledRuleset.ValueChanged += r => Ruleset.Value = r.NewValue;
            decoupledRuleset.DisabledChanged += r => Ruleset.Disabled = r;

            Beatmap.BindDisabledChanged(disabled => Carousel.AllowSelection = !disabled, true);
            Beatmap.BindValueChanged(workingBeatmapChanged);

            boundLocalBindables = true;
        }

        private void delete(BeatmapSetInfo beatmap)
        {
            if (beatmap == null || beatmap.ID <= 0) return;

            dialogOverlay?.Push(new BeatmapDeleteDialog(beatmap));
        }

        private void clearScores(BeatmapInfo beatmap)
        {
            if (beatmap == null || beatmap.ID <= 0) return;

            dialogOverlay?.Push(new BeatmapClearScoresDialog(beatmap, () =>
                // schedule done here rather than inside the dialog as the dialog may fade out and never callback.
                Schedule(() => BeatmapDetails.Leaderboard.RefreshScores())));
        }

        public override bool OnPressed(GlobalAction action)
        {
            if (!this.IsCurrentScreen()) return false;

            switch (action)
            {
                case GlobalAction.Select:
                    FinaliseSelection();
                    return true;
            }

            return base.OnPressed(action);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            switch (e.Key)
            {
                case Key.Delete:
                    if (e.ShiftPressed)
                    {
                        if (!Beatmap.IsDefault)
                            delete(Beatmap.Value.BeatmapSetInfo);
                        return true;
                    }

                    break;
            }

            return base.OnKeyDown(e);
        }

        private class ResetScrollContainer : Container
        {
            private readonly Action onHoverAction;

            public ResetScrollContainer(Action onHoverAction)
            {
                this.onHoverAction = onHoverAction;
            }

            protected override bool OnHover(HoverEvent e)
            {
                onHoverAction?.Invoke();
                return base.OnHover(e);
            }
        }
    }
}
