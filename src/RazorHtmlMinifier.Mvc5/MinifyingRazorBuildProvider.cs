using System.Web.Razor.Generator;
using System.Web.WebPages.Razor;

namespace RazorHtmlMinifier.Mvc5
{
    /// <inherit/>
    public class MinifyingRazorBuildProvider : RazorBuildProvider
    {
        /// <inherit/>
        protected override WebPageRazorHost GetHostFromConfig()
        {
            var host = base.GetHostFromConfig();

            if (host is WebCodeRazorHost) // this will be true when the virtual path starts with "~/App_Code"
            {
                return new MinifyingWebCodeRazorHost(host.VirtualPath, host.PhysicalPath);
            }

            return host;
        }

        private class MinifyingWebCodeRazorHost : WebCodeRazorHost
        {
            public MinifyingWebCodeRazorHost(string virtualPath, string physicalPath) : base(virtualPath, physicalPath)
            {
            }

            public override RazorCodeGenerator DecorateCodeGenerator(RazorCodeGenerator incomingCodeGenerator)
            {
                if (!(incomingCodeGenerator is CSharpRazorCodeGenerator))
                    return base.DecorateCodeGenerator(incomingCodeGenerator);

                return new MinifyingMvcCSharpRazorCodeGenerator(incomingCodeGenerator.ClassName, incomingCodeGenerator.RootNamespaceName, incomingCodeGenerator.SourceFileName, incomingCodeGenerator.Host);
            }
        }
    }
}