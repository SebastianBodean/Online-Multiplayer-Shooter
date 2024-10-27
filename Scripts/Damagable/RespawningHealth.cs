using System.Collections;
using UnityEngine;

public class RespawningHealth : SimpleHealth
{
    [SerializeField] private float respawnTime = 3f;

    private Renderer rend;
    private Collider damageCollider;
    private Rigidbody rb;
    private void Awake()
    {
        CreateHealthBar();

        rend = GetComponent<Renderer>();
        damageCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        health.Value = maxHealth.Value;
    }

    public override void Damage(float damage, Vector3 point)
    {
        Vector3 impactDir = (transform.position - point).normalized;

        print(impactDir);
        print(point);
        print(damage);

        rb.AddForceAtPosition(impactDir * damage * 5, point);
        base.Damage(damage, point);
    }

    protected override void OnDeath()
    {
        DeathEffect?.Invoke();
        rend.enabled = false;
        damageCollider.enabled = false;
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        rend.enabled = true;
        damageCollider.enabled = true;
        health.Value = maxHealth.Value;
    }
}
