using UnityEngine;

public class PiercingBullet : MonoBehaviour
{
    private Vector3 direction;
    public float speed = 20f;
    public int damage = 40;
    public float lifetime = 10f;


    public Vector3 rotationFix = new Vector3(180, 0, 0);

    private float lifeTimer = 0f;

    public void Seek(Transform target)
    {
        Vector3 rawDirection = (target.position - transform.position);

        // Ignora a altura do alvo e da torre.
        rawDirection.y = 0f;

        direction = rawDirection.normalized;




        if (direction != Vector3.zero)
        {
            // 2. Fazemos a bala "olhar" na direção que vai viajar
            transform.rotation = Quaternion.LookRotation(direction);


            transform.Rotate(rotationFix);
        }
    }

    void Update()
    {
        // Move a bala
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth e = other.GetComponent<EnemyHealth>();
        if (e != null)
        {
            e.TakeDamage(damage);
        }
    }
}