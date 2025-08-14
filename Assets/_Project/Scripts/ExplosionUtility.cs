using UnityEngine;

public static class ExplosionUtility
{
    public static void ApplyExplosionForceToChildren(Transform parent, float force, float radius, float upwardsModifier)
    {
        if (parent == null) return;

        Vector3 explosionCenter = parent.position;
        var rigidbodies = parent.GetComponentsInChildren<Rigidbody>(true);

        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(force, explosionCenter, radius, upwardsModifier, ForceMode.Impulse);
        }
    }
}