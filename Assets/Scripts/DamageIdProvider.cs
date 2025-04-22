using System.Collections.Generic;

public class DamageIdProvider
{
    private Dictionary<string, int> damageCounters = new Dictionary<string, int>();

    /// <summary>
    /// Belirtilen damageId için sayaç artırılır. Eğer damageId daha önce yoksa eklenir.
    /// </summary>
    /// <param name="damageId">Hasar kaynağının tagi veya adı</param>
    /// <returns>İlgili damageId'nin mevcut sayaç değeri</returns>
    public string Add(string damageId)
    {
        if (!damageCounters.ContainsKey(damageId))
        {
            damageCounters[damageId] = 1;
        }
        else
        {
            damageCounters[damageId]++;
        }

        return damageId + damageCounters[damageId];
    }

    /// <summary>
    /// Belirtilen damageId için sayaç azaltılır. Sayaç 0'a düşerse true döner.
    /// </summary>
    /// <param name="damageId">Hasar kaynağının tagi veya adı</param>
    /// <returns>Sayaç 0'a düştüyse true, aksi halde false</returns>
    public bool Remove(string damageId)
    {
        if (!damageCounters.ContainsKey(damageId))
            return true; // Mevcut değilse zaten kaldırılmıştır.

        damageCounters[damageId]--;

        if (damageCounters[damageId] <= 0)
        {
            damageCounters.Remove(damageId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Mevcut sayaç değerini döner.
    /// </summary>
    public string GetCount(string damageId)
    {
        return damageId + (damageCounters.TryGetValue(damageId, out int count) ? count.ToString() : "0");
    }
}
