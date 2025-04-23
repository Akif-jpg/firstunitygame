public class EnemyHealth
{
    private float enemyHealth;
    private bool isEnemyAlive;
    public EnemyHealth()
    {
        this.enemyHealth = 50f;
        this.isEnemyAlive = this.enemyHealth >= 0;
    }

    public void AddDamage(float damage)
    {
        this.enemyHealth -= damage;
        if (this.enemyHealth <= 0)
        {
            this.isEnemyAlive = false;
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