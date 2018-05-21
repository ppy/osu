// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Game.Screens.Menu;
using OpenTK;
using osu.Framework.Screens;

namespace osu.Game.Screens
{
    public class Loader : OsuScreen
    {
        private bool showDisclaimer;

        protected override bool HideOverlaysOnEnter => true;

        protected override bool AllowBackButton => false;

        public Loader()
        {
            ValidForResume = false;
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            logo.Triangles = false;
            logo.Origin = Anchor.BottomRight;
            logo.Anchor = Anchor.BottomRight;
            logo.Position = new Vector2(-40);
            logo.Scale = new Vector2(0.2f);

            logo.FadeInFromZero(5000, Easing.OutQuint);
        }

        private OsuScreen loadScreen;
        private ShaderPrecompiler precompiler;

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            LoadComponentAsync(precompiler = new ShaderPrecompiler(loadIfReady), Add);
            LoadComponentAsync(loadScreen = showDisclaimer ? (OsuScreen)new Disclaimer() : new Intro(), s => loadIfReady());
        }

        private void loadIfReady()
        {
            if (ChildScreen == loadScreen) return;

            if (loadScreen.LoadState != LoadState.Ready)
                return;

            if (!precompiler.FinishedCompiling)
                return;

            Push(loadScreen);
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            base.LogoSuspending(logo);
            logo.FadeOut(100);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            showDisclaimer = game.IsDeployedBuild;
        }

        /// <summary>
        /// Compiles a set of shaders before continuing. Attempts to draw some frames between compilation by limiting to one compile per draw frame.
        /// </summary>
        public class ShaderPrecompiler : Drawable
        {
            private readonly Action onLoaded;
            private readonly List<Shader> loadTargets = new List<Shader>();

            public bool FinishedCompiling { get; private set; }

            public ShaderPrecompiler(Action onLoaded)
            {
                this.onLoaded = onLoaded;
            }

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

            protected override void Update()
            {
                base.Update();

                // if our target is null we are done.
                if (loadTargets.All(s => s.Loaded))
                {
                    FinishedCompiling = true;
                    Expire();
                    onLoaded?.Invoke();
                }
            }
        }
    }
}
