#region

using GameNetcodeStuff;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Core.Scripts;

public class ShiggyAI : EnemyAI
{
    public float detectionRadius = 100;

    private readonly Collider[] allPlayerColliders = new Collider[4];

    private float closestPlayerDist;

    private Collider tempTargetCollider;

    private MeshRenderer renderer;
    private const float FPS = 40f;

    private static bool texturesLoaded;
    private static readonly Texture2D[] frames = new Texture2D[10];

    public override void Start()
    {
        base.Start();
        movingTowardsTargetPlayer = true;

        renderer = transform.Find("Texture").GetComponent<MeshRenderer>();
        renderer.material = Instantiate(ImpAssets.ShiggyMaterial);

        if (!texturesLoaded) LoadTextures();
    }

    private static void LoadTextures()
    {
        var prefix = Random.Range(0, 4096) == 1337 ? "shinyshig" : "shig";
        for (var i = 0; i < 10; i++)
        {
            frames[i] = ImpAssets.ImperiumAssets.LoadAsset<Texture2D>($"Assets/Special/{prefix}0{i + 1}.png");
        }

        texturesLoaded = true;
    }

    public override void DoAIInterval()
    {
        if (!Imperium.IsSceneLoaded.Value || !IsOwner) return;

        var num = Physics.OverlapSphereNonAlloc(
            transform.position, detectionRadius, allPlayerColliders, StartOfRound.Instance.playersMask
        );

        if (num > 0)
        {
            closestPlayerDist = 255555f;
            for (var i = 0; i < num; i++)
            {
                var num2 = Vector3.Distance(transform.position, allPlayerColliders[i].transform.position);
                if (num2 < closestPlayerDist)
                {
                    closestPlayerDist = num2;
                    tempTargetCollider = allPlayerColliders[i];
                }
            }

            if (Vector3.Distance(tempTargetCollider.transform.position, transform.position) > 15f)
            {
                SetMovingTowardsTargetPlayer(tempTargetCollider.gameObject.GetComponent<PlayerControllerB>());
            }
            else
            {
                agent.speed = 0;
                movingTowardsTargetPlayer = false;
            }
        }
        else
        {
            agent.speed = 5f;
        }

        base.DoAIInterval();
    }

    public override void Update()
    {
        renderer.material.mainTexture = frames[(int)(Time.time * FPS) % 9];

        if (!IsOwner) return;

        if (!Imperium.IsSceneLoaded.Value)
        {
            transform.position = Imperium.StartOfRound.playerSpawnPositions[0].position + Vector3.up * 1.45f;

            var playerPosition = Imperium.Player.transform.position;
            transform.LookAt(new Vector3(playerPosition.x, transform.position.y, playerPosition.z));
            return;
        }

        if (movingTowardsTargetPlayer)
        {
            agent.speed = Mathf.Clamp(agent.speed + Time.deltaTime * 5f, 0f, 12f);
        }

        base.Update();
    }
}