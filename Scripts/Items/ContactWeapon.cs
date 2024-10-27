using UnityEngine;

/// <summary>
/// Basic weapon which deals damage when in contact with a target
/// </summary>
public class ContactWeapon : GenericItem
{
    [SerializeField] private float damage;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent(out SimpleHealth target))
            return;

        target.Damage(damage,transform.position);
    }
}
