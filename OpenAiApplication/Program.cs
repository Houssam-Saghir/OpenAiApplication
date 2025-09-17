using OpenAI.Chat;
using OpenAiApplication;
using System.Text.Json;

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
    ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is required");


ChatClient client = new("gpt-4-turbo", apiKey);

#region
List<ChatMessage> messages = [new UserChatMessage("Who are the employees over 45?"),];

ChatCompletionOptions options = new()
{
    Tools = { ChatExample.getCurrentWeatherTool, ChatExample.getCurrentLocationTool, ChatExample.getOverAgeEmployeesTool },
};
#endregion

#region
bool requiresAction;

do
{
    requiresAction = false;
    ChatCompletion completion = client.CompleteChat(messages, options);

    switch (completion.FinishReason)
    {
        case ChatFinishReason.Stop:
            {
                // Add the assistant message to the conversation history.
                messages.Add(new AssistantChatMessage(completion));
                break;
            }

        case ChatFinishReason.ToolCalls:
            {
                // First, add the assistant message with tool calls to the conversation history.
                messages.Add(new AssistantChatMessage(completion));

                // Then, add a new tool message for each tool call that is resolved.
                foreach (ChatToolCall toolCall in completion.ToolCalls)
                {
                    switch (toolCall.FunctionName)
                    {
                        case nameof(ChatExample.GetCurrentLocation):
                            {
                                string toolResult = ChatExample.GetCurrentLocation();
                                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            }

                        case nameof(ChatExample.GetEmplyeeOverAge):
                            {
                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                bool hasAge = argumentsJson.RootElement.TryGetProperty("age", out JsonElement age);

                                if (!hasAge)
                                {
                                    throw new ArgumentNullException(nameof(hasAge), "The age argument is required.");
                                }

                                (string,string,string) toolResult = ChatExample.GetEmplyeeOverAge(int.Parse(age.ToString()));
                                messages.Add(new ToolChatMessage(toolCall.Id, toolResult.ToString()));
                                break;
                            }

                        case nameof(ChatExample.GetCurrentWeather):
                            {
                                using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                bool hasLocation = argumentsJson.RootElement.TryGetProperty("location", out JsonElement location);
                                bool hasUnit = argumentsJson.RootElement.TryGetProperty("unit", out JsonElement unit);

                                if (!hasLocation)
                                {
                                    throw new ArgumentNullException(nameof(location), "The location argument is required.");
                                }

                                string toolResult = hasUnit
                                    ? ChatExample.GetCurrentWeather(location.GetString(), unit.GetString())
                                    : ChatExample.GetCurrentWeather(location.GetString());
                                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                break;
                            }

                        default:
                            {
                                // Handle other unexpected calls.
                                throw new NotImplementedException();
                            }
                    }
                }

                requiresAction = true;
                break;
            }

        case ChatFinishReason.Length:
            throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

        case ChatFinishReason.ContentFilter:
            throw new NotImplementedException("Omitted content due to a content filter flag.");

        case ChatFinishReason.FunctionCall:
            throw new NotImplementedException("Deprecated in favor of tool calls.");

        default:
            throw new NotImplementedException(completion.FinishReason.ToString());
    }
} while (requiresAction);
#endregion

#region
foreach (ChatMessage message in messages)
{
    switch (message)
    {
        case UserChatMessage userMessage:
            Console.WriteLine($"[USER]:");
            Console.WriteLine($"{userMessage.Content[0].Text}");
            Console.WriteLine();
            break;

        case AssistantChatMessage assistantMessage when assistantMessage.Content.Count > 0:
            Console.WriteLine($"[ASSISTANT]:");
            Console.WriteLine($"{assistantMessage.Content[0].Text}");
            Console.WriteLine();
            break;

        case ToolChatMessage:
            // Do not print any tool messages; let the assistant summarize the tool results instead.
            break;

        default:
            break;
    }
}
#endregion

