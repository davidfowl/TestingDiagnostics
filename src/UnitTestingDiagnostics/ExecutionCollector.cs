using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace UnitTestingDiagnostics
{
    [DataCollectorFriendlyName("execution")]
    [DataCollectorTypeUri("datacollector://Microsoft/TestPlatform/Extensions/execution/v1")]
    public class ExecutionCollector : DataCollector
    {
        public override void Initialize(XmlElement configurationElement, DataCollectionEvents events, DataCollectionSink dataSink, DataCollectionLogger logger, DataCollectionEnvironmentContext environmentContext)
        {
            events.TestCaseStart += (sender, e) => logger.LogWarning(environmentContext.SessionDataCollectionContext, e.TestCaseName + " [Start]");
            events.TestCaseEnd += (sender, e) => logger.LogWarning(environmentContext.SessionDataCollectionContext, e.TestCaseName + " [Ended]");
        }
    }
}
