using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Infrastructure.Services;

/// <summary>
/// OpenAI-powered LLM service for problem analysis and reflection generation.
/// </summary>
public class OpenAiLlmService : ILlmService
{
  private readonly ChatClient _chatClient;
  private readonly ILogger<OpenAiLlmService> _logger;
  private readonly string _analysisModel;
  private readonly string _reflectionModel;

  public OpenAiLlmService(IConfiguration configuration, ILogger<OpenAiLlmService> logger)
  {
    _logger = logger;

    // Check environment variable first (for Docker), then fall back to configuration
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? configuration["OpenAI:ApiKey"]
        ?? throw new InvalidOperationException("OpenAI API key not configured. Set OPENAI_API_KEY environment variable or 'OpenAI:ApiKey' in configuration.");

    _analysisModel = configuration["OpenAI:AnalysisModel"] ?? "gpt-4o-mini";
    _reflectionModel = configuration["OpenAI:ReflectionModel"] ?? "gpt-4o";

    var client = new OpenAIClient(apiKey);
    _chatClient = client.GetChatClient(_analysisModel);
  }

  public async Task<ProblemAnalysisResult> AnalyzeProblemAsync(
      string title,
      string content,
      IReadOnlyList<string> tags,
      string difficulty,
      CancellationToken ct = default)
  {
    var systemPrompt = """
      You are an expert algorithm instructor specializing in LeetCode-style interview problems.
      Your task is to analyze a coding problem and provide structured educational content.
      
      Focus on:
      1. Identifying the PRIMARY algorithmic pattern(s) needed to solve this problem
      2. Key signals/keywords in the problem that indicate which pattern to use
      3. Common mistakes or wrong approaches people take
      4. The key insight or "aha moment" needed to solve it
      
      Respond ONLY with valid JSON matching this exact structure:
      {
        "primaryPatterns": ["PatternName1", "PatternName2"],
        "secondaryPatterns": ["AlternativePattern"],
        "keySignals": [
          {
            "signal": "keyword or phrase from problem",
            "explanation": "why this indicates the pattern",
            "indicatesPattern": "PatternName"
          }
        ],
        "commonMistakes": [
          {
            "mistake": "description of wrong approach",
            "whyItFails": "why this doesn't work",
            "betterApproach": "what to do instead"
          }
        ],
        "timeComplexity": "O(n)",
        "spaceComplexity": "O(1)",
        "keyInsight": "The core insight needed to solve this problem",
        "approachExplanation": "Step-by-step explanation of the approach without code",
        "similarProblems": ["Problem Title 1", "Problem Title 2"]
      }
      
      Use these standard pattern names:
      - TwoPointers, SlidingWindow, BinarySearch, DynamicProgramming
      - DFS, BFS, Backtracking, Greedy
      - Stack, MonotonicStack, Queue, Heap
      - Trie, UnionFind, Graph, Tree
      - HashMap, Array, String, LinkedList
      - Recursion, Memoization, DivideAndConquer
      """;

    var userPrompt = $"""
      Analyze this LeetCode problem:
      
      Title: {title}
      Difficulty: {difficulty}
      Tags: {string.Join(", ", tags)}
      
      Problem Description:
      {content}
      """;

    try
    {
      var messages = new List<ChatMessage>
      {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userPrompt)
      };

      var options = new ChatCompletionOptions
      {
        Temperature = 0.3f,
        MaxOutputTokenCount = 2000
      };

      // Use the analysis model
      var client = new OpenAIClient(GetApiKey());
      var chatClient = client.GetChatClient(_analysisModel);

      var response = await chatClient.CompleteChatAsync(messages, options, ct);
      var responseText = response.Value.Content[0].Text;

      _logger.LogInformation("Analyzed problem {Title}, response length: {Length}", title, responseText.Length);

      // Parse the JSON response
      var result = ParseAnalysisResponse(responseText);
      return result with { RawResponse = responseText };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to analyze problem {Title}", title);
      throw;
    }
  }

  public async Task<ReflectionResult> GenerateReflectionAsync(
      string problemTitle,
      string problemContent,
      ProblemAnalysis analysis,
      ColdStartSubmission coldStart,
      bool wasPatternCorrect,
      int confidenceLevel,
      CancellationToken ct = default)
  {
    var confidenceLabel = confidenceLevel switch
    {
      1 => "Not Confident",
      2 => "Slightly Confident",
      3 => "Moderately Confident",
      4 => "Confident",
      5 => "Very Confident",
      _ => "Unknown"
    };

    var systemPrompt = """
      You are a supportive coding interview coach providing personalized feedback.
      Your goal is to help the user develop better pattern recognition skills.
      
      Be encouraging but honest. Focus on actionable advice.
      
      Respond ONLY with valid JSON matching this exact structure:
      {
        "feedback": "Overall feedback on their approach - 2-3 sentences",
        "correctIdentifications": ["Things they got right"],
        "missedSignals": ["Signals they should have noticed"],
        "nextTimeAdvice": "Specific advice for recognizing this pattern in the future",
        "patternTips": "Tips specific to the pattern used in this problem",
        "confidenceCalibration": "Feedback on their confidence level vs actual performance"
      }
      """;

    var userPrompt = $"""
      Problem: {problemTitle}
      
      Actual Pattern(s): {analysis.PrimaryPatterns}
      Key Signals to Notice: {analysis.KeySignals}
      
      User's Cold Start Response:
      - Identified Signals: {coldStart.IdentifiedSignals}
      - Chosen Pattern: {coldStart.ChosenPatternId}
      - Pattern Correct: {wasPatternCorrect}
      - Confidence Level: {confidenceLabel} ({confidenceLevel}/5)
      - Thinking Time: {coldStart.ThinkingDurationSeconds} seconds
      
      Please provide personalized feedback to help them improve.
      """;

    try
    {
      var messages = new List<ChatMessage>
      {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userPrompt)
      };

      var options = new ChatCompletionOptions
      {
        Temperature = 0.7f,
        MaxOutputTokenCount = 1500
      };

      // Use the reflection model (higher quality)
      var client = new OpenAIClient(GetApiKey());
      var chatClient = client.GetChatClient(_reflectionModel);

      var response = await chatClient.CompleteChatAsync(messages, options, ct);
      var responseText = response.Value.Content[0].Text;

      _logger.LogInformation("Generated reflection for attempt, response length: {Length}", responseText.Length);

      // Parse the JSON response
      var result = ParseReflectionResponse(responseText, wasPatternCorrect);
      return result with { RawResponse = responseText };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate reflection for problem {Title}", problemTitle);
      throw;
    }
  }

  /// <summary>
  /// New overload for the LeetCode flow (without ColdStartSubmission entity).
  /// </summary>
  public async Task<ReflectionResult> GenerateReflectionAsync(
      string problemTitle,
      IReadOnlyList<string> correctPatterns,
      IReadOnlyList<string> keySignals,
      string userChosenPattern,
      string userIdentifiedSignals,
      int confidenceLevel,
      CancellationToken ct = default)
  {
    var confidenceLabel = confidenceLevel switch
    {
      <= 20 => "Not Confident",
      <= 40 => "Slightly Confident",
      <= 60 => "Moderately Confident",
      <= 80 => "Confident",
      _ => "Very Confident"
    };

    // Determine if pattern is correct (case-insensitive match)
    var normalizedChosenPattern = userChosenPattern.ToLowerInvariant().Trim();
    var isCorrect = correctPatterns.Any(p =>
        p.ToLowerInvariant().Trim() == normalizedChosenPattern ||
        p.ToLowerInvariant().Contains(normalizedChosenPattern) ||
        normalizedChosenPattern.Contains(p.ToLowerInvariant()));

    var systemPrompt = """
      You are a supportive coding interview coach providing personalized feedback.
      Your goal is to help the user develop better pattern recognition skills.
      
      Be encouraging but honest. Focus on actionable advice.
      
      Respond ONLY with valid JSON matching this exact structure:
      {
        "feedback": "Overall feedback on their approach - 2-3 sentences",
        "correctIdentifications": ["Things they got right"],
        "missedSignals": ["Signals they should have noticed"],
        "nextTimeAdvice": "Specific advice for recognizing this pattern in the future",
        "patternTips": "Tips specific to the pattern used in this problem",
        "confidenceCalibration": "Feedback on their confidence level vs actual performance"
      }
      """;

    var userPrompt = $"""
      Problem: {problemTitle}
      
      Actual Pattern(s): {string.Join(", ", correctPatterns)}
      Key Signals to Notice: {string.Join(", ", keySignals)}
      
      User's Response:
      - Identified Signals: {userIdentifiedSignals}
      - Chosen Pattern: {userChosenPattern}
      - Pattern Correct: {isCorrect}
      - Confidence Level: {confidenceLabel} ({confidenceLevel}%)
      
      Please provide personalized feedback to help them improve.
      """;

    try
    {
      var messages = new List<ChatMessage>
      {
        new SystemChatMessage(systemPrompt),
        new UserChatMessage(userPrompt)
      };

      var options = new ChatCompletionOptions
      {
        Temperature = 0.7f,
        MaxOutputTokenCount = 1500
      };

      // Use the reflection model (higher quality)
      var client = new OpenAIClient(GetApiKey());
      var chatClient = client.GetChatClient(_reflectionModel);

      var response = await chatClient.CompleteChatAsync(messages, options, ct);
      var responseText = response.Value.Content[0].Text;

      _logger.LogInformation("Generated reflection for attempt, response length: {Length}", responseText.Length);

      // Parse the JSON response
      var result = ParseReflectionResponse(responseText, isCorrect);
      return result with { RawResponse = responseText };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate reflection for problem {Title}", problemTitle);
      throw;
    }
  }

  private string GetApiKey()
  {
    // This is a bit of a hack - in production you'd inject IConfiguration
    // For now, we'll re-read from environment
    return Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        ?? throw new InvalidOperationException("OpenAI API key not found");
  }

  private static ProblemAnalysisResult ParseAnalysisResponse(string responseText)
  {
    try
    {
      // Clean the response (remove markdown code blocks if present)
      var jsonText = responseText;
      if (jsonText.StartsWith("```"))
      {
        var lines = jsonText.Split('\n');
        jsonText = string.Join("\n", lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
      }

      using var doc = JsonDocument.Parse(jsonText);
      var root = doc.RootElement;

      var primaryPatterns = root.GetProperty("primaryPatterns")
          .EnumerateArray()
          .Select(e => e.GetString() ?? "")
          .ToList();

      var secondaryPatterns = root.TryGetProperty("secondaryPatterns", out var secPat)
          ? secPat.EnumerateArray().Select(e => e.GetString() ?? "").ToList()
          : new List<string>();

      var keySignals = root.GetProperty("keySignals")
          .EnumerateArray()
          .Select(e => new SignalInfo(
              e.GetProperty("signal").GetString() ?? "",
              e.GetProperty("explanation").GetString() ?? "",
              e.GetProperty("indicatesPattern").GetString() ?? ""))
          .ToList();

      var commonMistakes = root.TryGetProperty("commonMistakes", out var mistakes)
          ? mistakes.EnumerateArray()
              .Select(e => new MistakeInfo(
                  e.GetProperty("mistake").GetString() ?? "",
                  e.GetProperty("whyItFails").GetString() ?? "",
                  e.TryGetProperty("betterApproach", out var ba) ? ba.GetString() ?? "" : ""))
              .ToList()
          : new List<MistakeInfo>();

      var similarProblems = root.TryGetProperty("similarProblems", out var similar)
          ? similar.EnumerateArray().Select(e => e.GetString() ?? "").ToList()
          : new List<string>();

      return new ProblemAnalysisResult(
          primaryPatterns,
          secondaryPatterns,
          keySignals,
          commonMistakes,
          root.TryGetProperty("timeComplexity", out var tc) ? tc.GetString() ?? "" : "",
          root.TryGetProperty("spaceComplexity", out var sc) ? sc.GetString() ?? "" : "",
          root.GetProperty("keyInsight").GetString() ?? "",
          root.TryGetProperty("approachExplanation", out var ae) ? ae.GetString() ?? "" : "",
          similarProblems,
          ""
      );
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to parse LLM analysis response: {ex.Message}", ex);
    }
  }

  private static ReflectionResult ParseReflectionResponse(string responseText, bool isCorrectPattern)
  {
    try
    {
      // Clean the response (remove markdown code blocks if present)
      var jsonText = responseText;
      if (jsonText.StartsWith("```"))
      {
        var lines = jsonText.Split('\n');
        jsonText = string.Join("\n", lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
      }

      using var doc = JsonDocument.Parse(jsonText);
      var root = doc.RootElement;

      var correctIdentifications = root.TryGetProperty("correctIdentifications", out var ci)
          ? ci.EnumerateArray().Select(e => e.GetString() ?? "").ToList()
          : new List<string>();

      var missedSignals = root.TryGetProperty("missedSignals", out var ms)
          ? ms.EnumerateArray().Select(e => e.GetString() ?? "").ToList()
          : new List<string>();

      return new ReflectionResult(
          root.GetProperty("feedback").GetString() ?? "",
          correctIdentifications,
          missedSignals,
          root.TryGetProperty("nextTimeAdvice", out var nta) ? nta.GetString() ?? "" : "",
          root.TryGetProperty("patternTips", out var pt) ? pt.GetString() ?? "" : "",
          root.TryGetProperty("confidenceCalibration", out var cc) ? cc.GetString() ?? "" : "",
          isCorrectPattern,
          ""
      );
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to parse LLM reflection response: {ex.Message}", ex);
    }
  }
}
