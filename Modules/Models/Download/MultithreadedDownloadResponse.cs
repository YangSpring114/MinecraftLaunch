﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftLaunch.Modules.Models.Download {
    public class MultithreadedDownloadResponse {
        public Dictionary<DownloadRequest, FileDownloaderResponse> FailedDownloadRequests { get; set; }

        public bool IsAllSuccess { get; set; }
    }
}
