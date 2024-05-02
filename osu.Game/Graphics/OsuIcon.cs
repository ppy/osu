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
        #region Legacy spritesheet-based icons

        private static IconUsage get(int icon) => new IconUsage((char)icon, @"osuFont");

        // ruleset icons in circles
        public static IconUsage RulesetOsu => get(0xe000);
        public static IconUsage RulesetMania => get(0xe001);
        public static IconUsage RulesetCatch => get(0xe002);
        public static IconUsage RulesetTaiko => get(0xe003);

        // ruleset icons without circles
        public static IconUsage FilledCircle => get(0xe004);
        public static IconUsage Logo => get(0xe006);
        public static IconUsage ChevronDownCircle => get(0xe007);
        public static IconUsage EditCircle => get(0xe033);
        public static IconUsage LeftCircle => get(0xe034);
        public static IconUsage RightCircle => get(0xe035);
        public static IconUsage Charts => get(0xe036);
        public static IconUsage Solo => get(0xe037);
        public static IconUsage Multi => get(0xe038);
        public static IconUsage Gear => get(0xe039);

        // misc icons
        public static IconUsage Bat => get(0xe008);
        public static IconUsage Bubble => get(0xe009);
        public static IconUsage BubblePop => get(0xe02e);
        public static IconUsage Dice => get(0xe011);
        public static IconUsage HeartBreak => get(0xe030);
        public static IconUsage Hot => get(0xe031);
        public static IconUsage ListSearch => get(0xe032);

        //osu! playstyles
        public static IconUsage PlayStyleTablet => get(0xe02a);
        public static IconUsage PlayStyleMouse => get(0xe029);
        public static IconUsage PlayStyleKeyboard => get(0xe02b);
        public static IconUsage PlayStyleTouch => get(0xe02c);

        // osu! difficulties
        public static IconUsage EasyOsu => get(0xe015);
        public static IconUsage NormalOsu => get(0xe016);
        public static IconUsage HardOsu => get(0xe017);
        public static IconUsage InsaneOsu => get(0xe018);
        public static IconUsage ExpertOsu => get(0xe019);

        // taiko difficulties
        public static IconUsage EasyTaiko => get(0xe01a);
        public static IconUsage NormalTaiko => get(0xe01b);
        public static IconUsage HardTaiko => get(0xe01c);
        public static IconUsage InsaneTaiko => get(0xe01d);
        public static IconUsage ExpertTaiko => get(0xe01e);

        // fruits difficulties
        public static IconUsage EasyFruits => get(0xe01f);
        public static IconUsage NormalFruits => get(0xe020);
        public static IconUsage HardFruits => get(0xe021);
        public static IconUsage InsaneFruits => get(0xe022);
        public static IconUsage ExpertFruits => get(0xe023);

        // mania difficulties
        public static IconUsage EasyMania => get(0xe024);
        public static IconUsage NormalMania => get(0xe025);
        public static IconUsage HardMania => get(0xe026);
        public static IconUsage InsaneMania => get(0xe027);
        public static IconUsage ExpertMania => get(0xe028);

        // mod icons
        public static IconUsage ModPerfect => get(0xe049);
        public static IconUsage ModAutopilot => get(0xe03a);
        public static IconUsage ModAuto => get(0xe03b);
        public static IconUsage ModCinema => get(0xe03c);
        public static IconUsage ModDoubleTime => get(0xe03d);
        public static IconUsage ModEasy => get(0xe03e);
        public static IconUsage ModFlashlight => get(0xe03f);
        public static IconUsage ModHalftime => get(0xe040);
        public static IconUsage ModHardRock => get(0xe041);
        public static IconUsage ModHidden => get(0xe042);
        public static IconUsage ModNightcore => get(0xe043);
        public static IconUsage ModNoFail => get(0xe044);
        public static IconUsage ModRelax => get(0xe045);
        public static IconUsage ModSpunOut => get(0xe046);
        public static IconUsage ModSuddenDeath => get(0xe047);
        public static IconUsage ModTarget => get(0xe048);

        // Use "Icons/BeatmapDetails/mod-icon" instead
        // public static IconUsage ModBg => Get(0xe04a);

        #endregion

        #region New single-file-based icons

        public const string FONT_NAME = @"Icons";

        public static IconUsage Audio => get(OsuIconMapping.Audio);
        public static IconUsage Beatmap => get(OsuIconMapping.Beatmap);
        public static IconUsage Calendar => get(OsuIconMapping.Calendar);
        public static IconUsage ChangelogA => get(OsuIconMapping.ChangelogA);
        public static IconUsage ChangelogB => get(OsuIconMapping.ChangelogB);
        public static IconUsage Chat => get(OsuIconMapping.Chat);
        public static IconUsage CheckCircle => get(OsuIconMapping.CheckCircle);
        public static IconUsage CollapseA => get(OsuIconMapping.CollapseA);
        public static IconUsage Collections => get(OsuIconMapping.Collections);
        public static IconUsage Cross => get(OsuIconMapping.Cross);
        public static IconUsage CrossCircle => get(OsuIconMapping.CrossCircle);
        public static IconUsage Crown => get(OsuIconMapping.Crown);
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

        #endregion
    }
}
