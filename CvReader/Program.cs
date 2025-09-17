using Microsoft.SemanticKernel;
using UglyToad.PdfPig;

class Program
{
    static string extractedText = string.Empty;
    static string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
        ?? throw new InvalidOperationException("OPENAI_API_KEY environment variable is required");

    static async Task Main()
    {
        string filePath = "C:\\Workspace\\Magnar\\Candidates\\Candidates-Junior Web Developer-02112022\\CV's\\reematwiresume.pdf"; // Path to your CV file
         extractedText = ExtractTextFromPdf(filePath);

        Console.WriteLine("Extracted CV Content:");
        Console.WriteLine(extractedText);

        // Process the extracted text with Semantic Kernel
        await AnalyzeCvWithSemanticKernel(extractedText);
    }

    static string ExtractTextFromPdf(string filePath)
    {
        using var document = PdfDocument.Open(filePath);
        string text = "";

        foreach (var page in document.GetPages())
        {
            text += page.Text + "\n";
        }

        return text;
    }

    static async Task AnalyzeCvWithSemanticKernel(string cvText)
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4", apiKey) 
            .Build();

        var summarizeFunction = kernel.CreateFunctionFromPrompt(
            "get the contents from {{$TestText}} to be viewed in a json format including all the details (work experience, education and skills), example: First name:Houssam, Last name:Saghir"
            );

        var arguments = new KernelArguments();
        arguments.Add("TestText", cvText);

        var summary = await summarizeFunction.InvokeAsync(kernel, arguments);
        Console.WriteLine("\n🔹 CV Summary:\n" + summary);
    }
}
