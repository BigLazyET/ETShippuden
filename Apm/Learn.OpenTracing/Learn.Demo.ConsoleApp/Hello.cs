using OpenTracing;
using Microsoft.Extensions.Logging;
using System.Net;
using OpenTracing.Tag;
using OpenTracing.Propagation;

namespace Learn.Demo.ConsoleApp
{
    public class Hello
    {
        private readonly ITracer tracer;
        private readonly ILogger<Hello> logger;

        public Hello(ITracer tracer, ILoggerFactory loggerFactory)
        {
            this.tracer = tracer;
            logger = loggerFactory.CreateLogger<Hello>();
        }

        /// <summary>
        /// Just trace -> span
        /// </summary>
        /// <param name="content"></param>
        public void SayHello(string content)
        {
            var spanBuilder = tracer.BuildSpan("say-hello");
            var span = spanBuilder.Start();
            logger.LogInformation($"Hello {content}");
            span.Finish();
        }

        /// <summary>
        /// Trace -> span -> span -> span  with AsChildOf to descripe father-son relationship
        /// </summary>
        /// <param name="content"></param>
        public void SayHello2(string content)
        {
            var span = tracer.BuildSpan("say-hello2").Start();
            var helloString = FormatString(span, content);
            PrintHello(span, helloString);
            span.Finish();
        }

        /// <summary>
        /// Trace -> Span -> Span -> Span With StartActive(bool) to replace AsChildOf()
        /// </summary>
        /// <param name="content"></param>
        public void SayHello3(string content)
        {
            using var scope = tracer.BuildSpan("say-hello3").StartActive(true);
            scope.Span.SetTag("hello-to", content);
            var helloString = FormatString(content);
            PrintHello(helloString);
        }

        public void SayHello4(string content)
        {
            using var scope = tracer.BuildSpan("say-hello4").StartActive(true);
            scope.Span.SetTag("hello-to", content);
            var helloString = FormatStringFromApi(content);
            PrintHello(helloString);
        }

        private string FormatString(ISpan rootSpan, string helloTo)
        {
            //var span = tracer.BuildSpan("format-string").Start();
            var span = tracer.BuildSpan("format-string").AsChildOf(rootSpan).Start();   // 组合两个Span，显示其因果关系
            try
            {
                var helloString = $"Hello, {helloTo}";
                span.Log(new Dictionary<string, object>
                {
                    [LogFields.Event] = "string.format",
                    ["value"] = helloString
                });
                return helloString;
            }
            finally
            {
                span.Finish();
            }
        }

        private string FormatString(string helloTo)
        {
            // 其存储在线程本地存储中来使 span 处于“活动”状态
            // 自动创建 ChildOf 对先前活动范围的引用，因此我们不必AsChildOf()显式使用 builder 方法
            using var scope = tracer.BuildSpan("format-string").StartActive(true);
            var helloString = $"Hello, {helloTo}";
            scope.Span.Log(new Dictionary<string, object>
            {
                { LogFields.Event,"string.format" },
                { "value", helloString }
            });
            return helloString;
        }

        private string FormatStringFromApi(string helloTo)
        {
            using (var scope = tracer.BuildSpan("format-string").StartActive(true))
            {
                using var httpClient = new HttpClient();
                var url = $"http://localhost:5025/api/format/{helloTo}";
                var response = new ValueTask<HttpResponseMessage>(httpClient.GetAsync(url));
                var helloString = string.Empty;
                if (response.IsCompleted)
                {
                    helloString = response.Result.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    helloString = response.AsTask().Result.Content.ReadAsStringAsync().Result;
                }
                var span = scope.Span
                    .SetTag(Tags.SpanKind, Tags.SpanKindClient)
                    .SetTag(Tags.HttpMethod, "GET") 
                    .SetTag(Tags.HttpUrl, url);

                var dictionary = new Dictionary<string, string>();
                tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));
                foreach (var entry in dictionary)
                {
                    httpClient.DefaultRequestHeaders.Add(entry.Key, entry.Value);
                }
                return helloString;
            }
        }

        private void PrintHello(ISpan rootSpan, string helloString)
        {
            //var span = tracer.BuildSpan("print-hello").Start();
            var span = tracer.BuildSpan("print-hello").AsChildOf(rootSpan).Start();
            logger.LogInformation(helloString);
            span.Log("WriteLine");
            span.Finish();
        }

        private void PrintHello(string helloString)
        {
            using var scope = tracer.BuildSpan("print-hello").StartActive(true);
            logger.LogInformation(helloString);
            scope.Span.Log(new Dictionary<string, object>
            {
                [LogFields.Event] = "WriteLine"
            });
        }
    }
}
