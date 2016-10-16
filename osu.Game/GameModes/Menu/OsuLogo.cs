//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework;
using OpenTK;

namespace osu.Game.GameModes.Menu
{
    /// <summary>
    /// osu! logo and its attachments (pulsing, visualiser etc.)
    /// </summary>
    public partial class OsuLogo : AutoSizeContainer
    {
        private Sprite logo;
        private CircularContainer logoContainer;
        private Container logoBounceContainer;
        private MenuVisualisation vis;

        public Action Action;

        public float SizeForFlow => logo == null ? 0 : logo.Size.X * logo.Scale.X * logoBounceContainer.Scale.X * 0.8f;

        private Sprite ripple;

        private Container rippleContainer;

        public override bool Contains(Vector2 screenSpacePos)
        {
            return logoContainer.Contains(screenSpacePos);
        }

        public bool Ripple
        {
            get { return rippleContainer.Alpha > 0; }
            set
            {
                rippleContainer.Alpha = value ? 1 : 0;
            }
        }

        public bool Interactive = true;

        public OsuLogo()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                logoBounceContainer = new AutoSizeContainer
                {
                    Children = new Drawable[]
                    {
                        logoContainer = new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Children = new[]
                            {
                                logo = new Sprite
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                            },
                        },
                        rippleContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                ripple = new Sprite()
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Additive = true,
                                    Alpha = 0.05f
                                }
                            }
                        },
                        vis = new MenuVisualisation
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = logo.Size,
                            Additive = true,
                            Alpha = 0.2f,
                        }
                    }
                }
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            logo.Texture = game.Textures.Get(@"Menu/logo");
            ripple.Texture = game.Textures.Get(@"Menu/logo");

            ripple.ScaleTo(1.1f, 500);
            ripple.FadeOut(500);
            ripple.Loop(300);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (!Interactive) return false;

            logoBounceContainer.ScaleTo(1.1f, 1000, EasingTypes.Out);
            return true;
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {

            logoBounceContainer.ScaleTo(1.2f, 500, EasingTypes.OutElastic);
            return true;
        }

        protected override bool OnClick(InputState state)
        {
            if (!Interactive) return false;

            Action?.Invoke();
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            if (!Interactive) return false;
            logoBounceContainer.ScaleTo(1.2f, 500, EasingTypes.OutElastic);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            logoBounceContainer.ScaleTo(1, 500, EasingTypes.OutElastic);
        }
    }
}