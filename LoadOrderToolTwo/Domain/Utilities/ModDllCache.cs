using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadOrderToolTwo.Domain.Utilities;
internal class ModDllCache
{
    public Version? Version { get; set; }
    public long Length { get; set; }
    public bool IsMod { get; set; }
	public string? Path { get; set; }
}
