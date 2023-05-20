using Extensions.Sql;

namespace SkyveApp.Domain.Compatibility;

[DynamicSqlClass("Packages")]
public class PostPackage : Package
{
	public Author? Author { get; set; }
    public bool BlackListId { get; set; }
    public bool BlackListName { get; set; }
}
