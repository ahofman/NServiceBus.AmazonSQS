﻿namespace NServiceBus.Transport.SQS.CommandLine
{
    using System;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.SimpleNotificationService;
    using Amazon.SQS;
    using McMaster.Extensions.CommandLineUtils;

    static class Endpoint
    {
        public static async Task Create(IAmazonSQS sqs, CommandArgument name)
        {
            await Console.Out.WriteLineAsync($"Creating endpoint '{name.Value}'.");

            await Queue.Create(sqs, name);

            await Console.Out.WriteLineAsync($"Endpoint '{name.Value}' is ready.");
        }

        public static async Task AddLargeMessageSupport(IAmazonS3 s3, CommandArgument name, CommandArgument bucketName)
        {
            await Console.Out.WriteLineAsync($"Adding large message support to Endpoint '{name.Value}'.");

            await Bucket.Create(s3, name, bucketName);
            await Bucket.EnableCleanup(s3, name, bucketName);

            await Console.Out.WriteLineAsync($"Added large message support to Endpoint '{name.Value}'.");
        }

        public static async Task AddDelayDelivery(IAmazonSQS sqs, CommandArgument name)
        {
            await Console.Out.WriteLineAsync($"Adding delay delivery support to Endpoint '{name.Value}'.");

            await Queue.CreateDelayDelivery(sqs, name);

            await Console.Out.WriteLineAsync($"Added delay delivery support to Endpoint '{name.Value}'.");
        }

        /* public static async Task Subscribe(ManagementClient client, CommandArgument name, CommandOption topicName, CommandOption subscriptionName, CommandArgument eventType, CommandOption ruleName)
         {
             try
             {
                 await Rule.Create(client, name, topicName, subscriptionName, eventType, ruleName);
             }
             catch (MessagingEntityAlreadyExistsException)
             {
                 Console.WriteLine($"Rule '{name}' for topic '{topicName}' and subscription '{subscriptionName}' already exists, skipping creation. Verify SQL filter matches '[NServiceBus.EnclosedMessageTypes] LIKE '%{eventType.Value}%'.");
             }
         }

         public static async Task Unsubscribe(ManagementClient client, CommandArgument name, CommandOption topicName, CommandOption subscriptionName, CommandArgument eventType, CommandOption ruleName)
         {
             try
             {
                 await Rule.Delete(client, name, topicName, subscriptionName, eventType, ruleName);
             }
             catch (MessagingEntityNotFoundException)
             {
                 Console.WriteLine($"Rule '{name}' for topic '{topicName}' and subscription '{subscriptionName}' does not exist, skipping deletion");
             }
         }*/
    }

}