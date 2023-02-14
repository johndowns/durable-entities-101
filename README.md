# Durable Entities 101

This sample illustrates a simple use case for [Durable Entities](https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-entities) using C#.

When working with blob storage, it's common to want to wait until a certain condition is met before processing the data. In this example, the processing is triggered when either there are 15 files in the blob folder, or more than 15 minutes has elapsed since the time the last file was written.

This is NOT a production-ready sample. There are many edge cases that aren't accounted for here. This sample is only designed to show a simple durable entities scenario.