using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Domain;
public interface IDlcInfo
{
	uint Id { get; }
	string Name { get; }
	string ThumbnailUrl { get; }
	DateTime ReleaseDate { get; }
	string Description { get; }
}
