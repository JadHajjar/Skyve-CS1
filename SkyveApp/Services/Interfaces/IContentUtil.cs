using SkyveApp.Domain;
using SkyveApp.Domain.Enums;
using SkyveApp.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyveApp.Services.Interfaces;
public interface IContentUtil
{
	void ClearDllCache();
	void ContentUpdated(string path, bool builtIn, bool workshop, bool self);
	void DeleteAll(IEnumerable<ulong> ids);
	void DeleteAll(string folder);
	bool? GetDllModCache(string path, out Version? version);
	GenericPackageState GetGenericPackageState(IPackage item);
	GenericPackageState GetGenericPackageState(IPackage item, out Package? package);
	DateTime GetLocalSubscribeTime(string path);
	DateTime GetLocalUpdatedTime(string path);
	IEnumerable<IPackage> GetReferencingPackage(ulong steamId, bool includedOnly);
	string GetSubscribedItemPath(ulong id);
	IEnumerable<string> GetSubscribedItemPaths();
	long GetTotalSize(string path);
	List<Package> LoadContents();
	void MoveToLocalFolder<T>(T item) where T : IPackage;
	void RefreshPackage(Package package, bool self);
	void SaveDllCache();
	void SetBulkEnabled(IEnumerable<Mod> mods, bool value);
	void SetBulkIncluded(IEnumerable<IPackage> packages, bool value);
	void SetDllModCache(string path, bool isMod, Version? version);
	void StartListeners();
}
