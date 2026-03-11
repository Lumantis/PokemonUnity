using System.Collections;
using UnityEngine;
using PokemonUnity.Combat;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Plays battle visual effects (particle systems) during move execution.
    /// Uses <see cref="ObjectPool{T}"/> to reuse <see cref="ParticleSystem"/> instances.
    /// Attach to the BattleScene GameObject and assign the library and anchors.
    /// </summary>
    public class BattleVFXManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [SerializeField] private MoveVFXLibrary library;
        [SerializeField] private Transform      attackerVFXAnchor;
        [SerializeField] private Transform      defenderVFXAnchor;

        [Tooltip("Default particle prefab used when no specific VFX is defined.")]
        [SerializeField] private ParticleSystem defaultParticlePrefab;

        // ── Pool ──────────────────────────────────────────────────────────────
        private ObjectPool<ParticleSystem> _pool;

        private void Awake()
        {
            library?.Initialize();

            _pool = new ObjectPool<ParticleSystem>(
                factory:  CreateDefaultParticle,
                initialSize: 4,
                parent:   transform,
                onReturn: ps =>
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                }
            );
        }

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>
        /// Play the VFX for a move. Awaitable coroutine — yields until the effect finishes.
        /// </summary>
        /// <param name="move">The move being used.</param>
        /// <param name="attackerSide">True if attacker is the player.</param>
        public IEnumerator PlayMoveVFX(Moves move, bool attackerSide)
        {
            MoveVFXData data = library?.GetVFX(move);

            if (data == null)
            {
                // Fallback: brief flash with type colour
                yield return StartCoroutine(PlayFallbackVFX(Types.NORMAL, attackerSide
                    ? attackerVFXAnchor
                    : defenderVFXAnchor));
                yield break;
            }

            Coroutine c1 = null, c2 = null;

            if (data.playOnAttacker && attackerVFXAnchor != null)
                c1 = StartCoroutine(PlayVFXAt(data, attackerVFXAnchor,
                    data.attackerOffset, data.primaryColor));

            if (data.playOnDefender && defenderVFXAnchor != null)
                c2 = StartCoroutine(PlayVFXAt(data, defenderVFXAnchor,
                    data.defenderOffset, data.secondaryColor));

            if (c1 != null) yield return c1;
            if (c2 != null) yield return c2;
        }

        // ── Private ───────────────────────────────────────────────────────────
        private IEnumerator PlayVFXAt(MoveVFXData data, Transform anchor,
                                       Vector3 offset, Color color)
        {
            ParticleSystem ps;

            if (data.particlePrefab != null)
            {
                // Instantiate from prefab (not pooled — prefab may have children)
                ps = Instantiate(data.particlePrefab, anchor.position + offset,
                                  Quaternion.identity, transform);
                ps.transform.localScale = Vector3.one * data.scale;
            }
            else
            {
                // Use pooled generic particle
                ps = _pool.Rent();
                ps.transform.position   = anchor.position + offset;
                ps.transform.localScale = Vector3.one * data.scale;
                ApplyColor(ps, color);
            }

            ps.gameObject.SetActive(true);
            ps.Play();

            yield return new WaitForSeconds(data.duration);

            if (data.particlePrefab != null)
                Destroy(ps.gameObject);
            else
                _pool.Return(ps);
        }

        private IEnumerator PlayFallbackVFX(Types type, Transform anchor)
        {
            if (anchor == null) yield break;

            ParticleSystem ps = _pool.Rent();
            ps.transform.position = anchor.position;
            Color c = MoveVFXLibrary.GetTypeColor(type);
            ApplyColor(ps, c);
            ps.gameObject.SetActive(true);
            ps.Play();

            yield return new WaitForSeconds(0.5f);
            _pool.Return(ps);
        }

        private void ApplyColor(ParticleSystem ps, Color color)
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(color);
        }

        private ParticleSystem CreateDefaultParticle()
        {
            ParticleSystem prefab = defaultParticlePrefab;
            if (prefab == null)
            {
                // Create a minimal particle system if no prefab assigned
                var go = new GameObject("VFXParticle");
                go.SetActive(false);
                return go.AddComponent<ParticleSystem>();
            }

            ParticleSystem inst = Instantiate(prefab);
            inst.gameObject.SetActive(false);
            return inst;
        }
    }
}
