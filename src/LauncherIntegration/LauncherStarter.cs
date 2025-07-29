using System;
using System.Diagnostics;

namespace HyatlasGame.Core.LauncherIntegration
{
    /// <summary>
    /// Implementiert das Starten des externen Launchers als Prozess.
    /// </summary>
    public class LauncherStarter : ILauncherStarter
    {
        public void StartLauncher(string coreExecutablePath, string[] args)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = coreExecutablePath,
                Arguments = args is null ? "" : string.Join(' ', args),
                UseShellExecute = true
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Launcher konnte nicht gestartet werden: {ex.Message}", ex);
            }
        }
    }
}
