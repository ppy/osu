// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    [Cached]
    public class FirstRunSetupOverlay : OsuFocusedOverlayContainer
    {
        protected override bool StartHidden => true;

        [Resolved]
        private DialogOverlay dialogOverlay { get; set; }

        [Resolved]
        private OsuGame osuGame { get; set; }

        private ScreenWelcome welcomeScreen;

        private Container currentDisplayContainer;

        private PlayfieldBorder border;

        public FirstRunSetupOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuColour colours)
        {
            Children = new Drawable[]
            {
                border = new PlayfieldBorder
                {
                    PlayfieldBorderStyle = { Value = PlayfieldBorderStyle.Full },
                    Colour = colours.Blue,
                },
                currentDisplayContainer = new Container
                {
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f),
                    Position = new Vector2(0.5f),
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.8f,
                        },
                        new ScreenStack(welcomeScreen = new ScreenWelcome())
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // if we are valid for display, only do so after reaching the main menu.
            osuGame.PerformFromScreen(_ =>
            {
                Show();
            }, new[] { typeof(MainMenu) });

            border
                .FadeInFromZero(500)
                .Delay(1000)
                .FadeOut(500)
                .Loop();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (dialogOverlay.CurrentDialog == null)
            {
                dialogOverlay.Push(new ConfirmDialog("Are you sure you want to exit the setup process?",
                    Hide,
                    () => { }));
            }

            return base.OnClick(e);
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeIn(400, Easing.OutQuint);

            if (welcomeScreen.GetChildScreen() != null)
                welcomeScreen.MakeCurrent();
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(100);
        }

        public void MoveDisplayTo(Vector2 position) =>
            currentDisplayContainer.MoveTo(position, 1000, Easing.OutElasticQuarter);

        public void ResizeDisplayTo(Vector2 scale) =>
            currentDisplayContainer.ScaleTo(scale, 1000, Easing.OutElasticQuarter);
    }
}
