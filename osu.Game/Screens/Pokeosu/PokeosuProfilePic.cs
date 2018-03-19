using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Online.API;
using osu.Game.Users;
using System;

namespace osu.Game.Screens.Pokeosu
{
    public class PokeosuProfilePic : Container, IOnlineComponent
    {
        private Container dragContainer;

        private FillFlowContainer user;
        private UpdateableAvatar avatar;

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            Vector2 change = state.Mouse.Position - state.Mouse.PositionMouseDown.Value;

            // Diminish the drag distance as we go further to simulate "rubber band" feeling.
            change *= change.Length <= 0 ? 0 : (float)Math.Pow(change.Length, 0.7f) / change.Length;

            dragContainer.MoveTo(change);
            return base.OnDrag(state);
        }

        protected override bool OnDragEnd(InputState state)
        {
            dragContainer.MoveTo(Vector2.Zero, 800, Easing.OutElastic);
            return base.OnDragEnd(state);
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            api.Register(this);

            Children = new Drawable[]
            {
                dragContainer = new Container
                {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(140),
                            Children = new Drawable[]
                            {
                               user = new FillFlowContainer
                               {
                                   Anchor = Anchor.Centre,
                                   Origin = Anchor.Centre,
                                   AutoSizeAxes = Axes.X,
                               },
                           }
                      }
            };
            user.Add(avatar = new UpdateableAvatar
            {
                Masking = true,
                Size = new Vector2(140),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                CornerRadius = (16),
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 8,
                    Colour = Color4.Black.Opacity(0.1f),
                }
            });
        }
        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                default:
                    avatar.User = new User();
                    break;
                case APIState.Online:
                    avatar.User = api.LocalUser;
                    break;
            }
        }
    }
}
