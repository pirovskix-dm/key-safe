using KeySafe.ViewModels.Dependencies;
using KeySafe.ViewModels.ViewModels;

namespace KeySafe.ViewModels.Mappers;

public static class SafeItemEditResultMapper
{
    public static SafeItemViewModel ToSafeItemViewModel(this SafeItemEditResult safeItemEditResult)
    {
        var safeItemViewModel = new SafeItemViewModel();
        safeItemEditResult.ToSafeItemViewModel(ref safeItemViewModel);
        return safeItemViewModel;
    }
    
    public static void ToSafeItemViewModel(this SafeItemEditResult safeItemEditResult, ref SafeItemViewModel safeItemViewModel)
    {
        safeItemViewModel.Name = safeItemEditResult.Name;
        safeItemViewModel.Login = safeItemEditResult.Login;
        safeItemViewModel.Password = safeItemEditResult.Password;
        safeItemViewModel.Web = safeItemEditResult.Web;
        safeItemViewModel.Note = safeItemEditResult.Note;
    }
}