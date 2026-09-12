using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avoidance.Gameplay.Visuals
{
    // Scene-local presentation only. Owns and releases the runtime volume profile.
    public sealed class WorldLightingPresentation : MonoBehaviour
    {
        private VolumeProfile profile;
        public void Initialize(ModuleEnvironmentProfile settings)
        {
            if(profile!=null)Destroy(profile);
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=settings.AmbientLight;
            RenderSettings.ambientEquatorColor=Color.Lerp(settings.AmbientLight,settings.AmbientGround,.4f);
            RenderSettings.ambientGroundColor=settings.AmbientGround;
            var volume=GetComponent<Volume>()??gameObject.AddComponent<Volume>();
            volume.isGlobal=true;volume.priority=1;
            profile=ScriptableObject.CreateInstance<VolumeProfile>();
            profile.hideFlags=HideFlags.HideAndDontSave;
            var grade=profile.Add<ColorAdjustments>(true);
            grade.postExposure.value=settings.PostExposure;
            grade.contrast.value=settings.Contrast;
            grade.saturation.value=settings.Saturation;
            var bloom=profile.Add<Bloom>(true);
            bloom.intensity.value=settings.BloomIntensity;
            bloom.threshold.value=settings.BloomThreshold;
            bloom.highQualityFiltering.value=false;
            volume.sharedProfile=profile;
        }
        private void OnDestroy()
        {
            if(profile!=null)Destroy(profile);
            // Frontend scenes use their own flat ambient color.
            RenderSettings.ambientMode=AmbientMode.Flat;
        }
    }
}
