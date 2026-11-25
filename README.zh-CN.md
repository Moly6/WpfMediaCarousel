# WpfMediaCarousel

[English](README.md) | 简体中文

一个用于 WPF 应用的图片与视频轮播控件，支持自动播放、手动导航、位置指示器、视频占位图以及模糊背景。

## 功能

- 展示图片和视频媒体项。
- 为图片设置单独的显示时长，也可以使用控件的默认时长。
- 自动播放、上一项/下一项导航和位置指示器。
- 支持视频循环、静音和视频占位图。
- 可选模糊背景与背景遮罩。
- 提供导航命令及播放控制方法。

## 环境要求

- Windows 和 WPF。
- 新项目建议使用 .NET 10；控件同时面向 `net6.0-windows` 和 .NET Framework 4.7.2（`net472`）。.NET 6 已结束支持。
- 视频播放由 WPF `MediaElement` 提供，目标电脑需要支持相应媒体格式和编解码器。

## 添加到 WPF 项目

推荐从 NuGet 安装：

```powershell
dotnet add package WpfMediaCarousel
```

也可以在 Visual Studio 的 NuGet 包管理器中搜索 `WpfMediaCarousel` 并安装。若要从源码构建，则将 `src/WpfMediaCarousel/WpfMediaCarousel.csproj` 添加到解决方案并添加项目引用。

XAML 命名空间：

```xml
xmlns:carousel="clr-namespace:MediaCarousel.Controls;assembly=WpfMediaCarousel"
```

## 基本用法

在 ViewModel 中准备媒体列表：

```csharp
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaCarousel.Models;

public ObservableCollection<MediaItem> MediaItems { get; } = new()
{
    new(MediaType.Image, new Uri(
        "pack://application:,,,/MyApp;component/Assets/photo-1.jpg"))
    {
        Duration = TimeSpan.FromSeconds(5),
        Stretch = Stretch.UniformToFill,
        Tag = "第一张图片"
    },
    new(MediaType.Video, new Uri(@"C:\Media\clip.mp4"))
    {
        IsLoop = true,
        PlaceholderSource = new BitmapImage(new Uri(
            "pack://application:,,,/MyApp;component/Assets/video-cover.jpg")),
        Tag = "视频"
    }
};
```

在窗口或用户控件中绑定列表：

```xml
<carousel:MediaCarouselControl
    ItemsSource="{Binding MediaItems}"
    ImageDuration="0:0:5"
    IsAutoPlay="True"
    IsMuted="True"
    ShowNavigationButtons="True"
    ShowIndicators="True"
    EnableBlurBackground="True"
    BlurRadius="24"
    BackgroundOverlayOpacity="0.2"
    PlaceholderImage="pack://application:,,,/MyApp;component/Assets/video-cover.jpg" />
```

控件会在媒体列表有内容时选中并显示第一项。`ItemsSource` 中每个 `MediaItem.Source` 是媒体实际来源；`PlaceholderImage` 仅作为视频的默认占位图。请确保本地媒体被复制到程序输出目录，或使用有效的绝对路径/Pack URI。

## 控件属性

| 属性 | 说明 |
| --- | --- |
| `ImageDuration` | 图片项的默认显示时长。 |
| `IsAutoPlay` | 自动切换图片并启动视频播放。 |
| `ShowNavigationButtons` | 是否显示上一项和下一项按钮。 |
| `ShowIndicators` | 是否显示轮播位置指示器。 |
| `IsMuted` | 视频是否静音。 |
| `PlaceholderImage` | 未单独指定占位图的视频默认占位图。 |
| `PlaceholderFadeDuration` | 视频占位图淡出时长。 |
| `EnableBlurBackground` | 是否显示媒体模糊背景。 |
| `BlurRadius` | 背景模糊半径。 |
| `BackgroundOverlayOpacity` | 背景遮罩透明度。 |
| `NextCommand`、`PreviousCommand` | 下一项和上一项命令。 |

`MediaItem` 包含 `MediaType`、`Source`、`Duration`、`IsLoop`、`Stretch`、`PlaceholderSource`、`Tag` 和 `TagPlaceholder`。`Duration` 用于图片项；视频项的播放时长由媒体文件决定，循环播放的视频会持续播放，直到用户切换。

控件还提供 `Next()`、`Previous()`、`StartCarousel()`、`StopCarousel()`、`RestartCarousel()`、`CleanupResources()` 和 `CleanupResourcesAsync()` 方法，供代码进行导航、播放控制和资源清理。

## 示例项目

可运行示例见 [WpfMediaCarousel.Demo](https://github.com/Moly6/WpfMediaCarousel.Demo)。该示例通过 NuGet 包引用控件，直接克隆仓库即可运行：

```powershell
git clone https://github.com/Moly6/WpfMediaCarousel.Demo.git
```

## 许可证

本项目采用 MIT License，详见 [LICENSE](LICENSE)。
