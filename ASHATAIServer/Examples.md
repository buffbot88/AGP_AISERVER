# ASHATAIServer Example Client

This directory contains examples for connecting to ASHATAIServer.

## Quick Test with curl

### Health Check
```bash
curl http://localhost:8088/api/ai/health
```

### Get Status
```bash
curl http://localhost:8088/api/ai/status
```

### Process a Prompt
```bash
curl -X POST http://localhost:8088/api/ai/process \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hello, ASHAT! Tell me about yourself."}'
```

### Scan for Models
```bash
curl -X POST http://localhost:8088/api/ai/models/scan
```

## C# Client Example

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:8088") };

// Check health
var health = await client.GetFromJsonAsync<dynamic>("/api/ai/health");
Console.WriteLine($"Server Status: {health.status}");

// Get model status
var status = await client.GetFromJsonAsync<dynamic>("/api/ai/status");
Console.WriteLine($"Loaded Models: {status.loadedModels.Length}");

// Process a prompt
var response = await client.PostAsJsonAsync("/api/ai/process", new
{
    prompt = "What can you help me with?"
});

var result = await response.Content.ReadFromJsonAsync<dynamic>();
Console.WriteLine($"Response: {result.response}");
```

## Integration with ASHAT Goddess Client

In your ASHAT Goddess client code:

```csharp
// In AshatBrain.cs or similar
public class AshatBrain
{
    private readonly HttpClient _client;

    public AshatBrain(string serverUrl)
    {
        _client = new HttpClient { BaseAddress = new Uri(serverUrl) };
    }

    public async Task<string> ProcessPromptAsync(string prompt)
    {
        var response = await _client.PostAsJsonAsync("/api/ai/process", new
        {
            prompt = prompt
        });

        if (!response.IsSuccessStatusCode)
        {
            return "Sorry, I'm having trouble connecting to my AI server.";
        }

        var result = await response.Content.ReadFromJsonAsync<ProcessingResult>();
        return result?.Response ?? "No response received.";
    }

    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _client.GetAsync("/api/ai/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? ModelUsed { get; set; }
    public string? Response { get; set; }
    public long ProcessingTimeMs { get; set; }
}
```

## Python Client Example

```python
import requests
import json

BASE_URL = "http://localhost:8088"

# Check health
health = requests.get(f"{BASE_URL}/api/ai/health").json()
print(f"Server Status: {health['status']}")

# Process a prompt
response = requests.post(
    f"{BASE_URL}/api/ai/process",
    json={"prompt": "Hello, ASHAT!"}
)

result = response.json()
print(f"Response: {result['response']}")
```

## JavaScript/Node.js Client Example

```javascript
const fetch = require('node-fetch'); // or use native fetch in Node 18+

const BASE_URL = 'http://localhost:8088';

async function processPrompt(prompt) {
    const response = await fetch(`${BASE_URL}/api/ai/process`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ prompt })
    });
    
    const result = await response.json();
    return result.response;
}

// Usage
processPrompt('Hello, ASHAT!').then(response => {
    console.log('Response:', response);
});
```
