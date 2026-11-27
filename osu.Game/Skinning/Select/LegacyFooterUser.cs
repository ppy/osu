// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Rulesets;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Skinning.Select
{
    public partial class LegacyFooterUser : CompositeDrawable
    {
        private Sprite background = null!;
        private OsuSpriteText usernameText = null!;
        private OsuTextFlowContainer infoText = null!;
        private OsuSpriteText rankText = null!;
        private Sprite rulesetIcon = null!;
        private Sprite levelBar = null!;
        private UpdateableAvatar avatar = null!;
        private SkinnableSound hoverSound = null!;

        [Resolved]
        private LocalUserStatisticsProvider userStatisticsProvider { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            // reference: https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/Online/Drawable/User.cs#L622
            // with minor adjustments to position/size specifications to better match visually (besides multiplying by 1.6)
            InternalChildren = new Drawable[]
            {
                usernameText = new OsuSpriteText
                {
                    Position = new Vector2(50.5f, -2f) * 1.6f,
                    Font = OsuFont.Default.With(size: 14 * 1.6f),
                },
                infoText = new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 10 * 1.6f))
                {
                    Position = new Vector2(50f, 11.5f) * 1.6f,
                },
                rulesetIcon = new Sprite
                {
                    Position = new Vector2(176, 0) * 1.6f,
                    Colour = Color4.White.Opacity(70),
                },
                rankText = new OsuSpriteText
                {
                    Position = new Vector2(200, 9) * 1.6f,
                    Font = OsuFont.Default.With(size: 36 * 1.6f * 0.9f),
                    Origin = Anchor.TopRight,
                },
                levelBar = new Sprite
                {
                    Texture = skins.DefaultClassicSkin.GetTexture("levelbar"),
                    // fixing the texture rectangle allows us to crop the texture by the drawable width.
                    TextureRelativeSizeAxes = Axes.Y,
                    TextureRectangle = new RectangleF(0, 0, 200, 1),
                    OriginPosition = new Vector2(-120, -62),
                    Colour = new Color4(252, 184, 6, 255),
                    Alpha = 0.7f,
                },
                new Sprite
                {
                    Texture = skins.DefaultClassicSkin.GetTexture("levelbar-bg"),
                    OriginPosition = new Vector2(-120, -62),
                    Alpha = 0.4f,
                },
                background = new Sprite
                {
                    Texture = skins.DefaultClassicSkin.GetTexture("user-bg"),
                    // https://github.com/peppy/osu-stable-reference/blob/c34a74fb61c17c5667486a12548485d1f03baa2e/osu!/Online/Drawable/User.cs#L520-L521
                    Colour = new Color4(1, 1, 1, 255),
                    Blending = BlendingParameters.Additive,
                    Position = new Vector2(-4) * 1.6f,
                },
                avatar = new UpdateableAvatar(isInteractive: false, showUserPanelOnHover: false)
                {
                    Size = new Vector2(74),
                    Position = new Vector2(23.5f) * 1.6f,
                    Origin = Anchor.Centre,
                },
                hoverSound = new SkinnableSound(new SampleInfo("click-short")),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userStatisticsProvider.StatisticsUpdated += onStatisticsUpdated;
            ruleset.BindValueChanged(_ => updateDisplay(), true);
        }

        private void onStatisticsUpdated(UserStatisticsUpdate statistics)
        {
            if (ruleset.Value.Equals(statistics.Ruleset))
                updateDisplay();
        }

        private void updateDisplay()
        {
            var statistics = userStatisticsProvider.GetStatisticsFor(ruleset.Value);

            if (statistics == null)
            {
                usernameText.Text = string.Empty;
                infoText.Text = string.Empty;
                rankText.Text = string.Empty;
                rulesetIcon.Hide();
            }
            else
            {
                usernameText.Text = statistics.User.Username;
                infoText.Clear();
                infoText.AddText($"Performance:{statistics.PP:N0}pp");
                infoText.NewLine();
                infoText.AddText($"Accuracy:{statistics.DisplayAccuracy}");
                infoText.NewLine();
                infoText.AddText($"Lv{statistics.Level.Current}");

                if (!statistics.GlobalRank.HasValue)
                    rankText.Hide();
                else
                {
                    int rank = statistics.GlobalRank.Value;

                    rankText.Text = $"#{rank}";

                    if (rank > 100000)
                        rankText.Colour = new Color4(255, 255, 255, 40);
                    else if (rank > 50000)
                        rankText.Colour = new Color4(255, 255, 255, 60);
                    else if (rank > 1000)
                        rankText.Colour = new Color4(255, 255, 255, 80);
                    else if (rank > 10)
                        rankText.Colour = new Color4(255, 255, 255, 100);
                    else if (rank > 1)
                        rankText.Colour = new Color4(244, 218, 73, 120);
                    else
                        rankText.Colour = new Color4(88, 171, 248, 120);
                }

                rulesetIcon.Alpha = 70 / 255f;
                rulesetIcon.Texture = skins.DefaultClassicSkin.GetTexture($"mode-{ruleset.Value.ShortName}-small");

                if (statistics.Level.Progress == 0)
                    levelBar.Hide();
                else
                {
                    levelBar.Width = 198 * statistics.Level.Progress / 100f;
                    levelBar.Show();
                }

                avatar.User = statistics.User;
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(new Color4(41, 41, 41, 255), 200);
            hoverSound.Play();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(new Color4(1, 1, 1, 255), 200);
            base.OnHoverLost(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (userStatisticsProvider.IsNotNull())
                userStatisticsProvider.StatisticsUpdated -= onStatisticsUpdated;

            base.Dispose(isDisposing);
        }
    }
}
