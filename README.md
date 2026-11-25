# WpfMediaCarousel

English | [简体中文](README.zh-CN.md)

A reusable WPF control for presenting image and video items in a carousel.

## Features

- Image slides with per-item durations and automatic advance.
- Video playback with optional looping and a placeholder image.
- Previous/next navigation and position indicators.
- Optional blurred image backgrounds and overlay tint.
- Public commands and methods for navigation and playback control.

## Requirements

- Windows with the .NET 10 SDK/runtime for the recommended target.
- Also targets `net6.0-windows` and .NET Framework 4.7.2 (`net472`) for existing applications. .NET 6 has reached end of support; use .NET 10 for new applications.
- WPF `MediaElement` video playback depends on Windows media codecs and supported media formats.

## Add to an application

Install the package from NuGet:

```powershell
dotnet add package WpfMediaCarousel
```

Or search for `WpfMediaCarousel` in Visual Studio's NuGet Package Manager. To build from source instead, add `src/WpfMediaCarousel/WpfMediaCarousel.csproj` to your WPF solution. Then reference the control namespace:

```xml
xmlns:carousel="clr-namespace:MediaCarousel.Controls;assembly=WpfMediaCarousel"
```

Create media items in a view model:

```csharp
using System.Collections.ObjectModel;
using System.Windows.Media;
using MediaCarousel.Models;

public ObservableCollection<MediaItem> MediaItems { get; } = new()
{
    new(MediaType.Image, new Uri("pack://application:,,,/MyApp;component/Assets/photo-1.jpg"))
    {
        Duration = TimeSpan.FromSeconds(5),
        Stretch = Stretch.UniformToFill,
        Tag = "First photo"
    },
    new(MediaType.Video, new Uri("C:\\Media\\clip.mp4"))
    {
        IsLoop = true,
        Tag = "Video"
    }
};
```

Place the control in a window or view:

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
    BackgroundOverlayOpacity="0.2" />
```

Make sure media files are included in the application output or use a valid absolute/pack URI. For video, verify that the format is supported by the Windows media stack on the target machine.

## Control properties

| Property | Purpose |
| --- | --- |
| `ImageDuration` | Default display duration for image slides. |
| `IsAutoPlay` | Automatically advances images and starts video playback. |
| `ShowNavigationButtons` | Shows previous and next buttons. |
| `ShowIndicators` | Shows slide position indicators. |
| `IsMuted` | Mutes video playback. |
| `PlaceholderImage` | Default placeholder for videos without a per-item placeholder. |
| `PlaceholderFadeDuration` | Fade duration for the video placeholder. |
| `EnableBlurBackground` | Shows a blurred background behind media. |
| `BlurRadius` | Blur amount when the blurred background is enabled. |
| `BackgroundOverlayOpacity` | Opacity of the background overlay. |
| `NextCommand`, `PreviousCommand` | Commands exposed for navigation. |

`MediaItem` provides `MediaType`, `Source`, `Duration`, `IsLoop`, `Stretch`, `PlaceholderSource`, `Tag`, and `TagPlaceholder`. `Duration` is used for images; video duration is controlled by media playback unless the item loops.

The control also exposes `Next()`, `Previous()`, `StartCarousel()`, `StopCarousel()`, `RestartCarousel()`, `CleanupResources()`, and `CleanupResourcesAsync()` for imperative control and teardown.

## Demo

See [WpfMediaCarousel.Demo](https://github.com/Moly6/WpfMediaCarousel.Demo) for a runnable example that consumes the NuGet package.

```powershell
git clone https://github.com/Moly6/WpfMediaCarousel.Demo.git
```

## License

MIT. See [LICENSE](LICENSE).
