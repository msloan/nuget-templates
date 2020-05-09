using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MyFunctionApp.Tests
{
    public class ListLogger : ILogger
    {
        public IList<string> Logs;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => false;

        public ListLogger()
        {
            Logs = new List<string>();
        }

        public void Log<TState>(LogLevel logLevel, 
        						EventId eventId,
        						TState state,
        						Exception exception,
        						Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            Logs.Add(message);
        }
    }

    public class Tests
    {
        ILogger Logger = new ListLogger();

        public class PostBody
        {
            public string name = "bob";
        }

        [Test]
        public async Task Test1()
        {
            var httpRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = HttpMethods.Post
            };

            string json = JsonConvert.SerializeObject(new PostBody());
            var memoryStream = new MemoryStream();

            using var writer = new StreamWriter(memoryStream);
            await writer.WriteAsync(json);
            await writer.FlushAsync();
            memoryStream.Position = 0;

            httpRequest.Body = memoryStream;

            var response = (OkObjectResult) await Function1.Run(httpRequest, Logger);

            Assert.True(((string)response.Value).Contains("bob"));
        }
    }
}