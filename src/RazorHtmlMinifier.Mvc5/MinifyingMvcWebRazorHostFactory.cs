using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Razor;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;
using System.Web.Razor.Tokenizer.Symbols;
using System.Web.WebPages.Razor;

namespace RazorHtmlMinifier.Mvc5
{
    public class MinifyingMvcWebRazorHostFactory : MvcWebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            var host = base.CreateHost(virtualPath, physicalPath);

            if (host.IsSpecialPage || host.DesignTimeMode)
                return host;

            return new MinifyingMvcWebPageRazorHost(virtualPath, physicalPath);
        }

        private class MinifyingMvcWebPageRazorHost : MvcWebPageRazorHost
        {
            public MinifyingMvcWebPageRazorHost(string virtualPath, string physicalPath)
                : base(virtualPath, physicalPath)
            {
            }

            public override RazorCodeGenerator DecorateCodeGenerator(RazorCodeGenerator input)
            {
                if (!(input is CSharpRazorCodeGenerator))
                    return base.DecorateCodeGenerator(input);

                return new MinifyingCSharpRazorCodeGenerator(input.ClassName, input.RootNamespaceName, input.SourceFileName, input.Host);
            }
        }

        internal class MinifyingCSharpRazorCodeGenerator : CSharpRazorCodeGenerator
        {
            public MinifyingCSharpRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host) : base(className, rootNamespaceName, sourceFileName, host)
            {
            }

            private static readonly Regex MinifyingRegexLineBreak = new Regex(@"\s*[\n\r]+\s*", RegexOptions.Compiled | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            private static readonly Regex MinifyingRegexInline = new Regex(@"\s{2,}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            public string Minify(string content)
            {
                content = MinifyingRegexLineBreak.Replace(content, "\n");
                content = MinifyingRegexInline.Replace(content, " ");
                return content;
            }

            public override void VisitSpan(Span span)
            {
                if (span.Kind == SpanKind.Markup)
                {
                    var builder = new SpanBuilder
                    {
                        CodeGenerator = span.CodeGenerator,
                        EditHandler = span.EditHandler,
                        Kind = span.Kind,
                        Start = span.Start,
                    };
                    var symbol = new MarkupSymbol { Content = Minify(span.Content) };
                    builder.Accept(symbol);
                    span.ReplaceWith(builder);
                }

                base.VisitSpan(span);
            }

            private class MarkupSymbol : ISymbol
            {
                public void OffsetStart(SourceLocation documentStart)
                {
                    Start = documentStart;
                }

                public void ChangeStart(SourceLocation newStart)
                {
                    Start = newStart;
                }

                public SourceLocation Start { get; private set; } = SourceLocation.Zero;
                public string Content { get; internal set; }
            }
        }
    }
}