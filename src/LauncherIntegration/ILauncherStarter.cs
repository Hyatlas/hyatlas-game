namespace HyatlasGame.Core.LauncherIntegration
{
    /// <summary>
    /// Schnittstelle für das Starten des externen Launchers.
    /// </summary>
    public interface ILauncherStarter
    {
        /// <summary>
        /// Startet den Launcher mit dem angegebenen Core-Pfad und Argumenten.
        /// </summary>
        /// <param name="coreExecutablePath">Pfad zum Core-Executable.</param>
        /// <param name="args">Argumente für den Launcher.</param>
        void StartLauncher(string coreExecutablePath, string[] args);
    }
}
