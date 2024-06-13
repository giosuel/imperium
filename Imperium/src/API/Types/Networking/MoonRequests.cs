// ReSharper disable Unity.RedundantAttributeOnTarget

using UnityEngine;

namespace Imperium.API.Types.Networking;

public readonly struct ChangeWeatherRequest
{
    [SerializeField] public int LevelIndex { get; init; }
    [SerializeField] public LevelWeatherType WeatherType { get; init; }
}