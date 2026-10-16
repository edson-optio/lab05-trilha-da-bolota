using UnityEngine;

namespace TrilhaDaBolota
{
    public enum CollectibleType { Bolota, Morango, Coracao }

    // Item coletável. As bolotas são obrigatórias: juntar todas conclui a fase.
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        public CollectibleType type;
        public float floatHeight = .12f;

        Vector3 origin;
        bool collected;

        public bool Required => type == CollectibleType.Bolota;

        void Start() => origin = transform.position;

        // Flutua devagar para chamar a atenção (a animação de quadros fica no SpriteAnimator).
        void Update()
        {
            transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * 3f + origin.x) * floatHeight);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || other.GetComponent<PlayerController>() == null) return;
            collected = true;
            LevelManager.Instance.Collect(this);
            Destroy(gameObject);
        }
    }
}
