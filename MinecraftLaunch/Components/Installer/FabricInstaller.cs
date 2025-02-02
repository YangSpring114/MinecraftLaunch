﻿using Flurl.Http;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;

namespace MinecraftLaunch.Components.Installer;

public sealed class FabricInstaller : InstallerBase {
    private readonly string _customId;
    private readonly FabricBuildEntry _fabricBuildEntry;
    private readonly DownloaderConfiguration _configuration;

    public override GameEntry InheritedFrom { get; set; }

    public FabricInstaller(FabricBuildEntry entry, string customId = default, DownloaderConfiguration configuration = default) {
        _customId = customId;
        _fabricBuildEntry = entry;
        _configuration = configuration;
    }

    public FabricInstaller(GameEntry inheritedFrom, FabricBuildEntry entry, string customId = default, DownloaderConfiguration configuration = default) {
        _customId = customId;
        _fabricBuildEntry = entry;
        _configuration = configuration;
        InheritedFrom = inheritedFrom;
    }

    public override async Task<bool> InstallAsync(CancellationToken cancellation = default) {
        /*
         * Parse build
         */
        cancellation.ThrowIfCancellationRequested();
        ReportProgress(0.0d, "Start parse build", TaskStatus.Created);
        string url = $"https://meta.fabricmc.net/v2/versions/loader/{_fabricBuildEntry.McVersion}/{_fabricBuildEntry.BuildVersion}/profile/json";
        var versionInfoNode = (await url.GetStringAsync())
            .AsNode();

        var libraries = LibrariesResolver.GetLibrariesFromJsonArray(versionInfoNode
                .GetEnumerable("libraries"),
                InheritedFrom.GameFolderPath);

        /*
         * Download dependent resources
         */
        cancellation.ThrowIfCancellationRequested();
        ReportProgress(0.25d, "Start downloading dependent resources", TaskStatus.WaitingToRun);
        await libraries.DownloadResourceEntrysAsync(_configuration, x => {
            ReportProgress(x.ToPercentage().ToPercentage(0.25d, 0.75d), $"Downloading dependent resources：{x.CompletedCount}/{x.TotalCount}",
                TaskStatus.Running);
        }, cancellation);

        /*
         * Write information to version json
         */
        cancellation.ThrowIfCancellationRequested();
        ReportProgress(0.85d, "Write information to version json", TaskStatus.WaitingToRun);
        if (!string.IsNullOrEmpty(_customId)) {
            versionInfoNode = versionInfoNode.SetString("id", _customId);
        }

        var id = versionInfoNode.GetString("id");
        var jsonFile = new FileInfo(Path.Combine(InheritedFrom.GameFolderPath,
            "versions", id, $"{id}.json"));

        if (!jsonFile.Directory.Exists) {
            jsonFile.Directory.Create();
        }

        File.WriteAllText(jsonFile.FullName, versionInfoNode.ToString());

        cancellation.ThrowIfCancellationRequested();
        ReportProgress(1.0d, "Installation is complete", TaskStatus.Canceled);
        return true;
    }

    public static async ValueTask<IEnumerable<FabricBuildEntry>> EnumerableFromVersionAsync(string mcVersion) {
        string url = $"https://meta.fabricmc.net/v2/versions/loader/{mcVersion}";
        string json = await url.GetStringAsync();

        var entries = json.AsJsonEntry<IEnumerable<FabricBuildEntry>>();

        entries = entries
            .OrderByDescending(entry =>
                new Version(entry.Loader.Version.Replace(entry.Loader.Separator, "."))
            ).ToList();

        return entries;
    }
}