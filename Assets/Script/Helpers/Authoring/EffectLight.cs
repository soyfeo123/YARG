using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

namespace YARG.Helpers.Authoring
{
    // WARNING: Changing this could break themes or venues!
    //
    // This script is used a lot in theme creation.
    // Changing the serialized fields in this file will result in older themes
    // not working properly. Only change if you need to.

    [RequireComponent(typeof(Light))]
    public class EffectLight : MonoBehaviour
    {
        public enum Mode
        {
            Normal,
            FadeOut,
            Wavy
        }

        [Space]
        [SerializeField]
        private bool _allowColoring = true;

        [FormerlySerializedAs("_lightMode")]
        [Space]
        [SerializeField]
        private Mode _mode;

        [Space]
        [SerializeField]
        private float _fadeOutRate;

        private Light _light;

        private float _initialIntensity;
        private bool _playing;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _initialIntensity = _light.intensity;
        }

        private void Start()
        {
            // Do this here instead of in Awake because otherwise the
            // _initialIntensity would be zero for duplicated prefabs.
            _light.intensity = 0f;
        }

        private void Update()
        {
            // FadeOut mode
            /*if (_mode == Mode.FadeOut && _light.intensity > 0f)
            {
                Debug.Log($"Intensity: {_initialIntensity}, Rate: {_fadeOutRate}, Duration: {_initialIntensity / _fadeOutRate}");

                _light.DOKill();
                _light.DOIntensity(0f, _initialIntensity / _fadeOutRate).SetEase(Ease.OutBounce).SetUpdate(true);
            }*/

            // Wavy mode
            if (_mode == Mode.Wavy && _playing)
            {
                // TODO: Maybe allow customizing this?
                _light.intensity = _initialIntensity +
                   Mathf.Sin(Time.time * 30f) * 0.2f -   // boost this one
                   Mathf.Sin(Time.time * 40f) * 0.15f +  // and this one
                   Mathf.Sin(Time.time * 90f) * 0.05f;   // add another layer
            }
        }

        public void SetColor(Color c)
        {
            if (!_allowColoring) return;

            _light.color = c;
        }

        public void Play()
        {
            _light.intensity = _mode == Mode.FadeOut ? _initialIntensity * 1.5f : _initialIntensity;
            _playing = true;

            if (_mode == Mode.FadeOut && _light.intensity > 0f)
            {
                Debug.Log($"Intensity: {_initialIntensity}, Rate: {_fadeOutRate}, Duration: {_initialIntensity / _fadeOutRate}");

                _light.DOKill();
                _light.DOIntensity(0f, _initialIntensity / _fadeOutRate * 1.125f).SetEase(Ease.OutBounce, 3f, 0.3f);
            }
        }

        public void Stop()
        {
            if (_mode != Mode.FadeOut)
            {
                _light.intensity = 0f;
            }

            _playing = false;
        }
    }
}