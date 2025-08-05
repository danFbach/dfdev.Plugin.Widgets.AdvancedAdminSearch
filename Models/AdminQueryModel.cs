using System.Text.Json.Serialization;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch.Models;

public class AdminQueryModel
{
    [JsonPropertyName("searchQuery")]
    public string SearchQuery { get; set; } = string.Empty;
}
