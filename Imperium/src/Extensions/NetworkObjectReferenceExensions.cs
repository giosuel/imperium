using Unity.Netcode;

namespace Imperium.Extensions;

public static class NetworkObjectReferenceExensions
{
    public static bool TryGetComponent<T>(
        this NetworkObjectReference networkObjectReference,
        out NetworkObject netObj,
        out T component
    )
    {
        if (
            !networkObjectReference.TryGet(out netObj) ||
            !netObj.TryGetComponent(out component)
        )
        {
            component = default;
            return false;
        }

        return true;
    }
}