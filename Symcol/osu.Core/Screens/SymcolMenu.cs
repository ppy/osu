using osu.Core.Containers.Shawdooow;
using osu.Core.OsuMods;
using osu.Core.Screens.Evast;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Core.Screens
{
    public class SymcolMenu : BeatmapScreen
    {
        private const int animation_duration = 600;

        protected override float BackgroundBlur => 10;

        public static OsuScreen LegacyRulesetMultiplayerScreen;
        public static OsuScreen Lobby;

        private readonly OsuLogo logo;
        private readonly Container<SymcolButton> buttonsContainer;

        public SymcolMenu()
        {
            Children = new Drawable[]
            {
                new MenuSideFlashes(),
                buttonsContainer = new Container<SymcolButton>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        /*
                        new SymcolButton
                        {
                            ButtonName = "Legacy MP",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = OsuColour.FromHex("#d8d8d8"),
                            ButtonColorBottom = OsuColour.FromHex("#3a3a3a"),
                            ButtonSize = 60,
                            Action = delegate { Push(LegacyRulesetMultiplayerScreen); },
                            ButtonPosition = new Vector2(280 , -180),
                        },
                        new SymcolButton
                        {
                            ButtonName = "Multi",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.LimeGreen,
                            ButtonColorBottom = Color4.Yellow,
                            ButtonSize = 120,
                            Action = delegate { Push(Lobby); },
                            ButtonPosition = new Vector2(180 , -100),
                        },
                        */
                        new SymcolButton
                        {
                            ButtonName = "Back",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.DarkRed,
                            ButtonColorBottom = Color4.Red,
                            ButtonSize = 80,
                            Action = Exit,
                            ButtonPosition = new Vector2(-350 , 300),
                        }
                    }
                },
                logo = new OsuLogo
                {
                    Scale = new Vector2(1.25f),
                    Action = () => open(logo),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };

            foreach (OsuModSet set in OsuModStore.LoadedModSets)
                if (set.GetMenuButton() != null && set.GetScreen() != null)
                {
                    SymcolButton s = set.GetMenuButton();
                    buttonsContainer.Add(s);
                    s.Action = () => Push(set.GetScreen());
                }
        }

        private bool open(Container container)
        {
            logo.Action = () => close(container);
            container.ScaleTo(new Vector2(0.5f), animation_duration, Easing.InOutBack);

            foreach(var button in buttonsContainer)
                button.MoveTo(button.ButtonPosition, animation_duration, Easing.InOutBack);
            return true;
        }

        private bool close(Container container)
        {
            logo.Action = () => open(container);
            container.ScaleTo(new Vector2(1.25f), animation_duration, Easing.InOutBack);

            foreach (var button in buttonsContainer)
                button.MoveTo(Vector2.Zero, animation_duration, Easing.InOutBack);
            return true;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Content.FadeInFromZero(250);
        }
        
        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            Content.FadeIn(250);
            Content.ScaleTo(1, 250, Easing.OutSine);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            Content.ScaleTo(1.1f, 250, Easing.InSine);
            Content.FadeOut(250);
        }

        protected override bool OnExiting(Screen next)
        {
            Content.FadeOut(100);
            return base.OnExiting(next);
        }
    }
}
