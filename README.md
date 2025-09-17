# OpenAI Application

This repository contains various examples and applications using OpenAI APIs.

## Setup

### Prerequisites
- .NET 8.0 or later
- OpenAI API Key

### Configuration

To run any of the applications, you need to set your OpenAI API key as an environment variable.

#### Option 1: Environment Variable (Recommended)
Set the `OPENAI_API_KEY` environment variable:

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=your_openai_api_key_here
```

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY="your_openai_api_key_here"
```

**macOS/Linux:**
```bash
export OPENAI_API_KEY=your_openai_api_key_here
```

#### Option 2: .env File
1. Copy `.env.example` to `.env`
2. Fill in your actual API key in the `.env` file
3. Use a library like `DotNetEnv` to load the environment variables (you'll need to add this to each project)

### Security Note

**Never commit API keys to version control!** This repository has been configured to:
- Use environment variables instead of hardcoded keys
- Ignore `.env` files in `.gitignore`
- Provide an example configuration file (`.env.example`)

## Projects

### OpenAiApplication
Main OpenAI chat application with function calling examples.

### OpenAiRAG
Retrieval-Augmented Generation (RAG) example using OpenAI Assistants API.

### SemanticKernelTest
Examples using Microsoft Semantic Kernel framework.

### SKVectorIngest
Vector database ingestion example with Redis.

### CvReader
PDF CV/Resume reader and analyzer.

### AgenticExample
Advanced agent example with GitHub integration.

## Building and Running

```bash
# Build all projects
dotnet build

# Run a specific project (example)
cd OpenAiApplication
dotnet run
```

Make sure you have set the `OPENAI_API_KEY` environment variable before running any project.