﻿/// <credits>
/// <author>
/// <name>Shubh Pareek</name>
/// <rollnumber>112001039</rollnumber>
/// </author>
/// </credits>

using Azure;
using Azure.Data.Tables;
using MessengerCloud;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessengerCloud
{
    /// <summary>
    /// Custom Azure functions APIs class.
    /// </summary>
    public static class EntityApi
    {
        private const string TableName = "Entities";
        private const string ConnectionName = "AzureWebJobsStorage";
        private const string Route = "entity";

        [FunctionName("CreateEntity")]
        public static async Task<IActionResult> CreateEntity(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequest req,
                [Table(TableName, Connection = ConnectionName)] IAsyncCollector<Entity> entityTable,
                ILogger log)
        {
            Trace.WriteLine("[EntityApi]: create entity called");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Debug.WriteLine("request Body is ",requestBody);
            EntityInfoWrapper info= JsonSerializer.Deserialize<EntityInfoWrapper>(requestBody);
            Entity value = new(info);
            Debug.WriteLine("val inside api ",value);
            await entityTable.AddAsync(value);
            log.LogInformation($"New entity created Id = {value.Id}.strings are ", value.Sentences[0]);

            Trace.WriteLine("[EntityApi]: entity created");
            return new OkObjectResult(value);
        }

        [FunctionName("GetEntityById")]
        public static IActionResult GetEntityById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequest req,
        [Table(TableName, Entity.PartitionKeyName, "{id}", Connection = ConnectionName)] Entity entity,
        ILogger log,
        string id)
        {
            Trace.WriteLine("[EntityApi]: get entity called");
            log.LogInformation($"Getting entity {id}");
            if (entity == null)
            {
                log.LogInformation($"Entity {id} not found");
                return new NotFoundResult();
            }
            Trace.WriteLine("[EntityApi]: entity sent");

            return new OkObjectResult(entity);
        }

        [FunctionName("GetEntities")]
        public static async Task<IActionResult> GetEntitiesAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequest req,
        [Table(TableName, Connection = ConnectionName)] TableClient tableClient,
        ILogger log)
        {
            Trace.WriteLine("[EntityApi]: get entities called");
            log.LogInformation("Getting all entity items");
            Page<Entity> page = await tableClient.QueryAsync<Entity>().AsPages().FirstAsync();
            Trace.WriteLine("[EntityApi]: entities returned");
            return new OkObjectResult(page.Values);
        }


        [FunctionName("DeleteEntity")]
        public static async Task<IActionResult> DeleteEntity(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequest req,
        [Table(TableName, ConnectionName)] TableClient entityClient,
        ILogger log,
        string id)
        {
            Trace.WriteLine("[EntityApi]: Delete entity called");
            log.LogInformation($"Deleting entity by {id}");
            try
            {
                await entityClient.DeleteEntityAsync(Entity.PartitionKeyName, id, ETag.All);
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                return new NotFoundResult();
            }

            Trace.WriteLine("[EntityApi]: Deleted entity ");
            return new OkResult();
        }

        [FunctionName("DeleteEntities")]
        public static async Task<IActionResult> DeleteEntities(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route)] HttpRequest req,
        [Table(TableName, ConnectionName)] TableClient entityClient,
        ILogger log)
        {
            Trace.WriteLine("[EntityApi]: Delete all called ");
            log.LogInformation($"Deleting all entity items");
            try
            {
                await entityClient.DeleteAsync();
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                return new NotFoundResult();
            }
            Trace.WriteLine("[EntityApi]: Deleted all ");

            return new OkResult();
        }
    }
}
