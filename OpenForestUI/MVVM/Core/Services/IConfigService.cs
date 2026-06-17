namespace OpenForestUI.MVVM.Core.Services
{
    /// <summary>
    /// Thin, testable seam over the static <c>ConfigController</c>. View-models inject this instead
    /// of touching <c>ConfigController.Component.*</c> directly, which keeps a future out-of-process
    /// (web) menu reachable: that menu would reimplement this interface over HTTP/WS while the
    /// view-models stay unchanged. The Pilot-1 surface is the three component "active" toggles the
    /// Home landing page drives; it expands with the Ingame surface in Pilot 2 and the App/Settings
    /// surface in the later Settings pilot.
    /// </summary>
    internal interface IConfigService
    {
        bool PickBanActive { get; set; }
        bool IngameActive { get; set; }
        bool PostGameActive { get; set; }

        /// <summary>Persist the component config to <c>./Config/Component.json</c>.</summary>
        void Save();
    }
}
