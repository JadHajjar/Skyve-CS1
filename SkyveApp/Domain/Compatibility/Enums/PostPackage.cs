using Extensions.Sql;
using SkyveApp.Domain.Compatibility.Api;

namespace SkyveApp.Domain.Compatibility.Enums;

[DynamicSqlClass("Packages")]
public class PostPackage : CrPackage
{
    public Author? Author { get; set; }
    public bool BlackListId { get; set; }
    public bool BlackListName { get; set; }
}
