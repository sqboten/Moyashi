using UnityEngine;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance { get; private set; }

    [SerializeField] private GameObject hitEffectPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayHitEffect(Vector3 position)
    {
        if (hitEffectPrefab == null)
        {
            return;
        }

        GameObject effect =
            Instantiate(
                hitEffectPrefab,
                position,
                Quaternion.identity
            );

        Destroy(
            effect,
            2f
        );
    }
}