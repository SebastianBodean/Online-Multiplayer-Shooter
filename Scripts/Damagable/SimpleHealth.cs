using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// A simple health system. Mainly used as a foundation for other more complex systems
/// </summary>
public class SimpleHealth : NetworkBehaviour
{
    public NetworkVariable<int> maxHealth = new(100);
    public NetworkVariable<float> health = new();

    private HealthBar healthBar;
    private void Awake()
    {
        CreateHealthBar();

        health.Value = maxHealth.Value;
    }

    protected void CreateHealthBar()
    {
        healthBar = transform.AddComponent<HealthBar>();

        health.OnValueChanged += ((previousValue, newValue) =>
        {
            healthBar.SetHealth(maxHealth.Value, newValue, previousValue);
        });
    }

    public virtual void Damage(float damage, Vector3 point)
    {
        //We limit the damage so it never goes below 0
        health.Value -= Mathf.Min(damage, health.Value);
        OnDamage();
    }

    private void OnDamage()
    {
        if (health.Value <= 0)
            OnDeath();
    }

    /// <summary>
    /// This event allows other components to add different effects to the target's death
    /// </summary>
    public System.Action DeathEffect;

    protected virtual void OnDeath()
    {
        DeathEffect?.Invoke();
        Destroy(gameObject);
    }
}
