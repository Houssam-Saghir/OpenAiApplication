using OpenAI.Chat;

namespace OpenAiApplication
{
    internal class ChatExample
    {
        #region
        internal static string GetCurrentLocation()
        {
            // Call the location API here.
            return "San Francisco";
        }

        internal static string GetCurrentWeather(string location, string unit = "celsius")
        {
            // Call the weather API here.
            return $"31 {unit}";
        }

        internal static (string, string, string) GetEmplyeeOverAge(int age)
        {
            // Call the weather API here.
            return ("Houssam", "Rami", "Rabih");
        }
        #endregion

        #region
        internal static readonly ChatTool getCurrentLocationTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetCurrentLocation),
            functionDescription: "Get the user's current location"
        );

        internal static readonly ChatTool getOverAgeEmployeesTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetEmplyeeOverAge),
            functionDescription: "Get a list of employees who are over a specific age",
            functionParameters: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "age": {
                        "type": "integer",
                        "description": "The age that the function should return the employees who are over it."
                    }
                },
                "required": [ "age" ]
            }
            """u8.ToArray())
        );

        internal static readonly ChatTool getCurrentWeatherTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetCurrentWeather),
            functionDescription: "Get the current weather in a given location",
            functionParameters: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "location": {
                        "type": "string",
                        "description": "The city and state, e.g. Boston, MA"
                    },
                    "unit": {
                        "type": "string",
                        "enum": [ "celsius", "fahrenheit" ],
                        "description": "The temperature unit to use. Infer this from the specified location."
                    }
                },
                "required": [ "location" ]
            }
            """u8.ToArray())
        );


        #endregion
    }
}
