using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.StationReport;
using Robust.Shared.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;

namespace Content.Goobstation.Server.StationReportDiscordIntergrationSystem;

public sealed class StationReportDiscordIntergrationSystem : EntitySystem
{
    //thank you Timfa for writing this code
    private static readonly HttpClient client = new();
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private const int DiscordSoftLimit = 1800; // Omu - keep headroom under 2000

    private string? _webhookUrl;

    public override void Initialize()
    {
        base.Initialize();

        //subscribes to the endroundevent and Stationreportevent
        SubscribeLocalEvent<StationReportEvent>(OnStationReportReceived);

        // Keep track of CCVar value, update if changed
        _cfg.OnValueChanged(GoobCVars.StationReportDiscordWebHook, url => _webhookUrl = url, true);
    }

    public static string? report;

    private static readonly TagReplacement[] _replacements =
    {
        // Discord markdown replacements, these must happen BEFORE anything else!
        new(@"\*", @"\*"), // Omu, escape * so it doesn't unintentionally bold stuff in Discord (this is intentionally double-escaped in the literal string for regex replacements)
        new(@"_", @"\_"), // Omu, escape _ so it doesn't unintentionally italics stuff in Discord
        new(@"~", @"\~"), // Omu, escape ~ so it doesn't unintentionally strikethrough stuff in Discord
        new(@"`", @"\`"), // Omu, escape ` so it doesn't unintentionally codeblock stuff in Discord
        new(@"#", @"\#"), // Omu, escape # so it doesn't unintentionally header stuff in Discord
        new(@">", @"\>"), // Omu, escape > so it doesn't unintentionally quoteblock stuff in Discord
        // End of Discord markdown replacements, other stuff can come AFTER this.
        // Omu: filter out empty tags
		new(@"\[bold\] *\[/bold\]", @""),
        new(@"\[italics\] *\[/italics\]", @""),
        new(@"\[mono\] *\[/mono\]", @""),
		//Omu: end empty tags
        new(@"\[/?bold\]", @"**"),
        new(@"\[/?italics\]", @"_"), // Omu, fix the 's' that was forgotten in 'italicS'
        new(@"\[/?mono\]", @"__"),
        // new(@">", @""), // Omu, was causing issues with > escaping in the Discord markdown block
        new(@"\[h1\]", @""), // Omu, make head be replaced with empty, was # 
        new(@"\[h2\]", @""), // Omu, make head be replaced with empty, was ## 
        new(@"\[h3\]", @""), // Omu, make head be replaced with empty, was ###
        new(@"\[h4\]", @""), // Omu, make head be replaced with empty, was -# 
        new(@"\[/h[0-9]\]", @""),
        new(@"\[head=1\]", @""), // Omu, make head be replaced with empty, was # 
        new(@"\[head=2\]", @""), // Omu, make head be replaced with empty, was ## 
        new(@"\[head=3\]", @""), // Omu, make head be replaced with empty, was ### 
        new(@"\[head=4\]", @""), // Omu, make head be replaced with empty, was -# 
        new(@"\[/head\]", @""),
        new(@"\[/?color=?([#0-9a-zA-Z]+)?\]", @"") // Omu, fix colour tag regex
    };

    private void OnStationReportReceived(StationReportEvent ev)
    {
        report = ev.StationReportText;

        if (string.IsNullOrWhiteSpace(report))
            return;

        foreach (var replacement in _replacements)
            report = Regex.Replace(report, replacement.Tag, replacement.Replacement);

        // Run async without blocking
        _ = SendMessageAsync(report);
    }

    // Omu - Split Discord messages to avoid hitting the character limit
    private static IEnumerable<string> SplitDiscordMessage(string text, int chunkSize)
    {
        if (string.IsNullOrEmpty(text))
            yield break;
    
        var start = 0;
        while (start < text.Length)
        {
            var remaining = text.Length - start;
            var take = Math.Min(chunkSize, remaining);
            var end = start + take;
    
            if (end < text.Length)
            {
                // 1) Prefer a newline
                var splitAt = text.LastIndexOf('\n', end - 1, take);
                if (splitAt >= start)
                    end = splitAt + 1; // include the newline
                else
                {
                    // 2) Prefer sentence boundary (". ", "! ", "? ", "… ")
                    var bestBoundary = -1;
                    static int LastIndexOfSpan(string s, string token, int endExclusive, int searchLen)
                        => s.LastIndexOf(token, endExclusive - 1, searchLen, StringComparison.Ordinal);
    
                    var candidates = new[] { ". ", "! ", "? ", "… " };
                    foreach (var token in candidates)
                    {
                        var idx = LastIndexOfSpan(text, token, end, take);
                        if (idx >= start)
                            bestBoundary = Math.Max(bestBoundary, idx + token.Length);
                    }
    
                    if (bestBoundary >= start)
                        end = bestBoundary;
                    else
                    {
                        // 3) Fall back to last whitespace
                        var lastSpace = text.LastIndexOf(' ', end - 1, take);
                        if (lastSpace >= start)
                            end = lastSpace + 1;
                        // 4) Otherwise, hard cut at end (avoid infinite loop)
                    }
                }
            }
    
            var chunk = text.Substring(start, end - start);
            yield return chunk;
            start = end;
        }
    }
    
    private async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(_webhookUrl))
            return;
        //var payload = new { content = message };
        //var json = System.Text.Json.JsonSerializer.Serialize(payload);
        //var content = new StringContent(json, Encoding.UTF8, "application/json");

        //try
        //{
        //    var response = await client.PostAsync(_webhookUrl, content);
        //    response.EnsureSuccessStatusCode();
        //}
        //catch (Exception)
        foreach (var chunk in SplitDiscordMessage(message, DiscordSoftLimit)) // Omu - avoid hitting the Discord character limit of 200 by splitting up the payload
        {
            var payload = new { content = chunk };
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
    
            try
            {
                var response = client.PostAsync(_webhookUrl, content).GetAwaiter().GetResult(); // await doesn't seem to work properly inside a foreach
                response.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                // Optionally log
            }
        }
    }

    public struct TagReplacement
    {
        public string Tag, Replacement;
        public TagReplacement(string tag, string replacement)
        {
            Tag = tag;
            Replacement = replacement;
        }
    }
}
