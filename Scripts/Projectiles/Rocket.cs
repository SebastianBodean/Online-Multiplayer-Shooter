using UnityEngine;

public class Rocket : Projectile
{
    /// <summary>
    /// Dictates the volume of the explosion.
    /// </summary>
    [SerializeField] private float explosionRadius;
    

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject == gameObject)
                continue;

            Debug.Log(i, colliders[i]);
            if (!colliders[i].TryGetComponent(out SimpleHealth target))
                continue;

            //We draw a line between the explosion and the target to find a point closer to the explosion than the centre
            //This prevents large targets from taking less damage due to their centre being farther away from the explosion
            Physics.Linecast(explosionPos, colliders[i].transform.position, out RaycastHit hit);

            //We find the distance between the explosion and the 'closest' point of the target.
            //Note that the explosion will have a small volume around the centre where it will always deal max damage
            float explosionDist = (hit.point - explosionPos).magnitude - 0.25f;

            //At point blank, it will deal full damage as the fraction will equal 0, at maximum distance, it will deal the minimum damage as the fraction will equal 1
            float falloffMultiplier = 1 - 0.75f * (explosionDist / explosionRadius);
            float explosionDamage = damage * falloffMultiplier;

            //We cut off any excess damage which could be caused by having a negative distance
            explosionDamage = explosionDamage > damage ? damage : explosionDamage;

            target.Damage(explosionDamage, explosionPos);
            string debug = string.Format("Dealing explosive damage\n Damage: {0} \n Distance: {1} \n Falloff: {2}", explosionDamage, explosionDist, falloffMultiplier);
            print(debug);
        }

        Destroy(gameObject);
    }
}
