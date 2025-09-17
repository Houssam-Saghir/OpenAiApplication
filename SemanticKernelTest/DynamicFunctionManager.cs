using Microsoft.SemanticKernel;
using System.ComponentModel;

public class DynamicFunctionManager
{
    private readonly Kernel _kernel;
    private readonly Dictionary<string, KernelPlugin> _dynamicPlugins;

    public DynamicFunctionManager(Kernel kernel)
    {
        _kernel = kernel;
        _dynamicPlugins = new Dictionary<string, KernelPlugin>();
    }

    public void CreateLeaveBalanceFunction(string pluginName = "DynamicPlugin")
    {

        var parameterInfos = new[]
        {
            new { Name = "payload", Type = typeof(string), Description = "JSON payload containing leave request data" }
        };

        CreateReflectionBasedFunction(
            pluginName, 
            "Leave_Balance", 
            "Submit a leave request by consuming a web API with JSON payload",
            LeaveBalanceImplementation,
            parameterInfos.Select(p => (p.Name, p.Type, p.Description)).ToArray());
    }

    private async Task<string> LeaveBalanceImplementation(string payload)
    {
            using var httpClient = GetHttpClient();
            
            var apiEndpoint = "some api";
            
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(apiEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return $"Request submitted successfully. Response: {responseContent}";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Failed to submit request. Status: {response.StatusCode}, Error: {errorContent}";
            }
    }

    // Helper method to get HttpClient (you might want to use dependency injection instead)
    private HttpClient GetHttpClient()
    {
        var httpClient =  new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        return httpClient;
    }

    public void CreateDynamicFunction<T>(string pluginName, string functionName, string description, 
        Func<T, Task<string>> implementation, string parameterName, string parameterDescription)
    {
        var kernelFunction = KernelFunctionFactory.CreateFromMethod(
            implementation,
            functionName,
            description,
            new[] { new KernelParameterMetadata(parameterName) { Description = parameterDescription, ParameterType = typeof(T) } });

        AddFunctionToPlugin(pluginName, functionName, kernelFunction);
    }

    public void CreateMultiParameterFunction(string pluginName, string functionName, string description,
        Delegate implementation, KernelParameterMetadata[] parameters)
    {
        var kernelFunction = KernelFunctionFactory.CreateFromMethod(
            implementation,
            functionName,
            description,
            parameters);

        AddFunctionToPlugin(pluginName, functionName, kernelFunction);
    }

    public void CreateReflectionBasedFunction(string pluginName, string functionName, string description, 
        Delegate implementation, (string Name, Type Type, string Description)[] parameterInfos)
    {
        // Create parameter metadata using reflection
        var parameters = parameterInfos.Select(p => CreateParameterMetadata(p.Name, p.Type, p.Description)).ToArray();

        var kernelFunction = KernelFunctionFactory.CreateFromMethod(
            implementation,
            functionName,
            description,
            parameters);

        AddFunctionToPlugin(pluginName, functionName, kernelFunction);
    }

    private KernelParameterMetadata CreateParameterMetadata(string name, Type type, string description)
    {
        return new KernelParameterMetadata(name)
        {
            Description = description,
            ParameterType = type,
            IsRequired = !IsNullableType(type)
        };
    }

    private bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ||
               !type.IsValueType;
    }

    private void AddFunctionToPlugin(string pluginName, string functionName, KernelFunction kernelFunction)
    {
        if (!_dynamicPlugins.TryGetValue(pluginName, out var plugin))
        {
            var functions = new Dictionary<string, KernelFunction> { [functionName] = kernelFunction };
            plugin = KernelPluginFactory.CreateFromFunctions(pluginName, functions.Values);
            _dynamicPlugins[pluginName] = plugin;
            _kernel.Plugins.Add(plugin);
        }
        else
        {
            var existingFunctions = plugin.ToDictionary(f => f.Name, f => f);
            existingFunctions[functionName] = kernelFunction;

            // Remove old plugin and add updated one
            _kernel.Plugins.Remove(plugin);
            plugin = KernelPluginFactory.CreateFromFunctions(pluginName, existingFunctions.Values);
            _dynamicPlugins[pluginName] = plugin;
            _kernel.Plugins.Add(plugin);
        }

        Console.WriteLine($"Dynamic function '{functionName}' added to plugin '{pluginName}'");
    }

    public void RemoveFunction(string pluginName, string functionName)
    {
        if (_dynamicPlugins.TryGetValue(pluginName, out var plugin))
        {
            var functions = plugin.Where(f => f.Name != functionName).ToDictionary(f => f.Name, f => f);

            _kernel.Plugins.Remove(plugin);

            if (functions.Any())
            {
                plugin = KernelPluginFactory.CreateFromFunctions(pluginName, functions.Values);
                _dynamicPlugins[pluginName] = plugin;
                _kernel.Plugins.Add(plugin);
            }
            else
            {
                _dynamicPlugins.Remove(pluginName);
            }

            Console.WriteLine($"Function '{functionName}' removed from plugin '{pluginName}'");
        }
    }

    public void ListDynamicFunctions()
    {
        Console.WriteLine("Dynamic Functions:");
        foreach (var plugin in _dynamicPlugins)
        {
            Console.WriteLine($"Plugin: {plugin.Key}");
            foreach (var function in plugin.Value)
            {
                Console.WriteLine($"  - {function.Name}: {function.Description}");
            }
        }
    }
}