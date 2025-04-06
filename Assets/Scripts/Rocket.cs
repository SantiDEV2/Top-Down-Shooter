using UnityEngine;

public class Rocket : Bullets
{
    [Header("Rocket Settings")]
    public float explosionRadius = 5f;

    private void Awake()
    {
        OnHit = HandleRocketHit; 
    }

    private void HandleRocketHit(GameObject hitObject, Vector3 position, int damage)
    {
        Pawn directHitPawn = hitObject.GetComponent<Pawn>();
        if (directHitPawn != null && hitObject != owner)
        {
            ApplyDamage(directHitPawn.gameObject, position, damage);
        }

        Collider[] colliders = Physics.OverlapSphere(position, explosionRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject == owner || colliders[i].gameObject == hitObject)
                continue;

            Pawn targetPawn = colliders[i].GetComponent<Pawn>();
            if (targetPawn != null)
            {
                ApplyDamage(colliders[i].gameObject, colliders[i].transform.position, damage);
            }
        }
    }

    private void ApplyDamage(GameObject target, Vector3 position, int damage)
    {
        Pawn ownerPawn = owner?.GetComponent<Pawn>();
        if (ownerPawn != null)
        {
            ownerPawn.BulletHit(target, position, damage);
        }
        else
        {
            Pawn targetPawn = target.GetComponent<Pawn>();
            if (targetPawn != null)
            {
                targetPawn.OnDamage(damage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        OnTriggerEnter(GetComponent<Collider>());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
