using KeySafe.ViewModels.Service;
using KeySafe.ViewModels.ViewModels;

namespace KeySafe.ViewModels.Mappers;

public static class SafeItemViewModelMapper
{
    public static StorageItem ToStorageItem(this SafeItemViewModel safeItemViewModel)
    {
        return new StorageItem
        {
            Name = safeItemViewModel.Name,
            Login = safeItemViewModel.Login,
            Password = safeItemViewModel.Password,
            Web = safeItemViewModel.Web,
            Note = safeItemViewModel.Note,
        };
    }
}