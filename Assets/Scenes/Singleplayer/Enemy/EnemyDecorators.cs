using UnityEngine;

public interface IEnemyDecorator
{
    void Decorate(GameObject enemyObj);
}

//  DECORATOR NORMAL 
public class NormalEnemyDecorator : IEnemyDecorator
{
    public void Decorate(GameObject enemyObj)
    {
        var health = enemyObj.GetComponent<EnemyHealth>();
        var movement = enemyObj.GetComponent<Enemy>();

        // Repor valores base (caso o prefab tenha sido alterado)
        health.maxHealth = 5;
        health.reward = 20;
        movement.speed = 3.0f;

        enemyObj.transform.localScale = Vector3.one; // Escala 1,1,1

        // Garante que o material volta ao original se necessário
    }
}

// DECORATOR TANQUE
public class TankEnemyDecorator : IEnemyDecorator
{
    private Material _material;

    public TankEnemyDecorator(Material mat)
    {
        _material = mat;
    }

    public void Decorate(GameObject enemyObj)
    {
        var health = enemyObj.GetComponent<EnemyHealth>();
        var movement = enemyObj.GetComponent<Enemy>();

        // Stats do Tanque
        health.maxHealth = 20;
        health.reward = 30;
        movement.speed = 3.0f;

        // Escala Maior
        enemyObj.transform.localScale = new Vector3(2f, 2f, 2f);

        // Troca o Material
        var rend = enemyObj.GetComponentInChildren<Renderer>();
        if (rend != null && _material != null) rend.material = _material;
    }
}