// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.MathUtils;
using osu.Game.Screens.Menu;
using osuTK;
using osu.Framework.Screens;
using osu.Game.Configuration;
using IntroSequence = osu.Game.Configuration.IntroSequence;

namespace osu.Game.Screens
{
    public class Loader : StartupScreen
    {
        private bool showDisclaimer;

        public Loader()
        {
            ValidForResume = false;
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.BeatMatching = false;
            logo.Triangles = false;
            logo.RelativePositionAxes = Axes.None;
            logo.Origin = Anchor.BottomRight;
            logo.Anchor = Anchor.BottomRight;
            logo.Position = new Vector2(-40);
            logo.Scale = new Vector2(0.2f);

            logo.Delay(500).FadeInFromZero(1000, Easing.OutQuint);
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            base.LogoSuspending(logo);
            logo.FadeOut(logo.Alpha * 400);
        }

        private OsuScreen loadableScreen;
        private ShaderPrecompiler precompiler;

        private IntroSequence introSequence;

        protected virtual OsuScreen CreateLoadableScreen()
        {
            if (showDisclaimer)
                return new Disclaimer(getIntroSequence());

            return getIntroSequence();
        }

        private IntroScreen getIntroSequence()
        {
            if (introSequence == IntroSequence.Random)
                introSequence = (IntroSequence)RNG.Next(0, (int)IntroSequence.Random);

            switch (introSequence)
            {
                case IntroSequence.Circles:
                    return new IntroCircles();

                default:
                    return new IntroTriangles();
            }
        }

        protected virtual ShaderPrecompiler CreateShaderPrecompiler() => new ShaderPrecompiler();

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            LoadComponentAsync(precompiler = CreateShaderPrecompiler(), AddInternal);
            LoadComponentAsync(loadableScreen = CreateLoadableScreen());

            checkIfLoaded();
        }

        private void checkIfLoaded()
        {
            if (loadableScreen.LoadState != LoadState.Ready || !precompiler.FinishedCompiling)
            {
                Schedule(checkIfLoaded);
                return;
            }

            this.Push(loadableScreen);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuConfigManager config)
        {
            showDisclaimer = game.IsDeployedBuild;
            introSequence = config.Get<IntroSequence>(OsuSetting.IntroSequence);
        }

        /// <summary>
        /// Compiles a set of shaders before continuing. Attempts to draw some frames between compilation by limiting to one compile per draw frame.
        /// </summary>
        public class ShaderPrecompiler : Drawable
        {
            private readonly List<IShader> loadTargets = new List<IShader>();

            public bool FinishedCompiling { get; private set; }

            [BackgroundDependencyLoader]
            private void load(ShaderManager manager)
            {
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));

                loadTargets.Add(manager.Load(@"CursorTrail", FragmentShaderDescriptor.TEXTURE));

                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE_ROUNDED));
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
