using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class WorldAmbience : MonoBehaviour
    {
        private AudioSource _source;
        public void Initialize(AudioClip clip,float gain)
        {
            _source=GetComponent<AudioSource>();_source.playOnAwake=false;_source.loop=true;
            _source.spatialBlend=0;_source.volume=Mathf.Clamp01(gain);_source.clip=clip;
            if(clip!=null)_source.Play();
        }
        private void Update()
        {
            if(_source==null || _source.clip==null)return;
            if(Time.timeScale==0){if(_source.isPlaying)_source.Pause();}
            else if(!_source.isPlaying)_source.UnPause();
        }
        private void OnDisable(){if(_source!=null)_source.Stop();}
    }
}
