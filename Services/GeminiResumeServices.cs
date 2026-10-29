using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIResumeBuilder.Services;

public class GeminiResumeService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    public GeminiResumeService(HttpClient http, IConfiguration config) { _http = http; _config = config; }

    public async Task<string> ImproveAsync(string task, string content, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["AI:GeminiApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Gemini API key is not configured. Add AI:GeminiApiKey to user secrets or appsettings.json.");
        var prompt = task switch
        {
            "summary" => "Write a concise, professional resume summary based only on these details. Do not invent facts. Return only the summary:\n",
            "bullets" => "Rewrite the following resume experience/project text into clear, achievement-focused resume bullet points. Do not invent metrics or facts. Return only the rewritten text:\n",
            "skills" => "Suggest relevant resume skills based on the following role/profile details. Separate skills with commas and do not claim the person has skills not implied by the details; label them as suggestions:\n",
            _ => "Improve this resume text for clarity, grammar, and professional tone without adding facts. Return only the improved text:\n"
        } + content;

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.4, maxOutputTokens = 700 }
        }), Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Gemini API returned {(int)response.StatusCode}. Check the API key/quota and try again.");
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("candidates")[0].GetProperty("content")
            .GetProperty("parts")[0].GetProperty("text").GetString()?.Trim()
            ?? "No text was returned. Please try again.";
    }
}
