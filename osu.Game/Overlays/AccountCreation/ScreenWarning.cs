// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Overlays.AccountCreation
{
    public partial class ScreenWarning : AccountCreationScreen
    {
        private OsuTextFlowContainer multiAccountExplanationText = null!;
        private LinkFlowContainer furtherAssistance = null!;

        [Resolved]
        private IAPIProvider? api { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        private const string help_centre_url = "/help/wiki/Help_Centre#login";

        public override void OnEntering(ScreenTransitionEvent e)
        {
            if (string.IsNullOrEmpty(api?.ProvidedUsername) || game?.UseDevelopmentServer == true)
            {
                this.FadeOut();
                this.Push(new ScreenEntry());
                return;
            }

            base.OnEntering(e);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, TextureStore textures)
        {
            if (string.IsNullOrEmpty(api?.ProvidedUsername))
                return;

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Texture = textures.Get(@"Menu/dev-build-footer"),
                },
                new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Texture = textures.Get(@"Menu/dev-build-footer"),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 150,
                            Child = new OsuLogo
                            {
                                Scale = new Vector2(0.1f),
                                Anchor = Anchor.Centre,
                                Triangles = false,
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = Color4.Red,
                            Font = OsuFont.GetFont(size: 28, weight: FontWeight.Light),
                            Text = "Warning! 注意！",
                        },
                        multiAccountExplanationText = new OsuTextFlowContainer(cp => cp.Font = cp.Font.With(size: 12))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                        new SettingsButton
                        {
                            Text = AccountCreationStrings.MultiAccountWarningHelp,
                            Margin = new MarginPadding { Top = 50 },
                            Action = () => game?.OpenUrlExternally(help_centre_url)
                        },
                        new DangerousSettingsButton
                        {
                            Text = AccountCreationStrings.MultiAccountWarningAccept,
                            Action = () => this.Push(new ScreenEntry())
                        },
                        furtherAssistance = new LinkFlowContainer(cp => cp.Font = cp.Font.With(size: 12))
                        {
                            Margin = new MarginPadding { Top = 20 },
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            AutoSizeAxes = Axes.Both
                        },
                    }
                }
            };

            multiAccountExplanationText.AddText("Are you ");
            multiAccountExplanationText.AddText(api.ProvidedUsername, cp => cp.Colour = colours.BlueLight);
            multiAccountExplanationText.AddText("? osu! has a policy of ");
            multiAccountExplanationText.AddText("one account per person!", cp => cp.Colour = colours.Yellow);
            multiAccountExplanationText.AddText(" Please be aware that creating more than one account per person may result in ");
            multiAccountExplanationText.AddText("permanent deactivation of accounts", cp => cp.Colour = colours.Yellow);
            multiAccountExplanationText.AddText(".");

            furtherAssistance.AddText("Need further assistance? Contact us via our ");
            furtherAssistance.AddLink("support system", help_centre_url);
            furtherAssistance.AddText(".");
        }
    }
}
