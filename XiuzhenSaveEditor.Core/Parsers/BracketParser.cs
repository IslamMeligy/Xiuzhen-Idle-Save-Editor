namespace XiuzhenSaveEditor.Parsers;

/// <summary>
/// Utility for parsing bracket-delimited save file formats.
/// Handles values like: [val1],[val2],[val3] or [a,b,c],[d,e,f]
/// </summary>
public static class BracketParser
{
    /// <summary>
    /// Extracts the content of each top-level bracket group from a line.
    /// E.g. "[1],[100],[abc]" -> ["1", "100", "abc"]
    /// Correctly handles nested quotes and comma-separated sub-values.
    /// </summary>
    public static List<string> ExtractGroups(string line)
    {
        var groups = new List<string>();
        if (string.IsNullOrWhiteSpace(line)) return groups;

        int depth = 0;
        int start = -1;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '[')
            {
                if (depth == 0) start = i + 1;
                depth++;
            }
            else if (c == ']')
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    groups.Add(line.Substring(start, i - start));
                    start = -1;
                }
            }
        }

        return groups;
    }

    /// <summary>
    /// Splits a bracket group content by comma, respecting quoted strings.
    /// E.g. `34,0,"Dragon Heart"` -> ["34", "0", "\"Dragon Heart\""]
    /// </summary>
    public static List<string> SplitGroup(string groupContent)
    {
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuote = false;
        char quoteChar = '"';

        for (int i = 0; i < groupContent.Length; i++)
        {
            char c = groupContent[i];

            if (inQuote)
            {
                current.Append(c);
                if (c == quoteChar) inQuote = false;
            }
            else if (c == '"' || c == '\'')
            {
                inQuote = true;
                quoteChar = c;
                current.Append(c);
            }
            else if (c == ',')
            {
                parts.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0 || parts.Count > 0)
            parts.Add(current.ToString());

        return parts;
    }

    /// <summary>
    /// Strips surrounding double or single quotes from a string value.
    /// </summary>
    public static string StripQuotes(string value)
    {
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') ||
             (value[0] == '\'' && value[^1] == '\'')))
        {
            return value[1..^1];
        }
        return value;
    }
}
