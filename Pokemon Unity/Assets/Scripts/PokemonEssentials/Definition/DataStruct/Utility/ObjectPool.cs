using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Generic object pool for Unity Component subclasses.
    /// Avoids GC pressure by reusing objects instead of creating/destroying them.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> _available = new Queue<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _onRent;
        private readonly Action<T> _onReturn;
        private readonly Transform _parent;
        private int _totalCreated;

        public int Available   => _available.Count;
        public int TotalCreated => _totalCreated;

        public ObjectPool(Func<T> factory, int initialSize = 0,
                          Transform parent = null,
                          Action<T> onRent = null, Action<T> onReturn = null)
        {
            _factory  = factory  ?? throw new ArgumentNullException(nameof(factory));
            _parent   = parent;
            _onRent   = onRent;
            _onReturn = onReturn;
            Prewarm(initialSize);
        }

        /// <summary>Pre-creates <paramref name="count"/> instances.</summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
                _available.Enqueue(CreateNew());
        }

        /// <summary>Rent an instance from the pool (creates one if empty).</summary>
        public T Rent()
        {
            T item = _available.Count > 0 ? _available.Dequeue() : CreateNew();
            _onRent?.Invoke(item);
            return item;
        }

        /// <summary>Return an instance to the pool for reuse.</summary>
        public void Return(T item)
        {
            if (item == null) return;
            _onReturn?.Invoke(item);
            _available.Enqueue(item);
        }

        private T CreateNew()
        {
            T instance = _factory();
            if (_parent != null && instance != null)
                instance.transform.SetParent(_parent, false);
            _totalCreated++;
            return instance;
        }
    }

    /// <summary>
    /// Generic object pool for plain C# objects (no Component dependency).
    /// </summary>
    public class PlainObjectPool<T> where T : class
    {
        private readonly Queue<T> _available = new Queue<T>();
        private readonly Func<T>    _factory;
        private readonly Action<T>  _onRent;
        private readonly Action<T>  _onReturn;
        private int _totalCreated;

        public int Available    => _available.Count;
        public int TotalCreated => _totalCreated;

        public PlainObjectPool(Func<T> factory, int initialSize = 0,
                               Action<T> onRent = null, Action<T> onReturn = null)
        {
            _factory  = factory ?? throw new ArgumentNullException(nameof(factory));
            _onRent   = onRent;
            _onReturn = onReturn;
            Prewarm(initialSize);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T item = _factory();
                _totalCreated++;
                _available.Enqueue(item);
            }
        }

        public T Rent()
        {
            T item = _available.Count > 0 ? _available.Dequeue() : Create();
            _onRent?.Invoke(item);
            return item;
        }

        public void Return(T item)
        {
            if (item == null) return;
            _onReturn?.Invoke(item);
            _available.Enqueue(item);
        }

        private T Create()
        {
            T item = _factory();
            _totalCreated++;
            return item;
        }
    }

    /// <summary>
    /// Specialised pool for <see cref="AudioSource"/> components.
    /// Attach to a persistent GameObject alongside <see cref="AudioManager"/>.
    /// </summary>
    public class AudioSourcePool : MonoBehaviour
    {
        [SerializeField] private int initialPoolSize = 8;
        [SerializeField] private int maxPoolSize     = 32;

        private ObjectPool<AudioSource> _pool;

        public int Available    => _pool?.Available    ?? 0;
        public int TotalCreated => _pool?.TotalCreated ?? 0;

        private void Awake()
        {
            _pool = new ObjectPool<AudioSource>(
                factory:  CreateAudioSource,
                initialSize: initialPoolSize,
                parent:   transform,
                onRent:   src => src.gameObject.SetActive(true),
                onReturn: src =>
                {
                    src.Stop();
                    src.clip   = null;
                    src.volume = 1f;
                    src.pitch  = 1f;
                    src.gameObject.SetActive(false);
                }
            );
        }

        /// <summary>Rent a ready <see cref="AudioSource"/> from the pool.</summary>
        public AudioSource Rent()
        {
            if (_pool.TotalCreated >= maxPoolSize && _pool.Available == 0)
            {
                Debug.LogWarning("[AudioSourcePool] Pool exhausted — consider increasing maxPoolSize.");
                return null;
            }
            return _pool.Rent();
        }

        /// <summary>Return a finished <see cref="AudioSource"/> to the pool.</summary>
        public void Return(AudioSource src) => _pool.Return(src);

        /// <summary>Play a clip then automatically return the source when done.</summary>
        public System.Collections.IEnumerator PlayAndReturn(AudioClip clip,
                                                             float volume = 1f,
                                                             float pitch  = 1f,
                                                             float spatialBlend = 0f)
        {
            AudioSource src = Rent();
            if (src == null) yield break;

            src.clip         = clip;
            src.volume       = volume;
            src.pitch        = pitch;
            src.spatialBlend = spatialBlend;
            src.Play();

            yield return new WaitForSeconds(clip.length / Mathf.Abs(pitch));
            Return(src);
        }

        private AudioSource CreateAudioSource()
        {
            var go = new GameObject("PooledAudioSource");
            go.SetActive(false);
            return go.AddComponent<AudioSource>();
        }
    }
}
