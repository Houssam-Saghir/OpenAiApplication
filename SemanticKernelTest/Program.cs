// Import packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Populate values from your OpenAI deployment
var modelId = "gpt-4";

var apiKey = "some key";

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId, apiKey);

// Add enterprise components
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Add static plugin
kernel.Plugins.AddFromType<LightsPlugin>("Lights");

// Create dynamic function manager
var dynamicFunctionManager = new DynamicFunctionManager(kernel);

// Create the Leave_Balance function dynamically
dynamicFunctionManager.CreateLeaveBalanceFunction("HRPlugin");


// Enable planning
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Create a history store the conversation
var history = new ChatHistory();

// Add system message explaining available functions
var systemMessage = @"You are an AI assistant with access to various plugins:

Static Functions:
- Lights plugin: get_lights, change_state
- LightsPlugin.Leave_Balance (but this is placeholder)

Dynamic Functions:
- HRPlugin.Leave_Balance: Submit leave requests with start date, end date, and leave code

You can help users with lighting control, leave requests, text formatting, and calculations.
For leave requests, use the format dd/MM/yyyy for dates.";

history.AddSystemMessage(systemMessage);

// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();


    if (string.IsNullOrEmpty(userInput)) continue;

    // Add user input
    history.AddUserMessage(userInput);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);