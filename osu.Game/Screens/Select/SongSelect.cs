// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select.Options;

namespace osu.Game.Screens.Select
{
    public abstract class SongSelect : OsuScreen
    {
        private static readonly Vector2 wedged_container_size = new Vector2(0.5f, 245);
        private static readonly Vector2 background_blur = new Vector2(20);
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

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap();

        protected Container LeftContent;

        protected readonly BeatmapCarousel Carousel;
        private readonly BeatmapInfoWedge beatmapInfoWedge;
        private DialogOverlay dialogOverlay;
        private BeatmapManager beatmaps;

        private SampleChannel sampleChangeDifficulty;
        private SampleChannel sampleChangeBeatmap;

        protected new readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(this);
            dependencies.CacheAs(Ruleset);
            dependencies.CacheAs<IBindable<RulesetInfo>>(Ruleset);

            return dependencies;
        }

        protected SongSelect()
        {
            const float carousel_width = 640;
            const float filter_height = 100;

            AddRange(new Drawable[]
            {
                new ParallaxContainer
                {
                    Padding = new MarginPadding { Top = filter_height },
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
                LeftContent = new Container
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
                                Height = filter_height,
                                FilterChanged = c => Carousel.Filter(c),
                                Background = { Width = 2 },
                                Exit = () =>
                                {
                                    if (IsCurrentScreen)
                                        Exit();
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
                Add(FooterPanels = new Container
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
                Add(Footer = new Footer
                {
                    OnBack = Exit,
                });

                FooterPanels.Add(BeatmapOptions = new BeatmapOptionsOverlay());
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(BeatmapManager beatmaps, AudioManager audio, DialogOverlay dialog, OsuColour colours)
        {
            if (Footer != null)
            {
                Footer.AddButton(@"random", colours.Green, triggerRandom, Key.F2);
                Footer.AddButton(@"options", colours.Blue, BeatmapOptions, Key.F3);

                BeatmapOptions.AddButton(@"Delete", @"Beatmap", FontAwesome.fa_trash, colours.Pink, () => delete(Beatmap.Value.BeatmapSetInfo), Key.Number4, float.MaxValue);
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

            Carousel.LoadBeatmapSetsFromManager(this.beatmaps);
        }

        public void Edit(BeatmapInfo beatmap)
        {
            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap, Beatmap.Value);
            Push(new Editor());
        }

        /// <summary>
        /// Call to make a selection and perform the default action for this SongSelect.
        /// </summary>
        /// <param name="beatmap">An optional beatmap to override the current carousel selection.</param>
        /// <param name="performStartAction">Whether to trigger <see cref="OnStart"/>.</param>
        public void FinaliseSelection(BeatmapInfo beatmap = null, bool performStartAction = true)
        {
            // avoid attempting to continue before a selection has been obtained.
            // this could happen via a user interaction while the carousel is still in a loading state.
            if (Carousel.SelectedBeatmap == null) return;

            // if we have a pending filter operation, we want to run it now.
            // it could change selection (ie. if the ruleset has been changed).
            Carousel.FlushPendingFilterOperations();

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

        private void workingBeatmapChanged(WorkingBeatmap beatmap)
        {
            if (beatmap is DummyWorkingBeatmap) return;

            if (IsCurrentScreen && !Carousel.SelectBeatmap(beatmap?.BeatmapInfo, false))
                // If selecting new beatmap without bypassing filters failed, there's possibly a ruleset mismatch
                if (beatmap?.BeatmapInfo?.Ruleset != null && beatmap.BeatmapInfo.Ruleset != Ruleset.Value)
                {
                    base.Ruleset.Value = beatmap.BeatmapInfo.Ruleset;
                    Carousel.SelectBeatmap(beatmap.BeatmapInfo);
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

            void run()
            {
                Logger.Log($"updating selection with beatmap:{beatmap?.ID.ToString() ?? "null"} ruleset:{ruleset?.ID.ToString() ?? "null"}");

                bool preview = false;

                if (ruleset?.Equals(Ruleset.Value) == false)
                {
                    Logger.Log($"ruleset changed from \"{Ruleset.Value}\" to \"{ruleset}\"");

                    Beatmap.Value.Mods.Value = Enumerable.Empty<Mod>();
                    Ruleset.Value = ruleset;

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

                    preview = beatmap?.BeatmapSetInfoID != Beatmap.Value?.BeatmapInfo.BeatmapSetInfoID;
                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap, Beatmap.Value);

                    if (beatmap != null)
                    {
                        if (beatmap.BeatmapSetInfoID == beatmapNoDebounce?.BeatmapSetInfoID)
                            sampleChangeDifficulty.Play();
                        else
                            sampleChangeBeatmap.Play();
                    }
                }

                ensurePlayingSelected(preview);
                UpdateBeatmap(Beatmap.Value);
            }

            selectionChangedDebounce?.Cancel();

            if (beatmap == null)
                run();
            else
                selectionChangedDebounce = Scheduler.AddDelayed(run, 200);
        }

        private void triggerRandom()
        {
            if (GetContainingInputManager().CurrentState.Keyboard.ShiftPressed)
                Carousel.SelectPreviousRandom();
            else
                Carousel.SelectNextRandom();
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Content.FadeInFromZero(250);
            FilterControl.Activate();
        }

        private const double logo_transition = 250;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.RelativePositionAxes = Axes.Both;
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

        protected override void OnResuming(Screen last)
        {
            if (Beatmap != null && !Beatmap.Value.BeatmapSetInfo.DeletePending)
            {
                UpdateBeatmap(Beatmap.Value);
                ensurePlayingSelected();
            }

            base.OnResuming(last);

            Content.FadeIn(250);

            Content.ScaleTo(1, 250, Easing.OutSine);

            FilterControl.Activate();
        }

        protected override void OnSuspending(Screen next)
        {
            Content.ScaleTo(1.1f, 250, Easing.InSine);

            Content.FadeOut(250);

            FilterControl.Deactivate();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            FinaliseSelection(performStartAction: false);

            beatmapInfoWedge.State = Visibility.Hidden;

            Content.FadeOut(100);

            FilterControl.Deactivate();

            return base.OnExiting(next);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Ruleset.UnbindAll();

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
            Logger.Log($"working beatmap updated to {beatmap}");

            if (Background is BackgroundScreenBeatmap backgroundModeBeatmap)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurTo(background_blur, 750, Easing.OutQuint);
                backgroundModeBeatmap.FadeTo(1, 250);
            }

            beatmapInfoWedge.Beatmap = beatmap;
        }

        private void ensurePlayingSelected(bool preview = false)
        {
            Track track = Beatmap.Value.Track;

            if (!track.IsRunning)
            {
                // Ensure the track is added to the TrackManager, since it is removed after the player finishes the map.
                // Using AddItemToList rather than AddItem so that it doesn't attempt to register adjustment dependencies more than once.
                Game.Audio.Track.AddItemToList(track);
                if (preview) track.Seek(Beatmap.Value.Metadata.PreviewTime);
                track.Start();
            }
        }

        private void onBeatmapSetAdded(BeatmapSetInfo s) => Carousel.UpdateBeatmapSet(s);
        private void onBeatmapSetRemoved(BeatmapSetInfo s) => Carousel.RemoveBeatmapSet(s);
        private void onBeatmapRestored(BeatmapInfo b) => Carousel.UpdateBeatmapSet(beatmaps.QueryBeatmapSet(s => s.ID == b.BeatmapSetInfoID));
        private void onBeatmapHidden(BeatmapInfo b) => Carousel.UpdateBeatmapSet(beatmaps.QueryBeatmapSet(s => s.ID == b.BeatmapSetInfoID));

        private void carouselBeatmapsLoaded()
        {
            if (rulesetNoDebounce == null)
            {
                // manual binding to parent ruleset to allow for delayed load in the incoming direction.
                rulesetNoDebounce = Ruleset.Value = base.Ruleset.Value;
                base.Ruleset.ValueChanged += updateSelectedRuleset;
                Ruleset.ValueChanged += r => base.Ruleset.Value = r;

                Beatmap.BindDisabledChanged(disabled => Carousel.AllowSelection = !disabled, true);
                Beatmap.BindValueChanged(workingBeatmapChanged);
            }

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

        private void delete(BeatmapSetInfo beatmap)
        {
            if (beatmap == null || beatmap.ID <= 0) return;
            dialogOverlay?.Push(new BeatmapDeleteDialog(beatmap));
        }

        public override bool OnPressed(GlobalAction action)
        {
            if (!IsCurrentScreen) return false;

            switch (action)
            {
                case GlobalAction.Select:
                    FinaliseSelection();
                    return true;
            }

            return base.OnPressed(action);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.Delete:
                    if (state.Keyboard.ShiftPressed)
                    {
                        if (!Beatmap.IsDefault)
                            delete(Beatmap.Value.BeatmapSetInfo);
                        return true;
                    }

                    break;
            }

            return base.OnKeyDown(state, args);
        }

        private class ResetScrollContainer : Container
        {
            private readonly Action onHoverAction;

            public ResetScrollContainer(Action onHoverAction)
            {
                this.onHoverAction = onHoverAction;
            }

            protected override bool OnHover(InputState state)
            {
                onHoverAction?.Invoke();
                return base.OnHover(state);
            }
        }
    }
}
