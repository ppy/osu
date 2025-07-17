// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Text;

namespace osu.Game.Graphics
{
    public static class OsuIcon
    {
        public const string FONT_NAME = @"Icons";

        public static IconUsage Audio => get(OsuIconMapping.Audio);
        public static IconUsage Beatmap => get(OsuIconMapping.Beatmap);
        public static IconUsage Calendar => get(OsuIconMapping.Calendar);
        public static IconUsage ChangelogA => get(OsuIconMapping.ChangelogA);
        public static IconUsage ChangelogB => get(OsuIconMapping.ChangelogB);
        public static IconUsage Chat => get(OsuIconMapping.Chat);
        public static IconUsage CheckCircle => get(OsuIconMapping.CheckCircle);
        public static IconUsage Clock => get(OsuIconMapping.Clock);
        public static IconUsage CollapseA => get(OsuIconMapping.CollapseA);
        public static IconUsage Collections => get(OsuIconMapping.Collections);
        public static IconUsage Cross => get(OsuIconMapping.Cross);
        public static IconUsage CrossCircle => get(OsuIconMapping.CrossCircle);
        public static IconUsage Crown => get(OsuIconMapping.Crown);
        public static IconUsage DailyChallenge => get(OsuIconMapping.DailyChallenge);
        public static IconUsage Debug => get(OsuIconMapping.Debug);
        public static IconUsage Delete => get(OsuIconMapping.Delete);
        public static IconUsage Details => get(OsuIconMapping.Details);
        public static IconUsage Discord => get(OsuIconMapping.Discord);
        public static IconUsage EllipsisHorizontal => get(OsuIconMapping.EllipsisHorizontal);
        public static IconUsage EllipsisVertical => get(OsuIconMapping.EllipsisVertical);
        public static IconUsage ExpandA => get(OsuIconMapping.ExpandA);
        public static IconUsage ExpandB => get(OsuIconMapping.ExpandB);
        public static IconUsage FeaturedArtist => get(OsuIconMapping.FeaturedArtist);
        public static IconUsage FeaturedArtistCircle => get(OsuIconMapping.FeaturedArtistCircle);
        public static IconUsage GameplayA => get(OsuIconMapping.GameplayA);
        public static IconUsage GameplayB => get(OsuIconMapping.GameplayB);
        public static IconUsage GameplayC => get(OsuIconMapping.GameplayC);
        public static IconUsage Global => get(OsuIconMapping.Global);
        public static IconUsage Graphics => get(OsuIconMapping.Graphics);
        public static IconUsage Heart => get(OsuIconMapping.Heart);
        public static IconUsage Home => get(OsuIconMapping.Home);
        public static IconUsage Input => get(OsuIconMapping.Input);
        public static IconUsage Maintenance => get(OsuIconMapping.Maintenance);
        public static IconUsage Megaphone => get(OsuIconMapping.Megaphone);
        public static IconUsage Metronome => get(OsuIconMapping.Metronome);
        public static IconUsage Music => get(OsuIconMapping.Music);
        public static IconUsage News => get(OsuIconMapping.News);
        public static IconUsage Next => get(OsuIconMapping.Next);
        public static IconUsage NextCircle => get(OsuIconMapping.NextCircle);
        public static IconUsage Notification => get(OsuIconMapping.Notification);
        public static IconUsage Online => get(OsuIconMapping.Online);
        public static IconUsage Play => get(OsuIconMapping.Play);
        public static IconUsage Player => get(OsuIconMapping.Player);
        public static IconUsage PlayerFollow => get(OsuIconMapping.PlayerFollow);
        public static IconUsage Prev => get(OsuIconMapping.Prev);
        public static IconUsage PrevCircle => get(OsuIconMapping.PrevCircle);
        public static IconUsage Ranking => get(OsuIconMapping.Ranking);
        public static IconUsage Rulesets => get(OsuIconMapping.Rulesets);
        public static IconUsage Search => get(OsuIconMapping.Search);
        public static IconUsage Settings => get(OsuIconMapping.Settings);
        public static IconUsage SkinA => get(OsuIconMapping.SkinA);
        public static IconUsage SkinB => get(OsuIconMapping.SkinB);
        public static IconUsage Star => get(OsuIconMapping.Star);
        public static IconUsage Storyboard => get(OsuIconMapping.Storyboard);
        public static IconUsage Team => get(OsuIconMapping.Team);
        public static IconUsage ThumbsUp => get(OsuIconMapping.ThumbsUp);
        public static IconUsage Tournament => get(OsuIconMapping.Tournament);
        public static IconUsage Twitter => get(OsuIconMapping.Twitter);
        public static IconUsage UserInterface => get(OsuIconMapping.UserInterface);
        public static IconUsage Wiki => get(OsuIconMapping.Wiki);
        public static IconUsage EditorAddControlPoint => get(OsuIconMapping.EditorAddControlPoint);
        public static IconUsage EditorConvertToStream => get(OsuIconMapping.EditorConvertToStream);
        public static IconUsage EditorDistanceSnap => get(OsuIconMapping.EditorDistanceSnap);
        public static IconUsage EditorFinish => get(OsuIconMapping.EditorFinish);
        public static IconUsage EditorGridSnap => get(OsuIconMapping.EditorGridSnap);
        public static IconUsage EditorNewComboA => get(OsuIconMapping.EditorNewComboA);
        public static IconUsage EditorNewComboB => get(OsuIconMapping.EditorNewComboB);
        public static IconUsage EditorSelect => get(OsuIconMapping.EditorSelect);
        public static IconUsage EditorSound => get(OsuIconMapping.EditorSound);
        public static IconUsage EditorWhistle => get(OsuIconMapping.EditorWhistle);
        public static IconUsage Tortoise => get(OsuIconMapping.Tortoise);
        public static IconUsage Hare => get(OsuIconMapping.Hare);

        // ruleset icons in circles
        public static IconUsage RulesetOsu => get(OsuIconMapping.RulesetOsu);
        public static IconUsage RulesetMania => get(OsuIconMapping.RulesetMania);
        public static IconUsage RulesetCatch => get(OsuIconMapping.RulesetCatch);
        public static IconUsage RulesetTaiko => get(OsuIconMapping.RulesetTaiko);

        // ruleset icons without circles
        public static IconUsage FilledCircle => get(OsuIconMapping.FilledCircle);
        public static IconUsage Logo => get(OsuIconMapping.Logo);
        public static IconUsage ChevronDownCircle => get(OsuIconMapping.ChevronDownCircle);
        public static IconUsage EditCircle => get(OsuIconMapping.EditCircle);
        public static IconUsage LeftCircle => get(OsuIconMapping.LeftCircle);
        public static IconUsage RightCircle => get(OsuIconMapping.RightCircle);
        public static IconUsage Charts => get(OsuIconMapping.Charts);
        public static IconUsage Solo => get(OsuIconMapping.Solo);
        public static IconUsage Multi => get(OsuIconMapping.Multi);
        public static IconUsage Gear => get(OsuIconMapping.Gear);

        // misc icons
        public static IconUsage Bat => get(OsuIconMapping.Bat);
        public static IconUsage Bubble => get(OsuIconMapping.Bubble);
        public static IconUsage BubblePop => get(OsuIconMapping.BubblePop);
        public static IconUsage Dice => get(OsuIconMapping.Dice);
        public static IconUsage HeartBreak => get(OsuIconMapping.HeartBreak);
        public static IconUsage Hot => get(OsuIconMapping.Hot);
        public static IconUsage ListSearch => get(OsuIconMapping.ListSearch);

        //osu! playstyles
        public static IconUsage PlayStyleTablet => get(OsuIconMapping.PlayStyleTablet);
        public static IconUsage PlayStyleMouse => get(OsuIconMapping.PlayStyleMouse);
        public static IconUsage PlayStyleKeyboard => get(OsuIconMapping.PlayStyleKeyboard);
        public static IconUsage PlayStyleTouch => get(OsuIconMapping.PlayStyleTouch);

        // mod icons
        public static IconUsage ModKey1 => get(OsuIconMapping.ModKey1);
        public static IconUsage ModKey2 => get(OsuIconMapping.ModKey2);
        public static IconUsage ModKey3 => get(OsuIconMapping.ModKey3);
        public static IconUsage ModKey4 => get(OsuIconMapping.ModKey4);
        public static IconUsage ModKey5 => get(OsuIconMapping.ModKey5);
        public static IconUsage ModKey6 => get(OsuIconMapping.ModKey6);
        public static IconUsage ModKey7 => get(OsuIconMapping.ModKey7);
        public static IconUsage ModKey8 => get(OsuIconMapping.ModKey8);
        public static IconUsage ModKey9 => get(OsuIconMapping.ModKey9);
        public static IconUsage ModAuto => get(OsuIconMapping.ModAuto);
        public static IconUsage ModAutopilot => get(OsuIconMapping.ModAutopilot);
        public static IconUsage ModCinema => get(OsuIconMapping.ModCinema);
        public static IconUsage ModDoubleTime => get(OsuIconMapping.ModDoubleTime);
        public static IconUsage ModEasy => get(OsuIconMapping.ModEasy);
        public static IconUsage ModFadeIn => get(OsuIconMapping.ModFadeIn);
        public static IconUsage ModFlashlight => get(OsuIconMapping.ModFlashlight);
        public static IconUsage ModHalftime => get(OsuIconMapping.ModHalftime);
        public static IconUsage ModHardRock => get(OsuIconMapping.ModHardRock);
        public static IconUsage ModHidden => get(OsuIconMapping.ModHidden);
        public static IconUsage ModMirror => get(OsuIconMapping.ModMirror);
        public static IconUsage ModNightcore => get(OsuIconMapping.ModNightcore);
        public static IconUsage ModNoFail => get(OsuIconMapping.ModNoFail);
        public static IconUsage ModNoMod => get(OsuIconMapping.ModNoMod);
        public static IconUsage ModPerfect => get(OsuIconMapping.ModPerfect);
        public static IconUsage ModRandom => get(OsuIconMapping.ModRandom);
        public static IconUsage ModRelax => get(OsuIconMapping.ModRelax);
        public static IconUsage ModSpunOut => get(OsuIconMapping.ModSpunOut);
        public static IconUsage ModSuddenDeath => get(OsuIconMapping.ModSuddenDeath);
        public static IconUsage ModTarget => get(OsuIconMapping.ModTarget);
        public static IconUsage ModTouchDevice => get(OsuIconMapping.ModTouchDevice);

        private static IconUsage get(OsuIconMapping glyph) => new IconUsage((char)glyph, FONT_NAME);

        private enum OsuIconMapping
        {
            [Description(@"audio")]
            Audio,

            [Description(@"beatmap")]
            Beatmap,

            [Description(@"calendar")]
            Calendar,

            [Description(@"changelog-a")]
            ChangelogA,

            [Description(@"changelog-b")]
            ChangelogB,

            [Description(@"chat")]
            Chat,

            [Description(@"check-circle")]
            CheckCircle,

            [Description(@"clock")]
            Clock,

            [Description(@"collapse-a")]
            CollapseA,

            [Description(@"collections")]
            Collections,

            [Description(@"cross")]
            Cross,

            [Description(@"cross-circle")]
            CrossCircle,

            [Description(@"crown")]
            Crown,

            [Description(@"daily-challenge")]
            DailyChallenge,

            [Description(@"debug")]
            Debug,

            [Description(@"delete")]
            Delete,

            [Description(@"details")]
            Details,

            [Description(@"discord")]
            Discord,

            [Description(@"ellipsis-horizontal")]
            EllipsisHorizontal,

            [Description(@"ellipsis-vertical")]
            EllipsisVertical,

            [Description(@"expand-a")]
            ExpandA,

            [Description(@"expand-b")]
            ExpandB,

            [Description(@"featured-artist")]
            FeaturedArtist,

            [Description(@"featured-artist-circle")]
            FeaturedArtistCircle,

            [Description(@"gameplay-a")]
            GameplayA,

            [Description(@"gameplay-b")]
            GameplayB,

            [Description(@"gameplay-c")]
            GameplayC,

            [Description(@"global")]
            Global,

            [Description(@"graphics")]
            Graphics,

            [Description(@"heart")]
            Heart,

            [Description(@"home")]
            Home,

            [Description(@"input")]
            Input,

            [Description(@"maintenance")]
            Maintenance,

            [Description(@"megaphone")]
            Megaphone,

            [Description(@"metronome")]
            Metronome,

            [Description(@"music")]
            Music,

            [Description(@"news")]
            News,

            [Description(@"next")]
            Next,

            [Description(@"next-circle")]
            NextCircle,

            [Description(@"notification")]
            Notification,

            [Description(@"online")]
            Online,

            [Description(@"play")]
            Play,

            [Description(@"player")]
            Player,

            [Description(@"player-follow")]
            PlayerFollow,

            [Description(@"prev")]
            Prev,

            [Description(@"prev-circle")]
            PrevCircle,

            [Description(@"ranking")]
            Ranking,

            [Description(@"rulesets")]
            Rulesets,

            [Description(@"search")]
            Search,

            [Description(@"settings")]
            Settings,

            [Description(@"skin-a")]
            SkinA,

            [Description(@"skin-b")]
            SkinB,

            [Description(@"star")]
            Star,

            [Description(@"storyboard")]
            Storyboard,

            [Description(@"team")]
            Team,

            [Description(@"thumbs-up")]
            ThumbsUp,

            [Description(@"tournament")]
            Tournament,

            [Description(@"twitter")]
            Twitter,

            [Description(@"user-interface")]
            UserInterface,

            [Description(@"wiki")]
            Wiki,

            [Description(@"Editor/add-control-point")]
            EditorAddControlPoint = 1000,

            [Description(@"Editor/convert-to-stream")]
            EditorConvertToStream,

            [Description(@"Editor/distance-snap")]
            EditorDistanceSnap,

            [Description(@"Editor/finish")]
            EditorFinish,

            [Description(@"Editor/grid-snap")]
            EditorGridSnap,

            [Description(@"Editor/new-combo-a")]
            EditorNewComboA,

            [Description(@"Editor/new-combo-b")]
            EditorNewComboB,

            [Description(@"Editor/select")]
            EditorSelect,

            [Description(@"Editor/sound")]
            EditorSound,

            [Description(@"Editor/whistle")]
            EditorWhistle,

            [Description(@"tortoise")]
            Tortoise,

            [Description(@"hare")]
            Hare,

            [Description(@"OsuFontIcons/ruleset-osu")]
            RulesetOsu,

            [Description(@"OsuFontIcons/ruleset-mania")]
            RulesetMania,

            [Description(@"OsuFontIcons/ruleset-catch")]
            RulesetCatch,

            [Description(@"OsuFontIcons/ruleset-taiko")]
            RulesetTaiko,

            [Description(@"OsuFontIcons/filled-circle")]
            FilledCircle,

            [Description(@"OsuFontIcons/logo")]
            Logo,

            [Description(@"OsuFontIcons/chevron-down-circle")]
            ChevronDownCircle,

            [Description(@"OsuFontIcons/edit-circle")]
            EditCircle,

            [Description(@"OsuFontIcons/left-circle")]
            LeftCircle,

            [Description(@"OsuFontIcons/right-circle")]
            RightCircle,

            [Description(@"OsuFontIcons/charts")]
            Charts,

            [Description(@"OsuFontIcons/solo")]
            Solo,

            [Description(@"OsuFontIcons/multi")]
            Multi,

            [Description(@"OsuFontIcons/gear")]
            Gear,

            [Description(@"OsuFontIcons/bat")]
            Bat,

            [Description(@"OsuFontIcons/bubble")]
            Bubble,

            [Description(@"OsuFontIcons/bubble-pop")]
            BubblePop,

            [Description(@"OsuFontIcons/dice")]
            Dice,

            [Description(@"OsuFontIcons/heart-break")]
            HeartBreak,

            [Description(@"OsuFontIcons/hot")]
            Hot,

            [Description(@"OsuFontIcons/list-search")]
            ListSearch,

            [Description(@"OsuFontIcons/play-style-tablet")]
            PlayStyleTablet,

            [Description(@"OsuFontIcons/play-style-mouse")]
            PlayStyleMouse,

            [Description(@"OsuFontIcons/play-style-keyboard")]
            PlayStyleKeyboard,

            [Description(@"OsuFontIcons/play-style-touch")]
            PlayStyleTouch,

            [Description(@"ModIcons/mod-key1")]
            ModKey1,

            [Description(@"ModIcons/mod-key2")]
            ModKey2,

            [Description(@"ModIcons/mod-key3")]
            ModKey3,

            [Description(@"ModIcons/mod-key4")]
            ModKey4,

            [Description(@"ModIcons/mod-key5")]
            ModKey5,

            [Description(@"ModIcons/mod-key6")]
            ModKey6,

            [Description(@"ModIcons/mod-key7")]
            ModKey7,

            [Description(@"ModIcons/mod-key8")]
            ModKey8,

            [Description(@"ModIcons/mod-key9")]
            ModKey9,

            [Description(@"ModIcons/mod-auto")]
            ModAuto,

            [Description(@"ModIcons/mod-autopilot")]
            ModAutopilot,

            [Description(@"ModIcons/mod-cinema")]
            ModCinema,

            [Description(@"ModIcons/mod-double-time")]
            ModDoubleTime,

            [Description(@"ModIcons/mod-easy")]
            ModEasy,

            [Description(@"ModIcons/mod-fade-in")]
            ModFadeIn,

            [Description(@"ModIcons/mod-flashlight")]
            ModFlashlight,

            [Description(@"ModIcons/mod-halftime")]
            ModHalftime,

            [Description(@"ModIcons/mod-hard-rock")]
            ModHardRock,

            [Description(@"ModIcons/mod-hidden")]
            ModHidden,

            [Description(@"ModIcons/mod-mirror")]
            ModMirror,

            [Description(@"ModIcons/mod-nightcore")]
            ModNightcore,

            [Description(@"ModIcons/mod-no-fail")]
            ModNoFail,

            [Description(@"ModIcons/mod-no-mod")]
            ModNoMod,

            [Description(@"ModIcons/mod-perfect")]
            ModPerfect,

            [Description(@"ModIcons/mod-random")]
            ModRandom,

            [Description(@"ModIcons/mod-relax")]
            ModRelax,

            [Description(@"ModIcons/mod-spun-out")]
            ModSpunOut,

            [Description(@"ModIcons/mod-sudden-death")]
            ModSuddenDeath,

            [Description(@"ModIcons/mod-target")]
            ModTarget,

            [Description(@"ModIcons/mod-touch-device")]
            ModTouchDevice,
        }

        public class OsuIconStore : ITextureStore, ITexturedGlyphLookupStore
        {
            private readonly TextureStore textures;

            public OsuIconStore(TextureStore textures)
            {
                this.textures = textures;
            }

            public ITexturedCharacterGlyph? Get(string? fontName, char character)
            {
                if (fontName == FONT_NAME)
                    return new Glyph(textures.Get($@"{fontName}/{((OsuIconMapping)character).GetDescription()}"));

                return null;
            }

            public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character) => Task.Run(() => Get(fontName, character));

            public Texture? Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => null;

            public Texture Get(string name) => throw new NotImplementedException();

            public Task<Texture> GetAsync(string name, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public Stream GetStream(string name) => throw new NotImplementedException();

            public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

            public Task<Texture?> GetAsync(string name, WrapMode wrapModeS, WrapMode wrapModeT, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public class Glyph : ITexturedCharacterGlyph
            {
                public float XOffset => default;
                public float YOffset => default;
                public float XAdvance => default;
                public float Baseline => default;
                public char Character => default;

                public float GetKerning<T>(T lastGlyph) where T : ICharacterGlyph => throw new NotImplementedException();

                public Texture Texture { get; }
                public float Width => Texture.Width;
                public float Height => Texture.Height;

                public Glyph(Texture texture)
                {
                    Texture = texture;
                }
            }

            public void Dispose()
            {
                textures.Dispose();
            }
        }
    }
}
