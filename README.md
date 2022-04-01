![RazorHtmlMinifier.Mvc5 logo](https://raw.githubusercontent.com/tompazourek/RazorHtmlMinifier.Mvc5/master/assets/logo_32.png) RazorHtmlMinifier.Mvc5
======================

[![Build status](https://img.shields.io/appveyor/ci/tompazourek/razorhtmlminifier-mvc5/master.svg)](https://ci.appveyor.com/project/tompazourek/razorhtmlminifier-mvc5)
[![NuGet version](https://img.shields.io/nuget/v/RazorHtmlMinifier.Mvc5.svg)](https://www.nuget.org/packages/RazorHtmlMinifier.Mvc5/)
[![NuGet downloads](https://img.shields.io/nuget/dt/RazorHtmlMinifier.Mvc5.svg)](https://www.nuget.org/packages/RazorHtmlMinifier.Mvc5/)

*Trivial compile-time Razor HTML Minifier for ASP.NET MVC 5.*

Installation
------------

### Download

Binaries of the last build can be downloaded on the [AppVeyor CI page of the project](https://ci.appveyor.com/project/tompazourek/razorhtmlminifier-mvc5/build/artifacts).

The library is also [published on NuGet.org](https://www.nuget.org/packages/RazorHtmlMinifier.Mvc5/), install using:

```
PM> Install-Package RazorHtmlMinifier.Mvc5
```

<sup>RazorHtmlMinifier.Mvc5 is built for .NET v4.8 with a dependency on ASP.NET MVC 5.2.7 and `System.Web`.</sup>

### Configuration

Find the **Web.config** with your Razor configuration (by default it's in `Views/Web.config`). You should see something like this inside:

```xml
<host factoryType="System.Web.Mvc.MvcWebRazorHostFactory, System.Web.Mvc, Version=5.2.7.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" />
```

In order to start minifying views and partial views, replace it with (after the NuGet package is installed):

```xml
<host factoryType="RazorHtmlMinifier.Mvc5.MinifyingMvcWebRazorHostFactory, RazorHtmlMinifier.Mvc5, Version=2.0.0.0, Culture=neutral, PublicKeyToken=a517a17e203fcde4" />
```

Then rebuild your solution, which should also restart the app.

If you're using Razor `@helper` functions placed inside the `App_Code` folder, you have to do additional configuration in order to minify those. In the root `Web.config`, you should see something like this:

```xml
<compilation debug="true" targetFramework="4.8" />
```

Your `<compilation>` element might have different attributes. Leave the current attributes as-is, and add `<buildProviders>` like so:

```xml
<compilation debug="true" targetFramework="4.8">
    <buildProviders>
        <add extension=".cshtml" type="RazorHtmlMinifier.Mvc5.MinifyingRazorBuildProvider, RazorHtmlMinifier.Mvc5" />
    </buildProviders>
</compilation>
```

How it works
------------

The minifier processes the code generated during Razor compilation. Because it runs in compile-time, it shouldn't add any overhead during runtime.

The entire source code is just [two](/src/RazorHtmlMinifier.Mvc5/MinifyingMvcWebRazorHostFactory.cs) [files](/src/RazorHtmlMinifier.Mvc5/MinifyingRazorBuildProvider.cs), feel free to view them.

The minification algorithm is fairly trivial. It basically:

- replaces any amount of whitespace characters with a single space;
- replaces any line breaks with just the line feed character (`\n`);
- if there's a sequence of whitespace characters and line breaks, it only takes the first in the sequence.

The minification process is deliberately trivial so that it would be easy to understand and expect.

**CAUTION! The minification is not context-sensitive, and it doesn't have any special handling of `<pre>` tags or similar. If you use `<pre>` tags or have any other significant white-space in your HTML, you shouldn't use this library.**

The code is inspired by [Meleze.Web](https://github.com/meleze/Meleze.Web) (an older project), but it's much simplified and updated to be used for with the latest version of ASP.NET MVC.


Resolving IntelliSense issues
-----------------------------

When you change the Razor factory, you might experience IntelliSense issues in Visual Studio.

I've investigated this and it looks like **VS actually needs to have the assembly in GAC for IntelliSense to work**. Luckily, now when the latest version of the library is strong-named, it's possible to add it to GAC. Adding the assembly to GAC shouldn't have any side-effects, but it will need to be added on every developer machine that wants to use see the IntelliSense in Razor files completely (without the squiggly undelines), which can be a hassle.

If you want to add the assembly to GAC, you'll need to do the following:

- Open `Developer Command Prompt for VS` (you'll find it in Start menu) **as an Administrator**.
- Navigate to the folder of the NuGet package: `cd "C:\PATH_TO_YOUR_SOLUTION\packages\RazorHtmlMinifier.Mvc5.1.3.0\lib\net45"`
- Install it to GAC: `gacutil /i RazorHtmlMinifier.Mvc5.dll` (it should respond `Assembly successfully added to the cache`)
- Restart Visual Studio (and maybe also clear any ReSharper caches if you're using that)

**Then it should start working.**

There are also other alternative solutions:

- Set up the host factory in *Web.config.Release* instead of *Web.config*. That will make the minification run only when the Release configs are applied, and while debugging, you can keep the original host factory, which doesn't have problems with intellisense.
- Set the host factory in the Web.config file in runtime, e.g. in `Global.asax`. You'd have to use the `WebConfigurationManager` API to modify the Web.config during runtime.

However, I wouldn't recommend either of those (I also haven't tried them), as they feel more like a hack, and might cause more issues if you'd be doing stuff like precompilation of the views. Adding the assembly to GAC is probably the easiest, it's still annoying that VS requires that though...
