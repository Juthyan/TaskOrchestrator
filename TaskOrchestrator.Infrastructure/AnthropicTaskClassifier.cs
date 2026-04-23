using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using TaskOrchestrator.Application;

public class AnthropicTaskClassifier(AnthropicClient client) : ITaskClassifier
{
    private readonly AnthropicClient _client = client;

    public async Task<string> ClassifyAsync(string description, CancellationToken ct = default)
{
    var message = await _client.Messages.GetClaudeMessageAsync(new MessageParameters
    {
        Model = AnthropicModels.Claude45Haiku,
        MaxTokens = 10,
        Messages = new List<Message>
        {
            new Message
            {
                Role = RoleType.User,
                Content = new List<ContentBase>
                {
                    new TextContent
                    {
                        Text = $"""
                            Classify this task as either 'Simulation' or 'Monitoring'. 
                            Reply with ONLY one word: Simulation or Monitoring.
                            
                            Task description: {description}
                            """
                    }
                }
            }
        }
    });

    return message.Content.OfType<TextContent>().First().Text.Trim();
}
}