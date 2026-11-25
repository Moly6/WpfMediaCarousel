using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using MediaCarousel.Models;

namespace MediaCarousel.Controls
{
    [TemplatePart(Name = PART_Presenter, Type = typeof(ContentControl))]
    public class MediaCarouselControl : Selector
    {
        private const string PART_Presenter = "PART_Presenter";
        private ContentControl? _presenter;
        private DispatcherTimer? _imageTimer;

        // 当前播放的视频元素引用，用于资源释放
        private MediaElement? _currentMediaElement;

        static MediaCarouselControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(MediaCarouselControl),
                new FrameworkPropertyMetadata(typeof(MediaCarouselControl))
            );
        }

        public MediaCarouselControl()
        {
            _imageTimer = new DispatcherTimer();
            _imageTimer.Tick += OnImageTimerTick;

            // 绑定命令
            NextCommand = new RelayCommand(Next);
            PreviousCommand = new RelayCommand(Previous);

            this.Unloaded += OnUnloaded;
        }

        #region Dependency Properties

        /// <summary>
        /// 图片自动轮播时的显示时长
        /// </summary>
        public static readonly DependencyProperty ImageDurationProperty =
            DependencyProperty.Register(
                "ImageDuration",
                typeof(TimeSpan),
                typeof(MediaCarouselControl),
                new PropertyMetadata(TimeSpan.FromSeconds(3))
            );

        /// <summary>
        /// 图片自动轮播时的显示时长
        /// </summary>
        public TimeSpan ImageDuration
        {
            get => (TimeSpan)GetValue(ImageDurationProperty);
            set => SetValue(ImageDurationProperty, value);
        }

        /// <summary>
        /// 是否自动播放媒体内容（图片轮播或视频播放）
        /// </summary>
        public static readonly DependencyProperty IsAutoPlayProperty = DependencyProperty.Register(
            "IsAutoPlay",
            typeof(bool),
            typeof(MediaCarouselControl),
            new PropertyMetadata(true, OnAutoPlayChanged)
        );

        /// <summary>
        /// 是否自动播放媒体内容
        /// </summary>
        public bool IsAutoPlay
        {
            get => (bool)GetValue(IsAutoPlayProperty);
            set => SetValue(IsAutoPlayProperty, value);
        }

        /// <summary>
        /// 是否显示左右切换按钮
        /// </summary>
        public static readonly DependencyProperty ShowNavigationButtonsProperty =
            DependencyProperty.Register(
                nameof(ShowNavigationButtons),
                typeof(bool),
                typeof(MediaCarouselControl),
                new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否显示左右切换按钮
        /// </summary>
        public bool ShowNavigationButtons
        {
            get => (bool)GetValue(ShowNavigationButtonsProperty);
            set => SetValue(ShowNavigationButtonsProperty, value);
        }

        /// <summary>
        /// 视频播放时是否静音
        /// </summary>
        public static readonly DependencyProperty IsMutedProperty = DependencyProperty.Register(
            nameof(IsMuted),
            typeof(bool),
            typeof(MediaCarouselControl),
            new PropertyMetadata(true)
        );

        /// <summary>
        /// 视频播放时是否静音
        /// </summary>
        public bool IsMuted
        {
            get => (bool)GetValue(IsMutedProperty);
            set => SetValue(IsMutedProperty, value);
        }

        /// <summary>
        /// 是否显示底部圆点指示器
        /// </summary>
        public static readonly DependencyProperty ShowIndicatorsProperty =
            DependencyProperty.Register(
                nameof(ShowIndicators),
                typeof(bool),
                typeof(MediaCarouselControl),
                new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否显示底部圆点指示器
        /// </summary>
        public bool ShowIndicators
        {
            get => (bool)GetValue(ShowIndicatorsProperty);
            set => SetValue(ShowIndicatorsProperty, value);
        }

        /// <summary>
        /// 全局视频占位图（当 MediaItem 未指定 PlaceholderSource 时使用）
        /// </summary>
        public static readonly DependencyProperty PlaceholderImageProperty =
            DependencyProperty.Register(
                nameof(PlaceholderImage),
                typeof(ImageSource),
                typeof(MediaCarouselControl),
                new PropertyMetadata(null)
            );

        /// <summary>
        /// 全局视频占位图
        /// </summary>
        public ImageSource? PlaceholderImage
        {
            get => (ImageSource?)GetValue(PlaceholderImageProperty);
            set => SetValue(PlaceholderImageProperty, value);
        }

        /// <summary>
        /// 占位图淡出动画时长
        /// </summary>
        public static readonly DependencyProperty PlaceholderFadeDurationProperty =
            DependencyProperty.Register(
                nameof(PlaceholderFadeDuration),
                typeof(TimeSpan),
                typeof(MediaCarouselControl),
                new PropertyMetadata(TimeSpan.FromSeconds(0.2))
            );

        /// <summary>
        /// 占位图淡出动画时长
        /// </summary>
        public TimeSpan PlaceholderFadeDuration
        {
            get => (TimeSpan)GetValue(PlaceholderFadeDurationProperty);
            set => SetValue(PlaceholderFadeDurationProperty, value);
        }

        /// <summary>
        /// 是否启用 TikTok 风格的模糊背景
        /// </summary>
        public static readonly DependencyProperty EnableBlurBackgroundProperty =
            DependencyProperty.Register(
                nameof(EnableBlurBackground),
                typeof(bool),
                typeof(MediaCarouselControl),
                new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否启用 TikTok 风格的模糊背景
        /// </summary>
        public bool EnableBlurBackground
        {
            get => (bool)GetValue(EnableBlurBackgroundProperty);
            set => SetValue(EnableBlurBackgroundProperty, value);
        }

        /// <summary>
        /// 背景模糊半径
        /// </summary>
        public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(
            nameof(BlurRadius),
            typeof(double),
            typeof(MediaCarouselControl),
            new PropertyMetadata(40.0)
        );

        /// <summary>
        /// 背景模糊半径
        /// </summary>
        public double BlurRadius
        {
            get => (double)GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }

        /// <summary>
        /// 背景遮罩透明度（0-1，建议 0.3-0.6）
        /// </summary>
        public static readonly DependencyProperty BackgroundOverlayOpacityProperty =
            DependencyProperty.Register(
                nameof(BackgroundOverlayOpacity),
                typeof(double),
                typeof(MediaCarouselControl),
                new PropertyMetadata(0.4)
            );

        /// <summary>
        /// 背景遮罩透明度
        /// </summary>
        public double BackgroundOverlayOpacity
        {
            get => (double)GetValue(BackgroundOverlayOpacityProperty);
            set => SetValue(BackgroundOverlayOpacityProperty, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// 切换到下一项的命令
        /// </summary>
        public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(
            nameof(NextCommand),
            typeof(ICommand),
            typeof(MediaCarouselControl)
        );

        /// <summary>
        /// 切换到下一项的命令
        /// </summary>
        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        /// <summary>
        /// 切换到上一项的命令
        /// </summary>
        public static readonly DependencyProperty PreviousCommandProperty =
            DependencyProperty.Register(
                nameof(PreviousCommand),
                typeof(ICommand),
                typeof(MediaCarouselControl)
            );

        /// <summary>
        /// 切换到上一项的命令
        /// </summary>
        public ICommand PreviousCommand
        {
            get => (ICommand)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }

        #endregion

        #region Life Cycle & Logic

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _presenter = GetTemplateChild(PART_Presenter) as ContentControl;
            HandleChange();
        }

        private void HandleChange()
        {
            UpdateView();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            HandleChange();
        }

        private static void OnAutoPlayChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            var control = (MediaCarouselControl)d;
            if ((bool)e.NewValue)
            {
                // 重新触发当前项的播放逻辑
                control.HandleChange();
            }
            else
            {
                control.StopAllTimers();
                // 如果是视频，可能需要暂停，这里根据需求决定是否 Pause
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CleanupCurrentMedia();
            StopAllTimers();
        }

        #endregion

        #region Core Rendering Logic

        private void UpdateView()
        {
            if (_presenter == null || SelectedItem == null)
                return;

            // 1. 清理旧资源
            CleanupCurrentMedia();

            // 2. 获取新数据
            if (SelectedItem is not MediaItem item)
                return;

            // 3. 根据类型创建控件
            if (item.MediaType == MediaType.Image)
            {
                RenderImage(item);
            }
            else if (item.MediaType == MediaType.Video)
            {
                RenderVideo(item);
            }
        }

        private void RenderImage(MediaItem item)
        {
            var container = new Grid();

            if (item.Source == null)
            {
                var placeholderSource = item.PlaceholderSource ?? PlaceholderImage;
                if (placeholderSource != null)
                {
                    // 添加模糊背景层
                    if (EnableBlurBackground)
                    {
                        container.Children.Add(CreateBlurBackground(placeholderSource));
                    }

                    var placeholderImage = new Image
                    {
                        Source = placeholderSource,
                        Stretch = item.Stretch
                    };
                    container.Children.Add(placeholderImage);
                }
            }
            else
            {
                var mainSource = new BitmapImage(item.Source);

                // 添加模糊背景层
                if (EnableBlurBackground)
                {
                    container.Children.Add(CreateBlurBackground(mainSource));
                }

                var image = new Image { Source = mainSource, Stretch = item.Stretch };
                container.Children.Add(image);
            }

            if (_presenter != null)
            {
                _presenter.Content = container;
            }

            // 自动轮播逻辑
            if (_imageTimer != null && IsAutoPlay)
            {
                _imageTimer.Interval =
                    item.Duration > TimeSpan.Zero ? item.Duration : ImageDuration;
                _imageTimer.Start();
            }
        }

        private void RenderVideo(MediaItem item)
        {
            var container = new Grid();

            var placeholderSource = item.PlaceholderSource ?? PlaceholderImage;

            // 1. 添加模糊背景层 (视频背景始终使用缩略图以保证性能)
            if (EnableBlurBackground && placeholderSource != null)
            {
                container.Children.Add(CreateBlurBackground(placeholderSource));
            }

            // 2. 前景占位图 (用于淡出效果)
            var placeholderImage = new Image { Stretch = item.Stretch, Opacity = 1 };
            if (placeholderSource != null)
            {
                placeholderImage.Source = placeholderSource;
            }

            // 3. 视频播放器
            var mediaElement = new MediaElement
            {
                Source = item.Source,
                Stretch = item.Stretch,
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Manual,
                ScrubbingEnabled = true,
                IsMuted = IsMuted,
                Opacity = 0,
            };

            container.Children.Add(placeholderImage);
            container.Children.Add(mediaElement);

            _currentMediaElement = mediaElement;
            if (_presenter != null)
                _presenter.Content = container;

            mediaElement.MediaOpened += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var fadeOut = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = PlaceholderFadeDuration,
                    };
                    placeholderImage.BeginAnimation(Image.OpacityProperty, fadeOut);

                    var fadeIn = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = PlaceholderFadeDuration,
                    };
                    mediaElement.BeginAnimation(MediaElement.OpacityProperty, fadeIn);
                });
            };

            mediaElement.MediaEnded += (s, e) =>
            {
                if (item.IsLoop)
                {
                    mediaElement.Position = TimeSpan.Zero;
                    mediaElement.Play();
                }
                else if (IsAutoPlay)
                {
                    Next();
                }
            };

            mediaElement.MediaFailed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var fallbackSource = PlaceholderImage;
                    if (
                        fallbackSource != null
                        && !ReferenceEquals(placeholderImage.Source, fallbackSource)
                    )
                    {
                        placeholderImage.Source = fallbackSource;
                    }
                    placeholderImage.BeginAnimation(Image.OpacityProperty, null);
                    placeholderImage.Opacity = 1;
                    mediaElement.BeginAnimation(MediaElement.OpacityProperty, null);
                    mediaElement.Opacity = 0;

                    Next();
                });
            };

            mediaElement.Play();
        }

        /// <summary>
        /// 创建带有模糊效果和暗色遮罩的背景层
        /// </summary>
        private UIElement CreateBlurBackground(ImageSource source)
        {
            var grid = new Grid();

            // 1. 模糊图片
            var bgImage = new Image
            {
                Source = source,
                Stretch = Stretch.UniformToFill, // 背景必须填充全屏
                Effect = new BlurEffect { Radius = BlurRadius, KernelType = KernelType.Gaussian }
            };
            grid.Children.Add(bgImage);

            // 2. 暗色半透明遮罩 (TikTok 风格)
            var overlay = new Border
            {
                Background = new SolidColorBrush(Colors.Black) { Opacity = BackgroundOverlayOpacity }
            };
            grid.Children.Add(overlay);

            return grid;
        }

        #endregion

        #region Control Logic

        private void OnImageTimerTick(object? sender, EventArgs e)
        {
            _imageTimer?.Stop();
            Next();
        }

        public void Next()
        {
            if (Items.Count == 0)
                return;
            // 计算索引，处理循环
            SelectedIndex = (SelectedIndex + 1) % Items.Count;
        }

        public void Previous()
        {
            if (Items.Count == 0)
                return;
            var index = SelectedIndex - 1;
            if (index < 0)
                index = Items.Count - 1;
            SelectedIndex = index;
        }

        private void StopAllTimers()
        {
            _imageTimer?.Stop();
        }

        /// <summary>
        /// 停止轮播
        /// </summary>
        public void StopCarousel()
        {
            _imageTimer?.Stop();

            if (_currentMediaElement != null)
            {
                _currentMediaElement.Pause();
            }
        }

        /// <summary>
        /// 继续当前轮播顺序
        /// </summary>
        public void StartCarousel()
        {
            HandleChange();
        }

        /// <summary>
        /// 重新开始轮播，清理视频资源后回到第一项
        /// </summary>
        public void RestartCarousel()
        {
            CleanupCurrentMedia();

            if (Items.Count > 0)
            {
                SelectedIndex = 0;
            }
            IsAutoPlay = true;
            HandleChange();
        }

        /// <summary>
        /// 清理视频资源
        /// </summary>
        public void CleanupResources()
        {
            StopAllTimers();
            CleanupCurrentMedia();
        }

        /// <summary>
        /// 清理视频资源
        /// </summary>
        public Task CleanupResourcesAsync()
        {
            CleanupResources();
            return Task.CompletedTask;
        }

        private void CleanupCurrentMedia()
        {
            // 停止计时器
            _imageTimer?.Stop();

            // 如果上一个是视频，强制停止并释放
            if (_currentMediaElement != null)
            {
                try
                {
                    _currentMediaElement.Stop();
                    _currentMediaElement.Close();
                    _currentMediaElement.Source = null;
                }
                catch
                { /* 忽略销毁时的错误 */
                }
                finally
                {
                    _currentMediaElement = null;
                }
            }

            // 简单的内容清空，防止内存泄漏
            if (_presenter != null)
            {
                _presenter.Content = null;
            }
        }

        #endregion

        #region Container Management

        /// <summary>
        /// 获取或创建项容器
        /// </summary>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListBoxItem();
        }

        /// <summary>
        /// 判断项是否是自己的容器
        /// </summary>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ListBoxItem;
        }

        #endregion

        #region SourceChange

        protected override void OnItemsSourceChanged(
            System.Collections.IEnumerable oldValue,
            System.Collections.IEnumerable newValue
        )
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            // 关键逻辑：如果有数据，强制选中第一项
            if (this.Items.Count > 0)
            {
                this.SelectedIndex = 0;
                // 设置 SelectedIndex 会自动触发 OnSelectionChanged -> UpdateView
            }
            else
            {
                this.SelectedIndex = -1; // 清空选中
            }
        }

        protected override void OnItemsChanged(
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
        {
            base.OnItemsChanged(e);

            // 场景：原本是空的，现在添加了第一个数据，应该自动开始播放
            if (this.SelectedIndex == -1 && this.Items.Count > 0)
            {
                this.SelectedIndex = 0;
            }

            // 场景：数据被清空了
            if (this.Items.Count == 0)
            {
                // 这里可以调用你的清理逻辑
                // CleanupCurrentMedia();
                this.SelectedIndex = -1;
            }
        }
        #endregion
    }
}
