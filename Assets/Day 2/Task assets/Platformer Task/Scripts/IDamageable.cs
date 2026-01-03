using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Interface for any entity that can take damage
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Deal damage to this entity
        /// </summary>
        /// <param name="damage">Amount of damage to deal</param>
        void TakeDamage(float damage);

        /// <summary>
        /// Get the current health of this entity
        /// </summary>
        float GetCurrentHealth();

        /// <summary>
        /// Get the maximum health of this entity
        /// </summary>
        float GetMaxHealth();

        /// <summary>
        /// Check if this entity is dead
        /// </summary>
        bool IsDead();
    }
}
