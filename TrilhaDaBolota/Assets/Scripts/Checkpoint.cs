using UnityEngine;

namespace TrilhaDaBolota
{
    // Placa de madeira: ao passar por ela, o esquilo volta a partir daqui se cair num buraco.
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() != null)
                LevelManager.Instance.SetCheckpoint(transform.position);
        }
    }
}
