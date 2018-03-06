using System;
using System.CodeDom;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Razor;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Tokenizer.Symbols;
using System.Web.WebPages.Razor;

namespace RazorHtmlMinifier.Mvc5
{
    public class MinifyingMvcWebRazorHostFactory : MvcWebRazorHostFactory
    {
        public override WebPageRazorHost CreateHost(string virtualPath, string physicalPath)
        {
            var host = base.CreateHost(virtualPath, physicalPath);

            if (!host.IsSpecialPage)
            {
                return new MinifyingMvcWebPageRazorHost(virtualPath, physicalPath);
            }

            return host;
        }

        private class MinifyingMvcWebPageRazorHost : MvcWebPageRazorHost
        {
            public MinifyingMvcWebPageRazorHost(string virtualPath, string physicalPath)
                : base(virtualPath, physicalPath)
            {
            }

            public override RazorCodeGenerator DecorateCodeGenerator(RazorCodeGenerator incomingCodeGenerator)
            {
                if (!(incomingCodeGenerator is CSharpRazorCodeGenerator))
                    return base.DecorateCodeGenerator(incomingCodeGenerator);

                return new MinifyingMvcCSharpRazorCodeGenerator(incomingCodeGenerator.ClassName, incomingCodeGenerator.RootNamespaceName, incomingCodeGenerator.SourceFileName, incomingCodeGenerator.Host);
            }
        }

        private class MinifyingMvcCSharpRazorCodeGenerator : CSharpRazorCodeGenerator
        {
            public MinifyingMvcCSharpRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
                : base(className, rootNamespaceName, sourceFileName, host)
            {
                if (host is MvcWebPageRazorHost mvcHost && !mvcHost.IsSpecialPage)
                {
                    // set base type dynamic by default
                    var baseType = new CodeTypeReference($"{Context.Host.DefaultBaseClass}<dynamic>");
                    Context.GeneratedClass.BaseTypes.Clear();
                    Context.GeneratedClass.BaseTypes.Add(baseType);
                }
            }

            public override void VisitSpan(Span currentSpan)
            {
                if (currentSpan?.Kind == SpanKind.Markup)
                {
                    VisitMarkupSpan(currentSpan);
                }

                base.VisitSpan(currentSpan);
            }

            private void VisitMarkupSpan(Span currentMarkupSpan)
            {
                var builder = new SpanBuilder(currentMarkupSpan);
                builder.ClearSymbols();

                var previousMarkupSpan = currentMarkupSpan.Previous?.Kind == SpanKind.Markup ? currentMarkupSpan.Previous : null;
                var previousSymbol = previousMarkupSpan?.Symbols.LastOrDefault();

                foreach (var currentSymbol in currentMarkupSpan.Symbols)
                {
                    VisitSymbol(currentSymbol, previousSymbol, builder);
                    previousSymbol = currentSymbol;
                }

                currentMarkupSpan.ReplaceWith(builder);
            }

            private static void VisitSymbol(ISymbol currentSymbol, ISymbol previousSymbol, SpanBuilder builder)
            {
                if (IsSymbolWhiteSpaceOrNewLine(currentSymbol, out var currentHtmlSymbol))
                {
                    if (IsSymbolWhiteSpaceOrNewLine(previousSymbol, out var _))
                    {
                        // both current and previous symbols are whitespace/newline, we can skip current symbol
                        return;
                    }

                    // current symbol is whitespace/newline, previous is not, we'll replace current with the smallest
                    var replacementSymbol = GetReplacementSymbol(currentHtmlSymbol);
                    builder.Accept(replacementSymbol);
                    return;
                }

                builder.Accept(currentSymbol);
            }

            private static bool IsSymbolWhiteSpaceOrNewLine(ISymbol symbol, out HtmlSymbol outputHtmlSymbol)
            {
                if (symbol is HtmlSymbol htmlSymbol)
                {
                    if (htmlSymbol.Type == HtmlSymbolType.WhiteSpace || htmlSymbol.Type == HtmlSymbolType.NewLine)
                    {
                        outputHtmlSymbol = htmlSymbol;
                        return true;
                    }
                }

                outputHtmlSymbol = null;
                return false;
            }

            private static HtmlSymbol GetReplacementSymbol(HtmlSymbol htmlSymbol)
            {
                switch (htmlSymbol.Type)
                {
                    case HtmlSymbolType.WhiteSpace:
                        // any amount of whitespace is replaced with a single space
                        return new HtmlSymbol(htmlSymbol.Start, " ", HtmlSymbolType.WhiteSpace, htmlSymbol.Errors);
                    case HtmlSymbolType.NewLine:
                        // newline is replaced with just \n
                        return new HtmlSymbol(htmlSymbol.Start, "\n", HtmlSymbolType.NewLine, htmlSymbol.Errors);
                    default:
                        throw new ArgumentException($"Expected either {HtmlSymbolType.WhiteSpace} or {HtmlSymbolType.NewLine} symbol, {htmlSymbol.Type} given.");
                }
            }
        }
    }
}