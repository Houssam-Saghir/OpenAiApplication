using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using SKVectorIngest;

namespace SKVectorIngest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


            // Replace with your values.
            var deploymentName = "text-embedding-ada-002";
            var apiKey = "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";

            // Register Azure OpenAI text embedding generation service and Redis vector store.
            var builder = Kernel.CreateBuilder()
                .AddOpenAITextEmbeddingGeneration(deploymentName, apiKey)
                .AddRedisVectorStore("localhost:6379");

            // Register the data uploader.
            builder.Services.AddSingleton<DataUploader>();

            // Build the kernel and get the data uploader.
            var kernel = builder.Build();
            var dataUploader = kernel.Services.GetRequiredService<DataUploader>();

            // Load the data.
            var textParagraphs = DocumentReader.ReadParagraphs(
                new FileStream(
                    "C:\\Users\\admin\\Downloads\\New Candidates\\vector-store-data-ingestion-input\\vector-store-data-ingestion-input.docx",
                    FileMode.Open),
                "file:///c:/vector-store-data-ingestion-input.docx");

            await dataUploader.GenerateEmbeddingsAndUpload(
               "sk-documentation",
               textParagraphs);
        }
    }
}
