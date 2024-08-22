#region

using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Core;

public abstract class ImpLifecycleObject : MonoBehaviour
{
    internal static T Create<T>(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected) where T : ImpLifecycleObject
    {
        var lifecycleObj = new GameObject(typeof(T).Name).AddComponent<T>();
        lifecycleObj.Init(sceneLoaded, playersConnected);

        return lifecycleObj;
    }

    private void Init(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
    {
        sceneLoaded.onTrue += OnSceneLoad;
        sceneLoaded.onFalse += OnSceneUnload;

        playersConnected.onUpdate += OnPlayersUpdate;

        Init();
    }

    protected virtual void Init()
    {
    }

    /// <summary>
    ///     Invoked after the moon has been generated and the ship starts the landing animation.
    ///     Called after <see cref="RoundManager.FinishGeneratingNewLevelClientRpc" />.
    /// </summary>
    protected virtual void OnSceneLoad()
    {
    }

    /// <summary>
    ///     Invoked after the scene has been unloaded for all players.
    ///     Called after <see cref="StartOfRound.unloadSceneForAllPlayers" />.
    /// </summary>
    protected virtual void OnSceneUnload()
    {
    }

    protected virtual void OnPlayersUpdate(int playersConnected)
    {
    }
}