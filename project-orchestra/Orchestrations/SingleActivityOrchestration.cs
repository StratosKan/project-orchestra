using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using project_orchestra.Models.Orchestration;

namespace project_orchestra.Orchestrations
{
    public static class SingleActivityOrchestration
    {
        public const string ORCHESTRATOR_NAME = nameof(SingleActivityOrchestration);
        public const string SINGLE_TASK_ACTIVITY_NAME = ORCHESTRATOR_NAME + "_" + nameof(RunActivity);

        [FunctionName(ORCHESTRATOR_NAME)]
        public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            try
            {
                AzureOrchestrationCommand command = context.GetInput<AzureOrchestrationCommand>();

                bool isCompleted = false;

                var singleActivityTask = context.CallActivityAsync<AzureOrchestrationCommand>(SINGLE_TASK_ACTIVITY_NAME, command);

                context.SetCustomStatus(new { progress = 10, description = "preparing..." });

                while (!isCompleted)
                {
                    var statusUpdateTask = context.WaitForExternalEvent<bool>("StatusUpdate"); //not implemented yet...

                    var result = await Task.WhenAny(singleActivityTask, statusUpdateTask);

                    if (result == singleActivityTask)
                    {
                        var modifiedCommand = singleActivityTask.Result;
                        log.LogInformation($"Received {SINGLE_TASK_ACTIVITY_NAME} Completed event");
                        isCompleted = true;

                        //SET CUSTOM STATUS = COMPLETED 

                        context.SetOutput(modifiedCommand);
                    }
                    else
                    {
                        //context.SetCustomStatus --- StatusUpdate obj
                    }
                }
            }
            catch(Exception ex) //top-level
            {
                log.LogError(ex,$"ORCHESTRATOR-ERROR: FAILED TO PROCESS INSTANCE: {context.InstanceId}");
                context.SetOutput("Orchestration failed: " + ex.Message);
                throw;
            }            
        }

        [FunctionName(SINGLE_TASK_ACTIVITY_NAME)]
        public static AzureOrchestrationCommand RunActivity([ActivityTrigger] AzureOrchestrationCommand command, ILogger log) //+ IDurableOrchestrationClient client
        { 
            command.FunctionVersion = "2.0.1";

            log.LogInformation($"Processed {command}");
            //client.RaiseEventAsync(command.ID)

            return command;
        }
    }
}
