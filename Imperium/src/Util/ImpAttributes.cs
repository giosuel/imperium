#region

using System;

#endregion

namespace Imperium.Util;

public abstract class ImpAttributes
{
    /// <summary>
    ///     Methods marked with host-only will only ever be executed on the host
    /// </summary>
    internal class HostOnly : Attribute;

    /// <summary>
    ///     Methods marked with local will only change stuff locally
    /// </summary>
    internal class LocalMethod : Attribute;

    /// <summary>
    ///     Methods marked with remote will call an RPC and cause other clients to change stuff as well
    /// </summary>
    internal class RemoteMethod : Attribute;
}