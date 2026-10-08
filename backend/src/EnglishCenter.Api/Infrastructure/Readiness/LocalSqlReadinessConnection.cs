using Microsoft.Data.SqlClient;

namespace EnglishCenter.Api.Infrastructure.Readiness;

internal static class LocalSqlReadinessConnection
{
    internal static SqlConnectionStringBuilder? Prepare(string resolved, ExecutionProfile execution, bool requireCertificateValidation = false)
    {
        var builder = new SqlConnectionStringBuilder(resolved);
        if (!builder.ShouldSerialize("Encrypt") || builder.Encrypt.Equals(SqlConnectionEncryptOption.Optional) ||
            string.IsNullOrWhiteSpace(builder.InitialCatalog) || builder.UserInstance || builder.AttachDBFilename.Length != 0 ||
            (requireCertificateValidation && builder.TrustServerCertificate)) return null;
        var source = builder.DataSource;
        if (source.StartsWith("lpc:", StringComparison.OrdinalIgnoreCase)) source = source[4..];
        var parts = source.Split('\\');
        if (parts.Length != 2 || parts[0] is not ("." or "localhost" or "(local)") ||
            string.IsNullOrWhiteSpace(parts[1]) || !parts[1].All(character => char.IsAsciiLetterOrDigit(character) || character == '_')) return null;
        builder.DataSource = "lpc:.\\" + parts[1]; builder.Pooling = false; builder.Enlist = false;
        builder.ConnectRetryCount = 0; builder.PersistSecurityInfo = false;
        builder.ConnectTimeout = Math.Min(execution.SqlSeconds, (int)((execution.ReadinessMs + 999L) / 1000));
        return builder;
    }
}
