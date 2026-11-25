using System.Windows.Media;

namespace MediaCarousel.Models
{
    public enum MediaType
    {
        Image,
        Video,
    }

    public class MediaItem
    {
        /// <summary>
        /// 明确指定媒体类型
        /// </summary>
        public MediaType MediaType { get; set; }

        /// <summary>
        /// 资源路径 (String or Uri)
        /// </summary>
        public Uri? Source { get; set; }

        /// <summary>
        /// 图片展示时长（默认 3 秒，视频则忽略此值使用视频长度）
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// 视频是否循环播放（若为 True，轮播会在该视频处无限循环，除非手动切换）
        /// </summary>
        public bool IsLoop { get; set; } = false;

        /// <summary>
        /// 填充模式
        /// </summary>
        public Stretch Stretch { get; set; } = Stretch.Uniform;

        /// <summary>
        /// 视频占位图（可选，覆盖全局设置）
        /// </summary>
        public ImageSource? PlaceholderSource { get; set; }

        public string? Tag { get; set; }
        public string? TagPlaceholder { get; set; }

        public MediaItem() { }

        public MediaItem(
            MediaType mediaType,
            Uri? source,
            TimeSpan duration,
            bool isLoop,
            Stretch stretch
        )
        {
            MediaType = mediaType;
            Source = source;
            Duration = duration;
            IsLoop = isLoop;
            Stretch = stretch;
        }

        public MediaItem(MediaType mediaType, Uri? source)
        {
            MediaType = mediaType;
            Source = source;
        }
    }
}
