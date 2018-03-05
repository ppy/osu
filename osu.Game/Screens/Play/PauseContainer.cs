// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A container which handles pausing children, displaying a pause overlay with choices etc.
    /// This alleviates a lot of the intricate pause logic from being in <see cref="Player"/>
    /// </summary>
    public class PauseContainer : Container
    {
        public bool IsPaused { get; private set; }

        public Func<bool> CheckCanPause;

        private const double pause_cooldown = 1000;
        private double lastPauseActionTime;

        private readonly PauseOverlay pauseOverlay;
        private readonly ResumeOverlay resumeOverlay;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public int Retries { set { pauseOverlay.Retries = value; } }

        public bool CanPause => (CheckCanPause?.Invoke() ?? true) && Time.Current >= lastPauseActionTime + pause_cooldown;
        public bool IsResuming { get; private set; }

        public Action OnRetry;
        public Action OnQuit;

        public Action OnResume;
        public Action OnPause;

        private readonly IAdjustableClock adjustableClock;
        private readonly FramedClock framedClock;

        public PauseContainer(FramedClock framedClock, IAdjustableClock adjustableClock,
                              RulesetInfo rulesetInfo, PassThroughInputManager rulesetInputManager, CursorContainer cursor)
        {
            this.framedClock = framedClock;
            this.adjustableClock = adjustableClock;

            RelativeSizeAxes = Axes.Both;

            AddInternal(content = new Container
            {
                Clock = this.framedClock,
                ProcessCustomClock = false,
                RelativeSizeAxes = Axes.Both
            });

            resumeOverlay = createResumeOverlay(rulesetInfo, rulesetInputManager, cursor, resumeInternal, () =>
                          {
                              IsResuming = false;
                              pauseOverlay.Show();
                          });

            if (resumeOverlay != null)
                AddInternal(resumeOverlay);

            AddInternal(pauseOverlay = new PauseOverlay
            {
                OnResume = () =>
                {
                    IsResuming = true;
                    this.Delay(resumeOverlay == null ? 400 : 0).Schedule(Resume); ;
                },
                OnRetry = () => OnRetry(),
                OnQuit = () => OnQuit(),
            });
        }

        private ResumeOverlay createResumeOverlay(RulesetInfo rulesetInfo, PassThroughInputManager inputManager, CursorContainer cursor, Action resumeAction, Action escAction)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyName = rulesetInfo.InstantiationInfo.Split(',')[1].Trim();
            var assembly = assemblies.FirstOrDefault(a => !a.IsDynamic && a.FullName.Split(',').First().Trim() == assemblyName);

            var osuResumeOverlayType = assembly?.ExportedTypes.FirstOrDefault(t => !t.IsAbstract && t.IsSubclassOf(typeof(ResumeOverlay)));
            return osuResumeOverlayType != null ? Activator.CreateInstance(osuResumeOverlayType, inputManager, cursor, resumeAction, escAction) as ResumeOverlay : null;
        }

        public void Pause(bool force = false) => Schedule(() => // Scheduled to ensure a stable position in execution order, no matter how it was called.
        {
            if (!CanPause && !force) return;

            if (IsPaused) return;

            // stop the seekable clock (stops the audio eventually)
            adjustableClock.Stop();
            IsPaused = true;

            OnPause?.Invoke();
            pauseOverlay.Show();

            lastPauseActionTime = Time.Current;
        });

        public void Resume()
        {
            if (!IsPaused) return;

            if (resumeOverlay == null)
                resumeInternal();
            else
                resumeOverlay.Show();
        }

        private void resumeInternal()
        {
            pauseOverlay.Hide();
            resumeOverlay?.Hide();

            IsPaused = false;
            lastPauseActionTime = Time.Current;

            // seek back to the time of the framed clock.
            // this accounts for the audio clock potentially taking time to enter a completely stopped state.
            adjustableClock.Seek(framedClock.CurrentTime);
            adjustableClock.Start();

            OnResume?.Invoke();
            IsResuming = false;
        }

        private OsuGameBase game;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            this.game = game;
        }

        protected override void Update()
        {
            // eagerly pause when we lose window focus (if we are locally playing).
            if (!game.IsActive && CanPause)
                Pause();

            if (!IsPaused)
                framedClock.ProcessFrame();

            base.Update();
        }

        public class PauseOverlay : GameplayMenuOverlay
        {
            public Action OnResume;

            public override string Header => "paused";
            public override string Description => "you're not going to do what i think you're going to do, are ya?";

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (!args.Repeat && args.Key == Key.Escape)
                {
                    InternalButtons.Children.First().TriggerOnClick();
                    return true;
                }

                return base.OnKeyDown(state, args);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AddButton("Continue", colours.Green, () => OnResume?.Invoke());
                AddButton("Retry", colours.YellowDark, () => OnRetry?.Invoke());
                AddButton("Quit", new Color4(170, 27, 39, 255), () => OnQuit?.Invoke());
            }
        }

        public abstract class ResumeOverlay : GameplayMenuOverlay
        {
            protected readonly PassThroughInputManager RulesetInputManager;
            protected readonly CursorContainer Cursor;
            protected readonly Action EscAction;
            protected readonly Action ResumeAction;

            protected ResumeOverlay(PassThroughInputManager rulesetInputManager, CursorContainer cursor, Action resumeAction, Action escAction)
            {
                RulesetInputManager = rulesetInputManager;
                Cursor = cursor;
                ResumeAction = resumeAction;
                EscAction = escAction;
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (!args.Repeat && args.Key == Key.Escape)
                {
                    EscAction();
                    return true;
                }

                return base.OnKeyDown(state, args);
            }
        }
    }
}
