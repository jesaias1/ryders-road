using System;
using System.Collections.Generic;
using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [DisallowMultipleComponent]
    public sealed class GameplayVfxService : MonoBehaviour
    {
        private const int PrewarmCount = 4;
        private const int MaximumEmitterCount = 8;

        private readonly List<ParticleSystem> _emitters = new List<ParticleSystem>();
        private readonly Dictionary<ModuleMaterialRole, Material> _materials =
            new Dictionary<ModuleMaterialRole, Material>();

        private ModuleVisualProfile _visuals;
        private ModuleEnvironmentProfile _environment;
        private Action _onEmitterCreated;
        private int _cursor;

        public int PoolSize => _emitters.Count;

        public void Initialize(
            ModuleVisualProfile visuals,
            ModuleEnvironmentProfile environment,
            Action onEmitterCreated = null)
        {
            _visuals = visuals;
            _environment = environment;
            _onEmitterCreated = onEmitterCreated;

            for (var index = _emitters.Count; index < PrewarmCount; index++)
            {
                CreateEmitter();
            }
        }

        public void PlayPickup(Vector3 position)
        {
            Emit(position, Vector3.up, ModuleMaterialRole.CircuitLine, 10, ParticleSystemShapeType.Sphere, 0.12f, 0.3f, 1.2f, 0.065f);
            Emit(position, Vector3.up, ModuleMaterialRole.WarmAccent, 3, ParticleSystemShapeType.Sphere, 0.1f, 0.22f, 1f, 0.045f);
        }
        public void PlayFlowStreak(Vector3 position, Vector3 right, Vector3 direction)
        {
            Emit(position + right * 1.15f, direction, ModuleMaterialRole.CircuitLine, 2, ParticleSystemShapeType.Cone, 0.08f, 0.18f, 7f, 0.025f);
            Emit(position - right * 1.15f, direction, ModuleMaterialRole.CircuitLine, 2, ParticleSystemShapeType.Cone, 0.08f, 0.18f, 7f, 0.025f);
        }

        public void PlayLanding(Vector3 position, float landingSpeed)
        {
            if (landingSpeed < 3f)
            {
                return;
            }

            var count = Mathf.Clamp(Mathf.RoundToInt(4f + landingSpeed * 0.55f), 5, 12);
            Emit(
                position + Vector3.up * 0.06f,
                Vector3.up,
                ModuleMaterialRole.DecorationStone,
                count,
                ParticleSystemShapeType.Circle,
                0.26f,
                0.34f,
                0.55f,
                0.075f);

            if (landingSpeed >= 9f)
            {
                Emit(
                    position + Vector3.up * 0.08f,
                    Vector3.up,
                    ModuleMaterialRole.CircuitLine,
                    4,
                    ParticleSystemShapeType.Circle,
                    0.2f,
                    0.28f,
                    0.8f,
                    0.055f);
            }
        }

        public void PlayCrumbleWarning(Vector3 position)
        {
            Emit(
                position + Vector3.up * 0.3f,
                Vector3.up,
                ModuleMaterialRole.Crumbling,
                7,
                ParticleSystemShapeType.Box,
                0.42f,
                0.32f,
                0.75f,
                0.07f);
        }

        public void PlayCrumbleCollapse(Vector3 position)
        {
            Emit(
                position,
                Vector3.down,
                ModuleMaterialRole.Crumbling,
                12,
                ParticleSystemShapeType.Box,
                0.52f,
                0.48f,
                1.25f,
                0.1f);
            Emit(
                position + Vector3.up * 0.1f,
                Vector3.up,
                ModuleMaterialRole.CircuitLine,
                5,
                ParticleSystemShapeType.Box,
                0.3f,
                0.3f,
                0.65f,
                0.055f);
        }

        public void PlayBoost(Vector3 position, Vector3 direction)
        {
            var travel = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.forward;
            Emit(
                position + Vector3.up * 0.35f,
                travel,
                ModuleMaterialRole.Boost,
                16,
                ParticleSystemShapeType.Cone,
                0.32f,
                0.34f,
                4.2f,
                0.08f);
            Emit(
                position + Vector3.up * 0.18f,
                travel,
                ModuleMaterialRole.WarmAccent,
                5,
                ParticleSystemShapeType.Cone,
                0.2f,
                0.24f,
                2.6f,
                0.06f);
        }

        public void PlayRestore(Vector3 position)
        {
            Emit(
                position + Vector3.up * 0.12f,
                Vector3.up,
                ModuleMaterialRole.Restore,
                18,
                ParticleSystemShapeType.Circle,
                0.45f,
                0.52f,
                1.8f,
                0.075f,
                upwardVelocity: 1.6f);
            Emit(
                position + Vector3.up * 0.9f,
                Vector3.up,
                ModuleMaterialRole.CircuitLine,
                7,
                ParticleSystemShapeType.Box,
                0.34f,
                0.42f,
                0.45f,
                0.055f,
                upwardVelocity: 0.8f);
        }

        public void PlayPatch(Vector3 position)
        {
            Emit(
                position,
                Vector3.up,
                ModuleMaterialRole.Patch,
                24,
                ParticleSystemShapeType.Sphere,
                0.55f,
                0.56f,
                2.9f,
                0.1f);
            Emit(
                position,
                Vector3.up,
                ModuleMaterialRole.WarmAccent,
                10,
                ParticleSystemShapeType.Sphere,
                0.38f,
                0.4f,
                2.2f,
                0.075f);
        }

        private void Emit(
            Vector3 position,
            Vector3 direction,
            ModuleMaterialRole role,
            int baseCount,
            ParticleSystemShapeType shapeType,
            float radius,
            float lifetime,
            float speed,
            float size,
            float upwardVelocity = 0f)
        {
            var density = _environment != null ? _environment.VfxDensity : 1f;
            if (_visuals == null || density <= 0.01f)
            {
                return;
            }

            var particles = RentEmitter();
            particles.transform.SetPositionAndRotation(
                position,
                direction.sqrMagnitude > 0.01f
                    ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                    : Quaternion.identity);
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.duration = 0.25f;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = _visuals.ColorFor(role);
            main.maxParticles = 32;

            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            var count = (short)Mathf.Clamp(Mathf.RoundToInt(baseCount * density), 1, 32);
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = shapeType;
            shape.radius = radius;
            shape.radiusThickness = shapeType == ParticleSystemShapeType.Circle ? 0.15f : 1f;
            shape.angle = shapeType == ParticleSystemShapeType.Cone ? 13f : 25f;
            shape.scale = shapeType == ParticleSystemShapeType.Box
                ? new Vector3(radius * 2f, 0.08f, radius * 2f)
                : Vector3.one;

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = upwardVelocity > 0f;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = upwardVelocity;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = MaterialFor(role);
            particles.Play();
        }

        private ParticleSystem RentEmitter()
        {
            for (var index = 0; index < _emitters.Count; index++)
            {
                if (!_emitters[index].isPlaying)
                {
                    return _emitters[index];
                }
            }

            if (_emitters.Count < MaximumEmitterCount)
            {
                return CreateEmitter();
            }

            var emitter = _emitters[_cursor % _emitters.Count];
            _cursor++;
            return emitter;
        }

        private ParticleSystem CreateEmitter()
        {
            var emitterObject = new GameObject("Gameplay VFX Emitter", typeof(ParticleSystem));
            emitterObject.transform.SetParent(transform, false);
            var particles = emitterObject.GetComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _emitters.Add(particles);
            _onEmitterCreated?.Invoke();
            return particles;
        }

        private Material MaterialFor(ModuleMaterialRole role)
        {
            if (_materials.TryGetValue(role, out var material))
            {
                return material;
            }

            material = VisualMaterialUtility.CreateRuntimeMaterial(
                "RR Gameplay VFX " + role,
                _visuals.ColorFor(role),
                VisualMaterialUtility.ResolveParticleShader());
            _materials[role] = material;
            return material;
        }

        private void OnDestroy()
        {
            foreach (var material in _materials.Values)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }
    }
}
