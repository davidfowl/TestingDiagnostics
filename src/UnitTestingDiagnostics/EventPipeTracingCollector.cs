using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace UnitTestingDiagnostics
{
    [DataCollectorFriendlyName("eventpipe")]
    [DataCollectorTypeUri("datacollector://Microsoft/TestPlatform/Extensions/eventpipe/v1")]
    public class EventPipeTracingCollector : DataCollector
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public override void Initialize(XmlElement configurationElement, DataCollectionEvents events, DataCollectionSink dataSink, DataCollectionLogger logger, DataCollectionEnvironmentContext environmentContext)
        {
            _ = StartTracing(message => logger.LogWarning(environmentContext.SessionDataCollectionContext, message));
        }

        private async Task StartTracing(Action<string> log)
        {
            var providers = new List<EventPipeProvider>()
            {
                // Runtime Metrics
                new EventPipeProvider(
                    "System.Runtime",
                    EventLevel.Informational,
                    0,
                    new Dictionary<string, string>() {
                        { "EventCounterIntervalSec", "1" }
                    }
                ),

                // new EventPipeProvider("Microsoft-DotNETCore-SampleProfiler", EventLevel.Informational),
                new EventPipeProvider("Microsoft-Windows-DotNETRuntime", EventLevel.Informational, 65536 | 32768),//ClrTraceEventParser.Keywords.Default)

                // Activity correlation
                new EventPipeProvider("System.Threading.Tasks.TplEventSource",
                        keywords: 511,
                        eventLevel: EventLevel.LogAlways),
            };

            log($"Starting event pipe session for pid {Process.GetCurrentProcess().Id}");

            EventPipeSession session = null;
            var client = new DiagnosticsClient(Process.GetCurrentProcess().Id);

            try
            {
                session = client.StartEventPipeSession(providers);
            }
            catch (EndOfStreamException)
            {

            }
            // If the process has already exited, a ServerNotAvailableException will be thrown.
            catch (ServerNotAvailableException)
            {
            }
            catch (Exception)
            {

                // We can't even start the session, wait until the process boots up again to start another metrics thread
            }

            void StopSession()
            {
                try
                {
                    session.Stop();
                }
                catch (EndOfStreamException)
                {
                    // If the app we're monitoring exits abruptly, this may throw in which case we just swallow the exception and exit gracefully.
                }
                // We may time out if the process ended before we sent StopTracing command. We can just exit in that case.
                catch (TimeoutException)
                {
                }
                // On Unix platforms, we may actually get a PNSE since the pipe is gone with the process, and Runtime Client Library
                // does not know how to distinguish a situation where there is no pipe to begin with, or where the process has exited
                // before dotnet-counters and got rid of a pipe that once existed.
                // Since we are catching this in StopMonitor() we know that the pipe once existed (otherwise the exception would've 
                // been thrown in StartMonitor directly)
                catch (PlatformNotSupportedException)
                {
                }
                // If the process has already exited, a ServerNotAvailableException will be thrown.
                // This can always race with tye shutting down and a process being restarted on exiting.
                catch (ServerNotAvailableException)
                {
                }
            }
            var _ = _cts.Token.Register(() => StopSession());

            try
            {
                using var traceOutput = File.Create("trace.nettrace");

                await session.EventStream.CopyToAsync(traceOutput);
            }
            catch (DiagnosticsClientException)
            {

            }
            catch (Exception)
            {
                // This fails if stop is called or if the process dies
            }
            finally
            {
                session?.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            _cts.Cancel();
            base.Dispose(disposing);
        }
    }
}
