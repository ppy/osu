// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Utils;
using osu.Game.Screens.Menu;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osuTK.Graphics;
using IntroSequence = osu.Game.Configuration.IntroSequence;

namespace osu.Game.Screens
{
    public partial class Loader : StartupScreen
    {
        private bool showDisclaimer;

        public override bool CursorVisible => cursorVisible;

        private bool cursorVisible = false;

        public Loader()
        {
            ValidForResume = false;
        }

        private OsuScreen loadableScreen;
        private ShaderPrecompiler precompiler;

        private IntroSequence introSequence;
        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;

        private Color4 backgroundColor;

        protected virtual OsuScreen CreateLoadableScreen() => new Disclaimer(getIntroSequence(), showDisclaimer, backgroundColor);

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenPureColor(backgroundColor);

        private IntroScreen getIntroSequence()
        {
            if (introSequence == IntroSequence.Random)
                introSequence = (IntroSequence)RNG.Next(0, (int)IntroSequence.Random);

            switch (introSequence)
            {
                case IntroSequence.Circles:
                    return new IntroCircles(createMainMenu);

                case IntroSequence.CirclesCN:
                    return new IntroCircles(createMainMenu, true);

                case IntroSequence.TrianglesCN:
                    return new IntroTriangles(createMainMenu, true);

                case IntroSequence.SkippedIntro:
                    return new IntroSkipped(createMainMenu);

                case IntroSequence.Welcome:
                    return new IntroWelcome(createMainMenu);

                default:
                    return new IntroTriangles(createMainMenu);
            }

            MainMenu createMainMenu() => new MainMenu();
        }

        protected virtual ShaderPrecompiler CreateShaderPrecompiler() => new ShaderPrecompiler();

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            LoadComponentAsync(precompiler = CreateShaderPrecompiler(), AddInternal);

            LoadComponentAsync(loadableScreen = CreateLoadableScreen());

            LoadComponentAsync(spinner = new LoadingSpinner(true, true)
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(40),
            }, _ =>
            {
                AddInternal(spinner);
                spinnerShow = Scheduler.AddDelayed(spinner.Show, 200);
            });

            checkIfLoaded();
        }

        private void checkIfLoaded()
        {
            if (loadableScreen?.LoadState != LoadState.Ready || !precompiler.FinishedCompiling)
            {
                Schedule(checkIfLoaded);
                return;
            }

            spinnerShow?.Cancel();

            if (spinner.State.Value == Visibility.Visible)
            {
                spinner.Hide();
                Scheduler.AddDelayed(() => this.Push(loadableScreen), LoadingSpinner.TRANSITION_DURATION);
            }
            else
                this.Push(loadableScreen);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuConfigManager config, MConfigManager mConfig)
        {
            showDisclaimer = game.IsDeployedBuild;
            introSequence = config.Get<IntroSequence>(OsuSetting.IntroSequence);
            backgroundColor = mConfig.GetCustomLoaderColor();
        }

        /// <summary>
        /// Compiles a set of shaders before continuing. Attempts to draw some frames between compilation by limiting to one compile per draw frame.
        /// </summary>
        public partial class ShaderPrecompiler : Drawable
        {
            private readonly List<IShader> loadTargets = new List<IShader>();

            public bool FinishedCompiling { get; private set; }

            [BackgroundDependencyLoader]
            private void load(ShaderManager manager)
            {
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));

                loadTargets.Add(manager.Load(@"CursorTrail", FragmentShaderDescriptor.TEXTURE));

                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, "TriangleBorder"));

                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));
            }

            protected virtual bool AllLoaded => loadTargets.All(s => s.IsLoaded);

            protected override void Update()
            {
                base.Update();

                // if our target is null we are done.
                if (AllLoaded)
                {
                    FinishedCompiling = true;
                    Expire();
                }
            }
        }
    }
}
