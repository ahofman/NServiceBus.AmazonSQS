﻿namespace NServiceBus.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting.Support;
    using AmazonSQS.AcceptanceTests;

    public class ConfigureEndpointSqsTransport : IConfigureEndpointTestExecution
    {
        public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
        {
            var transportConfig = configuration.UseTransport<SqsTransport>();

            transportConfig.ConfigureSqsTransport(SetupFixture.SqsQueueNamePrefix);

            var routingConfig = transportConfig.Routing();

            foreach (var publisher in publisherMetadata.Publishers)
            {
                foreach (var eventType in publisher.Events)
                {
                    routingConfig.RegisterPublisher(eventType, publisher.PublisherName);
                }
            }

            settings.TestExecutionTimeout = TimeSpan.FromSeconds(20);

            var recoverability = configuration.Recoverability();
            recoverability.DisableLegacyRetriesSatellite();
            return Task.FromResult(0);
        }

        public Task Cleanup()
        {
            // Queues are cleaned up once, globally, in SetupFixture.
            return Task.FromResult(0);
        }
    }
}