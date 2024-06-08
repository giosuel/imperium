#region

using System;

#endregion

namespace Imperium.API;

public class ImperiumAPIException(string message) : Exception(message);