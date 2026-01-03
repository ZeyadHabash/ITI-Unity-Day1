using UnityEngine;
using UnityEngine.Events;

namespace TopDown
{
    /// <summary>
    /// Deals damage on contact - for top-down games
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ContactDamageTopDown : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private string[] targetTags = { "Player" };

        [Header("Behavior")]
        [SerializeField] private bool damageOnTrigger = false;
        [SerializeField] private bool damageOnCollision = true;
        [SerializeField] private bool destroyOnContact = false;

        [Header("Events")]
        public UnityEvent<GameObject> OnDamageDealt;

        private float cooldownTimer;
        private HealthTopDown health;

        private void Awake()
        {
            health = GetComponent<HealthTopDown>();
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!damageOnCollision) return;
            TryDealDamage(collision.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!damageOnCollision) return;
            TryDealDamage(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!damageOnTrigger) return;
            TryDealDamage(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!damageOnTrigger) return;
            TryDealDamage(other.gameObject);
        }

        private void TryDealDamage(GameObject target)
        {
            // Don't deal damage if dead
            if (health != null && health.Dead) return;

            // Check cooldown
            if (cooldownTimer > 0f) return;

            // Check if target has valid tag
            bool validTarget = false;
            foreach (string tag in targetTags)
            {
                if (target.CompareTag(tag))
                {
                    validTarget = true;
                    break;
                }
            }

            if (!validTarget) return;

            // Try to deal damage
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                cooldownTimer = cooldown;
                OnDamageDealt?.Invoke(target);

                Debug.Log($"{gameObject.name} dealt {damage} damage to {target.name}");

                if (destroyOnContact)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        public void SetTargetTags(params string[] tags)
        {
            targetTags = tags;
        }
    }
}
