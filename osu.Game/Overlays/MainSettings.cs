// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Screens.Ranking;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class MainSettings : SettingsOverlay
    {
        private readonly KeyBindingOverlay keyBindingOverlay;
        private BackButton backButton;

        protected override IEnumerable<SettingsSection> CreateSections() => new SettingsSection[]
        {
            new GeneralSection(),
            new GraphicsSection(),
            new GameplaySection(),
            new AudioSection(),
            new SkinSection(),
            new InputSection(keyBindingOverlay),
            new OnlineSection(),
            new MaintenanceSection(),
            new DebugSection(),
        };

        protected override Drawable CreateHeader() => new SettingsHeader("settings", "Change the way osu! behaves");
        protected override Drawable CreateFooter() => new SettingsFooter();

        public MainSettings()
            : base(true)
        {
            keyBindingOverlay = new KeyBindingOverlay
            {
                Depth = 1,
                Anchor = Anchor.TopRight,
            };
            keyBindingOverlay.StateChanged += keyBindingOverlay_StateChanged;
        }

        public override bool AcceptsFocus => keyBindingOverlay.State != Visibility.Visible;

        private const float hidden_width = 120;

        private void keyBindingOverlay_StateChanged(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Visible:
                    Background.FadeTo(0.9f, 300, Easing.OutQuint);
                    Sidebar?.FadeColour(Color4.DarkGray, 300, Easing.OutQuint);

                    SectionsContainer.FadeOut(300, Easing.OutQuint);
                    ContentContainer.MoveToX(hidden_width - WIDTH, 500, Easing.OutQuint);

                    backButton.Delay(100).FadeIn(100);
                    break;
                case Visibility.Hidden:
                    Background.FadeTo(0.6f, 500, Easing.OutQuint);
                    Sidebar?.FadeColour(Color4.White, 300, Easing.OutQuint);

                    SectionsContainer.FadeIn(500, Easing.OutQuint);
                    ContentContainer.MoveToX(0, 500, Easing.OutQuint);

                    backButton.FadeOut(100);
                    break;
            }
        }

        protected override float ExpandedPosition => keyBindingOverlay.State == Visibility.Visible ? hidden_width - WIDTH : base.ExpandedPosition;

        [BackgroundDependencyLoader]
        private void load()
        {
            ContentContainer.Add(keyBindingOverlay);

            ContentContainer.Add(backButton = new BackButton
            {
                Alpha = 0,
                Width = hidden_width,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Action = () => keyBindingOverlay.Hide()
            });
        }

        private class BackButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
        {
            private AspectContainer aspect;

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    aspect = new AspectContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Y = -15,
                                Size = new Vector2(15),
                                Shadow = true,
                                Icon = FontAwesome.fa_chevron_left
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Y = 15,
                                TextSize = 12,
                                Font = @"Exo2.0-Bold",
                                Text = @"back",
                            },
                        }
                    }
                };
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                aspect.ScaleTo(0.75f, 2000, Easing.OutQuint);
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                aspect.ScaleTo(1, 1000, Easing.OutElastic);
                return base.OnMouseUp(state, args);
            }

            public bool OnPressed(GlobalAction action)
            {
                switch (action)
                {
                    case GlobalAction.Back:
                        Click();
                        return true;
                }

                return false;
            }

            public bool OnReleased(GlobalAction action) => false;
        }
    }
}
