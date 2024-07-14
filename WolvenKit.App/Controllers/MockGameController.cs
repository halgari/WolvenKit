using System;
using System.Threading.Tasks;
using WolvenKit.App.Models;
using WolvenKit.Common;
using WolvenKit.Core.Interfaces;

namespace WolvenKit.App.Controllers;

public class MockGameController : IGameController
{
    public bool AddToMod(IGameFile file, string? fileExtension) => throw new NotImplementedException();
    public Task<bool> AddFileToModModal(IGameFile file) => throw new NotImplementedException();

    public Task<bool> AddFileToModModal(IGameFile file, ArchiveManagerScope searchScope) =>
        throw new NotImplementedException();

    public Task<bool> AddFileToModModal(ulong hash) => throw new NotImplementedException();

    public Task<bool> AddFileToModModal(ulong hash, ArchiveManagerScope searchScope) =>
        throw new NotImplementedException();

    public bool AddToMod(ulong hash, ArchiveManagerScope searchScope, string? fileExtension) => throw new NotImplementedException();

    public bool AddToMod(IGameFile file, ArchiveManagerScope searchScope, string? fileExtension) => throw new NotImplementedException();
    public bool AddToMod(ulong hash, string? fileExtension) => throw new NotImplementedException();
    public async Task HandleStartup() => await Task.CompletedTask;
    public Task<bool> LaunchProject(LaunchProfile profile) => throw new NotImplementedException();
    public Task<bool> InstallProjectHot() => throw new NotImplementedException();
    public bool CleanAll(bool isPostBuild = false) => throw new NotImplementedException();
}
