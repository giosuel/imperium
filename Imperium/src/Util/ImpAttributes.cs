#region

using System;

#endregion

namespace Imperium.Util;

public abstract class ImpAttributes
{
    /// <summary>
    ///     Methods marked with this attribute should only ever be executed on the host. As they require server permissions.
    /// </summary>
    internal class HostOnly : Attribute;

    /// <summary>
    ///     Methods marked with this attribute will not call any Imperium RPCs.
    ///     Vanilla RPCs might still be called somewhere down the line.
    /// </summary>
    internal class LocalMethod : Attribute;

    /// <summary>
    ///     Methods marked with this attribute will call an Imperium RPC and cause other clients to change stuff as well
    /// </summary>
    internal class RemoteMethod : Attribute;

    /// <summary>
    ///     Bindings marked with this attribute won't be updated directly but instead by a slave network binding. The network
    ///     binding will ONLY update the master binding on the host and if the call comes from the host directly.
    ///     This is to make sure the configs of client's won't be changed by network updates through another client.
    /// </summary>
    internal class HostMasterBinding : Attribute;
}