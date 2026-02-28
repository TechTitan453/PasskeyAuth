using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PasskeyHackathon2._0.Repository
{
    public class PasskeyRepository : IPasskeyRepository
    {
        // Persist a passkey record. This implementation appends records to a CSV file
        // located in the application's base directory named "passkeys.csv".
        // Format: timestamp,username,fingerprintOrHash,domain
        public  async Task<bool> CreatePasskey(string username, byte[] fingerprintOrHash, string domain)
        {
            try
            {
                var fileName = Path.Combine(AppContext.BaseDirectory, "passkeys.csv");

                var directory = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                static string Quote(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";

                var line = string.Join(",",
                    Quote(DateTime.UtcNow.ToString("o")),
                    Quote(username),
                    Quote(Convert.ToBase64String(fingerprintOrHash)),
                    Quote(domain)
                );

                await File.AppendAllTextAsync(fileName, line + Environment.NewLine, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyPasskey(string username)
        {
            try
            {
                var fileName = Path.Combine(AppContext.BaseDirectory, "passkeys.csv");
                if (!File.Exists(fileName)) return false;

                var lines = await File.ReadAllLinesAsync(fileName, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var fields = ParseCsvLine(line);
                    if (fields.Length >= 2 && string.Equals(fields[1], username, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Very small CSV parser for our quoted CSV format. Returns fields without surrounding quotes and with doubled quotes unescaped.
        private static string[] ParseCsvLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return Array.Empty<string>();
            var list = new System.Collections.Generic.List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // peek for escaped quote
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++; // skip next quote
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            list.Add(sb.ToString());
            return list.ToArray();
        }
    }
}
