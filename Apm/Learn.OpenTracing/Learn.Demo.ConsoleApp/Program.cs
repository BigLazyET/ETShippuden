// See https://aka.ms/new-console-template for more information

using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Grpc;
using Learn.Demo.ConsoleApp;
using Microsoft.Extensions.Logging;
using OpenTracing.Util;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
// var trace = GlobalTracer.Instance;   // 链路追踪实例，无操作的tracer
//var trace = InitTracer("hello-world", loggerFactory);  // 自定义操作tracer
//var hello = new Hello(trace, loggerFactory);
var trace = InitTracerRemoteReport("hello-world", loggerFactory);  // 自定义操作tracer
var hello = new Hello(trace, loggerFactory);

hello.SayHello("This trace");

await Task.Delay(1000);
hello.SayHello2("This trace2");

await Task.Delay(1000);
hello.SayHello3("This trace3");

await Task.Delay(1000);
hello.SayHello4("This trace4");


Console.Read();

Tracer InitTracer(string serviceName, ILoggerFactory loggerFactory)
{
    // 采样(采样命中率，filter，头部/尾部采样策略等配置)
    var sampleConfig = new Configuration.SamplerConfiguration(loggerFactory).WithType(ConstSampler.Type).WithParam(1);

    // 报告(向哪里传送tracing数据等等配置)
    var reportConfig = new Configuration.ReporterConfiguration(loggerFactory).WithLogSpans(true);

    var tracer = (Tracer)new Configuration(serviceName, loggerFactory).WithSampler(sampleConfig).WithReporter(reportConfig).GetTracer();

    return tracer;
}

Tracer InitTracerRemoteReport(string serviceName, ILoggerFactory loggerFactory)
{
    Configuration.SenderConfiguration.DefaultSenderResolver = new Jaeger.Senders.SenderResolver(loggerFactory)
        .RegisterSenderFactory<GrpcSenderFactory>();

    var reporter = new RemoteReporter.Builder()
        .WithLoggerFactory(loggerFactory)
        .WithSender(new GrpcSender("10.181.129.3:14250", null, 0))
        .Build();

    var tracer = new Tracer.Builder(serviceName)
        .WithLoggerFactory(loggerFactory)
        .WithSampler(new ConstSampler(true))
        .WithReporter(reporter)
        .Build();

    return tracer;
}
