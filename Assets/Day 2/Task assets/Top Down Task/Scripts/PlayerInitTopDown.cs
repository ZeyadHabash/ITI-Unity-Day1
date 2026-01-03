using UnityEngine;

namespace TopDown
{
    public class PlayerInitTopDown : MonoBehaviour
    {
        [SerializeField] private GameObject player;

        private void Start()
        {
            Instantiate(player, transform.position, Quaternion.identity);
        }
    }
}
