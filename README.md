RazorHtmlMinifier.Mvc5
======================

[![Build status](https://ci.appveyor.com/api/projects/status/ii20k891o3t38q9k?svg=true)](https://ci.appveyor.com/project/tompazourek/razorhtmlminifier-mvc5)

*Trivial compile-time Razor HTML Minifier for ASP.NET MVC 5.*

Installation
------------

### Download

Binaries of the last build can be downloaded on the [AppVeyor CI page of the project](https://ci.appveyor.com/project/tompazourek/razorhtmlminifier-mvc5/build/artifacts).

The library is also [published on NuGet.org](https://www.nuget.org/packages/RazorHtmlMinifier.Mvc5/), install using:

```
PM> Install-Package RazorHtmlMinifier.Mvc5
```

<sup>RazorHtmlMinifier.Mvc5 is is built as for .NET v4.5 with a dependency on ASP.NET MVC 5.2.3.</sup>

### Configuration

Find the **Web.config** with your Razor configuration (by default it's in `Views/Web.config`). You should see something like this inside:

```xml
<host factoryType="System.Web.Mvc.MvcWebRazorHostFactory, System.Web.Mvc, Version=5.2.3.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
```

In order to start minifying, replace it with (after the NuGet package is installed):

```xml
<host factoryType="RazorHtmlMinifier.Mvc5.MinifyingMvcWebRazorHostFactory, RazorHtmlMinifier.Mvc5, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
```

Then rebuild your solution, which should also restart the app.


How it works
------------

The minifier processes the code generated during Razor compilation. Because it runs in compile-time, it shouldn't add any overhead during runtime.

The entire source code is just a [single file](https://github.com/tompazourek/RazorHtmlMinifier.Mvc5/blob/master/src/RazorHtmlMinifier.Mvc5/MinifyingMvcWebRazorHostFactory.cs), feel free to view it.

The minification applied is very trivial as you can [see here](https://github.com/tompazourek/RazorHtmlMinifier.Mvc5/blob/master/src/RazorHtmlMinifier.Mvc5/MinifyingMvcWebRazorHostFactory.cs#L47-L55). It basically:

- replaces multiple white-space characters next to each other with a single space;
- replaces multiple white-space characters containing line breaks with a single line break.

The minification process is deliberately trivial so that its behaviour would be easy to understand and expect.

**CAUTION! The minification is not context-sensitive, and it doesn't have any special handling of `<pre>` tags or similar. If you use `<pre>` tags or have any other significant white-space in your HTML, you shouldn't use this library.**

The code is inspired by [Meleze.Web](https://github.com/meleze/Meleze.Web) (an older project), but it's much simplified and updated to be used for with the latest version of ASP.NET MVC.