﻿using System.IO.Compression;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Classes.Models.Download;
using MinecraftLaunch.Components.Authenticator;

var account = new OfflineAuthenticator("Yang114").Authenticate();
var resolver = new GameResolver(".minecraft");

var config = new LaunchConfig {
    Account = account,
    IsEnableIndependencyCore = true,
    JvmConfig = new(@"C:\Program Files\Java\jdk1.8.0_301\bin\javaw.exe") {
        MaxMemory = 1024,
    }
};

Launcher launcher = new(resolver, config);
var gameProcessWatcher = await launcher.LaunchAsync("1.12.2");

//获取输出日志
gameProcessWatcher.OutputLogReceived += (sender, args) => {
    Console.WriteLine(args.Text);
};

//检测游戏退出
gameProcessWatcher.Exited += (sender, args) => {
    Console.WriteLine("exit");  
};