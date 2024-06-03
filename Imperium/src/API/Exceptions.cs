using System;

namespace Imperium.API;

public class ImperiumAPIException(string message) : Exception(message);