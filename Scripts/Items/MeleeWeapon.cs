using UnityEngine;

public class MeleeWeapon : GenericItem
{
    /*[SerializeField] private float damage;
    public override void OnPrimary(ulong clientID)
    {
        base.OnPrimary(clientID);

        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject == gameObject)
                continue;

            Debug.Log(i, colliders[i]);
            if (!colliders[i].TryGetComponent(out SimpleHealth target))
                continue;

            target.Damage(damage,) 
        }
    }*/
}
