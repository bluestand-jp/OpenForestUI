> 🌐 **English** ・ [日本語](README-ja.md)

# App branding assets

Static branding assets for the OpenForestUI desktop app (`OpenForestUI.exe`): the window/executable
icons, in-app logos and small UI images, and the brand font. These are referenced from WPF XAML (via
pack URIs / relative paths) and from `OpenForestUI.csproj`, which copies several of them to the output
directory at build time.

> Note: these are the **WPF app's** assets. Overlay (browser-source) artwork lives separately under
> `Overlays/*/public/images/`.

## Subdirectories

| Directory | Purpose |
| --- | --- |
| [`Fonts/`](Fonts/) | The Venus Rising brand font, embedded for the wordmark |
| [`Icons/`](Icons/) | `.ico` window / executable icons |
| [`Images/`](Images/) | In-app logos and small UI bitmaps (PNG) |
