using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public int damage = 1;


    public Vector3 rotationFix = new Vector3(180, 0, 0);

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

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // 1. Move a flecha
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        // 2. Faz a flecha olhar para o alvo
        transform.LookAt(target);

        // 3. Aplica a correção de 90 graus (localmente)
        transform.Rotate(rotationFix);
    }

    void HitTarget()
    {
        EnemyHealth e = target.GetComponent<EnemyHealth>();
        if (e != null)
        {
            e.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}