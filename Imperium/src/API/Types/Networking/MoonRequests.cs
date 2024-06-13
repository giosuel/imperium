// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using UnityEngine;

#endregion

namespace Imperium.API.Types.Networking;

public readonly struct ChangeWeatherRequest
{
    [SerializeField] public int LevelIndex { get; init; }
    [SerializeField] public LevelWeatherType WeatherType { get; init; }
}