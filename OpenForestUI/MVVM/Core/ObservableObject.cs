namespace OpenForestUI.MVVM.Core
{
    /// <summary>
    /// Shim that re-points the project's hand-rolled base onto CommunityToolkit.Mvvm's
    /// <see cref="CommunityToolkit.Mvvm.ComponentModel.ObservableObject"/>. The toolkit base
    /// already exposes the same <c>protected void OnPropertyChanged([CallerMemberName] string?)</c>
    /// overload (plus <c>SetProperty</c>), so every existing <c>: ObservableObject</c> view-model
    /// compiles unchanged. ViewModels opt in to <c>[ObservableProperty]</c>/<c>[RelayCommand]</c>
    /// source generators per page (requires <c>partial class</c>); this shim is removed once all
    /// view-models reference the toolkit base directly.
    /// </summary>
    public class ObservableObject : CommunityToolkit.Mvvm.ComponentModel.ObservableObject { }
}
