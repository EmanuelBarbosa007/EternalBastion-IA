using UnityEngine;

public class Fireball : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float explosionRadius = 3f;
    public int damage = 50;

    public GameObject impactEffect; //efeito visual de explosão

    // Define o alvo inicial
    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // mover até o alvo
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    void HitTarget()
    {
        // efeito visual (explosão)
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // aplicar dano em área
        Explode();

        Destroy(gameObject);
    }

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider collider in colliders)
        {
            EnemyHealth e = collider.GetComponent<EnemyHealth>() ?? collider.GetComponentInParent<EnemyHealth>();
            if (e != null)
            {
                e.TakeDamage(damage);
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
