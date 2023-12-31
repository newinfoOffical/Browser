﻿using Peernet.Browser.Application.Contexts;
using Peernet.Browser.Application.Services;
using Peernet.SDK.Common;
using Peernet.SDK.Models.Domain.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Peernet.Browser.Infrastructure.Tools
{
    public class CmdRunner : IRunable, IDisposable
    {
        private readonly IStatusService apiService;
        private readonly bool fileExist;
        private readonly string processName;
        private readonly ISettingsManager settingsManager;
        private readonly IShutdownService shutdownService;
        private Process process;
        private bool wasRun;
        private static string reservedAddress;

        public CmdRunner(ISettingsManager settingsManager, IShutdownService shutdownService, IStatusService apiService)
        {
            this.settingsManager = settingsManager;
            this.shutdownService = shutdownService;
            this.apiService = apiService;
            var backend = settingsManager.Backend;
            string fullPath = Path.GetFullPath(backend);
            processName = Path.GetFileName(fullPath);
            process = new Process();

            var currentProcess = Process.GetCurrentProcess();
            process.StartInfo = new ProcessStartInfo($"{fullPath}")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(fullPath),
                Arguments = $"-webapi={reservedAddress} -apikey={settingsManager.ApiKey} -watchpid={currentProcess.Id}"
            };

            process.EnableRaisingEvents = true;
            process.Exited += OnBackendExit;
            fileExist = true;
        }

        public bool IsRunning { get; private set; }

        public static bool SelfHosted { get; private set; }

        public IStatusService ApiService => apiService;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static void ReserveAddress(ISettingsManager settingsManager)
        {
            if (settingsManager.ApiUrl == null)
            {
                reservedAddress = $"127.0.0.1:{GetFreeTcpPort()}";
                settingsManager.ApiUrl = $"http://{reservedAddress}";
                SelfHosted = true;
            }
        }

        public void Run()
        {
            if (IsRunningCheck())
            {
                IsRunning = true;
            }
            if (fileExist)
            {
                RunProcess();
                WaitForBackendApiToStart();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && process != null)
            {
                if (wasRun)
                {
                    try
                    {
                        shutdownService.Shutdown();
                    }
                    catch (Exception)
                    {
                        // handle
                    }
                    finally
                    {
                        settingsManager.ApiUrl = null;
                    }

                    for (var i = 0; i < 25 && !process.HasExited; i++)
                    {
                        Thread.Sleep(200);
                    }

                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }

                process.Dispose();
                process = null;
                IsRunning = false;
                wasRun = false;
            }
        }

        private static int GetFreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        private bool IsRunningCheck() => Process.GetProcesses().Any(x => x.ProcessName.Contains(processName));

        private void OnBackendExit(object sender, EventArgs e)
        {
            GlobalContext.IsConnected = false;
            GlobalContext.ErrorMessage = $"Backend terminated with exit code: {process.ExitCode}";
        }

        private void RunProcess()
        {
            try
            {
                process.Start();
                process.Refresh();
                IsRunning = true;
                wasRun = true;
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void WaitForBackendApiToStart()
        {
            for (var i = 0; i < 25; i++)
            {
                try
                {
                    ApiResponseStatus status = Task.Run(async () => await ApiService.GetStatus()).Result;
                    if (status is { IsConnected: true })
                    {
                        break;
                    }

                    Thread.Sleep(200);
                }
                catch (Exception)
                {
                    // Handle
                }
            }
        }
    }
}