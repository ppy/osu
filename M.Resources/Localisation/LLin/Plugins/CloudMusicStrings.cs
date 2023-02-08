using osu.Framework.Localisation;

namespace M.Resources.Localisation.LLin.Plugins
{
    public class CloudMusicStrings
    {
        private const string prefix = @"M.Resources.Localisation.LLin.Plugins.CloudMusicStrings";

        //设置
        public static LocalisableString LocationDirection => new TranslatableString(getKey(@"location_direction"), @"位置方向");

        public static LocalisableString PositionX => new TranslatableString(getKey(@"pos_x"), @"横向位移");

        public static LocalisableString PositionY => new TranslatableString(getKey(@"pox_y"), @"纵向位移");

        public static LocalisableString UseDrawablePool => new TranslatableString(getKey(@"use_drawable_pool"), @"使用DrawablePool");

        public static LocalisableString ExperimentalWarning => new TranslatableString(getKey(@"experimental_warning"), @"试验性功能！");

        public static LocalisableString SaveLyricOnDownloadedMain => new TranslatableString(getKey(@"save_lrc_on_downloaded_main"), @"自动保存歌词到本地");

        public static LocalisableString SaveLyricOnDownloadedSub => new TranslatableString(getKey(@"save_lrc_on_downloaded_sub"), "歌词将保存在 custom/lyrics/beatmap-<ID>.json 中");

        public static LocalisableString DisableShader => new TranslatableString(getKey(@"disable_shader"), @"禁用额外阴影");

        public static LocalisableString LocalOffset => new TranslatableString(getKey(@"global_offset_main"), @"歌词本地偏移");

        public static LocalisableString LyricFadeInDuration => new TranslatableString(getKey(@"lyric_fade_in_duration"), @"歌词淡入时间");

        public static LocalisableString LyricFadeOutDuration => new TranslatableString(getKey(@"lyric_fade_out_duration"), @"歌词淡出时间");

        public static LocalisableString LyricAutoScrollMain => new TranslatableString(getKey(@"lyric_auto_scroll_main"), @"自动滚动歌词");

        public static LocalisableString LyricAutoScrollSub => new TranslatableString(getKey(@"lyric_auto_scroll_sub"), @"使歌词界面自动滚动歌词");

        public static LocalisableString AudioControlRequest => new TranslatableString(getKey(@"audio_control_request"), @"编辑歌词需要禁用切歌功能");

        public static LocalisableString EntryTooltip => new TranslatableString(getKey(@"entry_tooltip"), @"打开歌词面板");

        public static LocalisableString Refresh => new TranslatableString(getKey(@"refresh"), @"刷新");

        public static LocalisableString RefetchLyric => new TranslatableString(getKey(@"refetch_lyric"), @"重新获取歌词");

        public static LocalisableString ScrollToCurrent => new TranslatableString(getKey(@"scroll_to_current"), @"滚动至当前歌词");

        //编辑屏幕相关
        public static LocalisableString Edit => new TranslatableString(getKey(@"edit"), @"编辑");

        public static LocalisableString Save => new TranslatableString(getKey(@"save"), @"保存");

        public static LocalisableString Reset => new TranslatableString(getKey(@"reset"), @"重置");

        public static LocalisableString Delete => new TranslatableString(getKey(@"delete"), @"删除");

        public static LocalisableString SeekToNext => new TranslatableString(getKey(@"seek_next"), @"前往下一拍");

        public static LocalisableString SeekToPrev => new TranslatableString(getKey(@"seek_prev"), @"前往上一拍");

        public static LocalisableString InsertNewLine => new TranslatableString(getKey(@"insert_new_line"), @"插入新歌词");

        public static LocalisableString LyricTime => new TranslatableString(getKey(@"lyric_time"), @"歌词时间(毫秒)");

        public static LocalisableString LyricRaw => new TranslatableString(getKey(@"raw_lyric"), @"歌词原文");

        public static LocalisableString LyricTranslated => new TranslatableString(getKey(@"lyric_translated"), @"歌词翻译");

        public static LocalisableString LyricTimeToTrack => new TranslatableString(getKey(@"lyric_time_to_track"), @"调整歌词到歌曲时间");

        public static LocalisableString TrackTimeToLyric => new TranslatableString(getKey(@"track_time_to_lyric"), @"调整歌曲到歌词时间");

        //其他
        public static LocalisableString AdjustOffsetToLyric => new TranslatableString(getKey(@"offset_adjust_to_lyric"), @"对其偏移至该歌词");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
