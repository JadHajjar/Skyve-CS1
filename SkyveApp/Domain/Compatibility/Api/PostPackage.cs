using Extensions.Sql;

namespace SkyveApp.Domain.Compatibility.Api;

[DynamicSqlClass("Packages")]
public class PostPackage : CrPackage
{
    public Author? Author { get; set; }
    public bool BlackListId { get; set; }
    public bool BlackListName { get; set; }
}
