using System;

public class EnemyHealth
{
    private float enemyHealth;
    private bool isEnemyAlive;
    private static readonly Random random = new Random(); // Random nesnesi

    public EnemyHealth()
    {
        this.enemyHealth = random.Next(40, 81); // 40 ile 80 arasında (80 dahil)
        this.isEnemyAlive = this.enemyHealth > 0;
    }

    public void AddDamage(float damage)
    {
        this.enemyHealth -= damage;
        if (this.enemyHealth <= 0)
        {
            this.isEnemyAlive = false;
            this.enemyHealth = 0; // Sağlık negatif görünmesin isterseniz
        }
    }

    public float GetEnemyHealth()
    {
        return this.enemyHealth;
    }

    public bool IsEnemyAlive()
    {
        return this.isEnemyAlive;
    }
}
