using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    public static class VisualMaterialUtility
    {
        public const string LitShaderName = "Universal Render Pipeline/Lit";
        public const string SimpleLitShaderName = "Universal Render Pipeline/Simple Lit";
        public const string UnlitShaderName = "Universal Render Pipeline/Unlit";
        public const string ParticleUnlitShaderName = "Universal Render Pipeline/Particles/Unlit";
        public const string ErrorShaderName = "Hidden/InternalErrorShader";

        public static Shader ResolveOpaqueShader()
        {
            return Shader.Find(LitShaderName)
                ?? Shader.Find(SimpleLitShaderName)
                ?? Shader.Find(UnlitShaderName)
                ?? Resources.Load<Material>("RB_URP_Lit_Reference")?.shader;
        }

        public static Shader ResolveParticleShader()
        {
            return Shader.Find(ParticleUnlitShaderName)
                ?? Shader.Find(UnlitShaderName)
                ?? Resources.Load<Material>("RB_URP_Particle_Reference")?.shader
                ?? ResolveOpaqueShader();
        }

        public static bool IsInvalidGameplayShader(Shader shader)
        {
            return shader == null
                || !shader.isSupported
                || shader.name == ErrorShaderName
                || shader.name == "Standard"
                || !shader.name.StartsWith("Universal Render Pipeline/");
        }

        public static bool IsLikelyErrorColor(Color color)
        {
            return color.r > 0.92f
                && color.g < 0.18f
                && color.b > 0.92f
                && color.a > 0.85f;
        }

        public static Material CreateRuntimeMaterial(
            string materialName,
            Color color,
            Shader shader = null)
        {
            shader ??= ResolveOpaqueShader();
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            var material = new Material(shader)
            {
                name = materialName,
                hideFlags = HideFlags.DontSave
            };
            ApplyColor(material, color);
            return material;
        }

        public static Material CreateRuntimeTexturedMaterial(
            string materialName,
            Color color,
            Texture texture,
            Vector2 tiling,
            Shader shader = null)
        {
            var material = CreateRuntimeMaterial(materialName, color, shader);
            ApplyTexture(material, texture, tiling);
            return material;
        }

        public static Material CreateRuntimeUnlitTexturedMaterial(
            string materialName,
            Color color,
            Texture texture,
            Vector2 tiling)
        {
            return CreateRuntimeTexturedMaterial(
                materialName,
                color,
                texture,
                tiling,
                Shader.Find(UnlitShaderName));
        }

        public static Material CreateRuntimeTransparentMaterial(
            string materialName,
            Color color,
            Shader shader = null)
        {
            var material = CreateRuntimeMaterial(materialName, color, shader);
            ConfigureTransparent(material);
            return material;
        }

        public static Material CreateRuntimeTransparentTexturedMaterial(
            string materialName,
            Color color,
            Texture texture,
            Vector2 tiling,
            Shader shader = null)
        {
            var material = CreateRuntimeTransparentMaterial(materialName, color, shader);
            ApplyTexture(material, texture, tiling);
            return material;
        }

        public static Material CreateRuntimeEmissiveMaterial(
            string materialName,
            Color color,
            float intensity = 1.4f,
            Shader shader = null)
        {
            var material = CreateRuntimeMaterial(materialName, color, shader);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(0f, intensity));
            }

            return material;
        }

        public static Material CreateRuntimeEmissiveTexturedMaterial(
            string materialName,
            Color color,
            Texture texture,
            Vector2 tiling,
            float intensity = 1.4f,
            Shader shader = null)
        {
            var material = CreateRuntimeEmissiveMaterial(materialName, color, intensity, shader);
            ApplyTexture(material, texture, tiling);
            return material;
        }

        public static void ApplyColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        public static void ApplyTexture(Material material, Texture texture, Vector2 tiling)
        {
            if (material == null || texture == null)
            {
                return;
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", tiling);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
                material.SetTextureScale("_MainTex", tiling);
            }
        }

        private static void ConfigureTransparent(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_Blend"))
            {
                material.SetFloat("_Blend", 0f);
            }

            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
        }
    }
}
