// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelRandom : MatchmakingSelectPanel
    {
        public new MatchmakingPlaylistItemRandom Item => (MatchmakingPlaylistItemRandom)base.Item;

        public MatchmakingSelectPanelRandom(MatchmakingPlaylistItemRandom item)
            : base(item)
        {
        }

        private Container? randomPanelContent;
        private BeatmapCardMatchmakingBeatmapContent? beatmapPanelContent;
        private SpriteIcon dice = null!;
        private OsuSpriteText label = null!;

        private Sample? resultSample;
        private Sample? swooshSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OverlayColourProvider colourProvider)
        {
            resultSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Selection/roulette-result");
            swooshSample = audio.Samples.Get(@"SongSelect/options-pop-out");

            Add(randomPanelContent = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Dark5,
                    },
                    new TrianglesV2
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.1f,
                    },
                    label = new OsuSpriteText
                    {
                        Y = 20,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Random"
                    },
                }
            });

            ScaleContainer.Add(dice = new SpriteIcon
            {
                Y = -10,
                Size = new Vector2(28),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = randomDiceIcon(),
            });

            dice.Spin(10_000, RotationDirection.Clockwise);
        }

        public override void PresentAsChosenBeatmap(MatchmakingPlaylistItemBeatmap item)
        {
            const double duration = 800;

            this.MoveTo(Vector2.Zero, 1000, Easing.OutExpo)
                .ScaleTo(1.5f, duration, Easing.OutExpo);

            dice.MoveToY(-200, duration * 0.55, new PowEasingFunction(2.75, easeOut: true))
                .Then()
                .Schedule(() => ScaleContainer.ChangeChildDepth(dice, float.MaxValue))
                .MoveToY(-DrawHeight / 2, duration * 0.45, new PowEasingFunction(2.2))
                .Then()
                .FadeOut()
                .Expire();

            dice.RotateTo(dice.Rotation - 360 * 5, duration * 1.3f, Easing.Out);

            label.FadeOut(200).Expire();

            swooshSample?.Play();

            Scheduler.AddDelayed(() =>
            {
                randomPanelContent?.Expire();

                ShowChosenBorder();

                ScaleContainer.ScaleTo(0.92f, 120, Easing.Out)
                              .Then()
                              .ScaleTo(1f, 600, Easing.OutElasticHalf);

                var flashLayer = new Box { RelativeSizeAxes = Axes.Both };

                AddRange(new Drawable[]
                {
                    beatmapPanelContent = new BeatmapCardMatchmakingBeatmapContent(item.Beatmap, item.Mods),
                    flashLayer,
                });

                flashLayer.FadeOut(1000).Expire();

                resultSample?.Play();
            }, duration);
        }

        protected override bool OnClick(ClickEvent e)
        {
            var icon = randomDiceIcon();

            while (icon.Equals(dice.Icon))
                icon = randomDiceIcon();

            dice.Icon = icon;
            dice.ScaleTo(0.65f, 60, Easing.Out)
                .Then()
                .Schedule(() => dice.Icon = icon)
                .ScaleTo(1f, 400, Easing.OutElasticHalf);

            return base.OnClick(e);
        }

        protected override float AvatarOverlayOffset => base.AvatarOverlayOffset + (beatmapPanelContent?.AvatarOffset ?? 0);

        private static IconUsage[] diceIcons => new[]
        {
            FontAwesome.Solid.DiceOne,
            FontAwesome.Solid.DiceTwo,
            FontAwesome.Solid.DiceThree,
            FontAwesome.Solid.DiceFour,
            FontAwesome.Solid.DiceFive,
            FontAwesome.Solid.DiceSix,
        };

        private static IconUsage randomDiceIcon() => diceIcons[RNG.Next(diceIcons.Length)];

        private readonly struct PowEasingFunction(double exponent, bool easeOut = false) : IEasingFunction
        {
            public double ApplyEasing(double time)
            {
                if (easeOut)
                    time = 1 - time;

                double value = Math.Pow(time, exponent);

                return easeOut ? 1 - value : value;
            }
        }
    }
}
