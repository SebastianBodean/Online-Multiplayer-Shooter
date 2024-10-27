using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float damage;
    [SerializeField] private float speed;
    [SerializeField] private float initialForce;
    [SerializeField] private float gravity;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        //Initial burst
        rb.AddForce(transform.forward * initialForce, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        //Gravity
        rb.AddForce(Vector3.down * Mathf.Pow(gravity, 2));

        rb.AddForce(transform.forward * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.TryGetComponent(out SimpleHealth target))
        {
            target.Damage(damage, collision.GetContact(0).point);
        }
        Destroy(gameObject);
    }
}
