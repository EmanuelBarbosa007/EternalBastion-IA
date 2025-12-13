using UnityEngine;

public class BombItem : MonoBehaviour
{
    public int damage = 5;        // Dano pedido
    public float explosionRadius = 3f; // Raio da explosão
    public GameObject explosionVFX; // (Opcional) Particulas da explosão

    // Quando algo entra no Trigger da bomba
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto é um inimigo (assegura-te que os inimigos têm a Tag "Enemy")
        if (other.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // 1. Criar efeito visual se existir
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        // 2. Detetar todos os coliders dentro do raio
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hitCollider in hitColliders)
        {
            // Tenta encontrar o script EnemyHealth que tu enviaste
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // 3. Destruir a bomba
        Destroy(gameObject);
    }

    // Para desenhar o raio da explosão no editor e facilitar o ajuste
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}