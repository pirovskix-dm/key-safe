using KeySafe.ViewModels.Service;
using KeySafe.ViewModels.ViewModels;

namespace KeySafe.ViewModels.Mappers;

public static class StorageItemMapper
{
    public static SafeItemViewModel ToSafeItemViewModel(this StorageItem storageItem)
    {
        return new SafeItemViewModel
        {
            Name = storageItem.Name,
            Login = storageItem.Login,
            Password = storageItem.Password,
            Web = storageItem.Web,
            Note = storageItem.Note,
        };
    }
}
