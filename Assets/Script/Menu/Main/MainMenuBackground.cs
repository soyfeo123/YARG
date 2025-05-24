using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using YARG.Core.Logging;
using YARG.Venue;

namespace YARG.Menu.Main
{
    public class MainMenuBackground : MonoBehaviour
    {
        [SerializeField]
        private Transform _cameraContainer;
        [SerializeField]
        private Camera _camera;

        private async UniTaskVoid Start()
        {
            _cameraContainer.transform.position = new Vector3(0, 2f, 0);

            // DELUXE: load venue

            DEMANDNewVenue();
        }
        GameObject bgInstance;
        public async UniTaskVoid DEMANDNewVenue()
        {
            
            using var result = VenueLoader.GetVenuePathFromGlobal();
            if (result.Type != Core.Venue.BackgroundType.Yarground)
            {
                // damn it
                Debug.Log("nuh uh no custom loading");
                return;
            }

            if(bgInstance != null)
            {
                Destroy(bgInstance);
            }

            var bundle = AssetBundle.LoadFromStream(result.Stream);
            AssetBundle shaderBundle = null;

            // KEEP THIS PATH LOWERCASE
            // Breaks things for other platforms, because Unity
            var bg = (GameObject) await bundle.LoadAssetAsync<GameObject>(
                BundleBackgroundManager.BACKGROUND_PREFAB_PATH.ToLowerInvariant());

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            var metalShaders = new Dictionary<string, Shader>();

            var shaderBundleData = (TextAsset) await bundle.LoadAssetAsync<TextAsset>(
                "Assets/" + BundleBackgroundManager.BACKGROUND_SHADER_BUNDLE_NAME
            );

            if (shaderBundleData != null && shaderBundleData.bytes.Length > 0)
            {
                YargLogger.LogInfo("Loading Metal shader bundle");
                shaderBundle = await AssetBundle.LoadFromMemoryAsync(shaderBundleData.bytes);
                var allAssets = shaderBundle.LoadAllAssets<Shader>();
                foreach (var shader in allAssets)
                {
                    metalShaders.Add(shader.name, shader);
                }
            }
            else
            {
                YargLogger.LogInfo("Did not find Metal shader bundle");
            }

            // Yarground comes with shaders for dx11/dx12/glcore/vulkan
            // Metal shaders used on OSX come in this separate bundle
            // Update our renderers to use them
            var renderers = bg.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    var shaderName = material.shader.name;
                    if (metalShaders.TryGetValue(shaderName, out var shader))
                    {
                        YargLogger.LogFormatDebug("Found bundled shader {0}", shaderName);
                        // We found shader from Yarground
                        material.shader = shader;
                    }
                    else
                    {
                        YargLogger.LogFormatDebug("Did not find bundled shader {0}", shaderName);
                        // Fallback to try to find among builtin shaders
                        material.shader = Shader.Find(shaderName);
                    }
                }
            }
#endif
            bgInstance = Instantiate(bg);
            var bundleBackgroundManager = bgInstance.GetComponent<BundleBackgroundManager>();
            bundleBackgroundManager.Bundle = bundle;
            bundleBackgroundManager.ShaderBundle = shaderBundle;

            // Destroy the default camera (venue has its own)
            if(_camera != null)
            Destroy(_camera);
        }

        private void Update()
        {
            // Move the camera container down
            _cameraContainer.transform.position = Vector3.Lerp(_cameraContainer.transform.position,
                new Vector3(0, 0.5f, 0), Time.deltaTime * 1.5f);

            // Get the mouse position
            var mousePos = Mouse.current.position.ReadValue();
            mousePos = _camera.ScreenToViewportPoint(mousePos);

            // Clamp
            mousePos.x = Mathf.Clamp(mousePos.x, 0f, 1f);
            mousePos.y = Mathf.Clamp(mousePos.y, 0f, 1f);

            // Move camera with the cursor
            var transformCache = _camera.transform;
            var initialPos = transformCache.localPosition;
            transformCache.localPosition = initialPos
                .WithX(Mathf.Lerp(initialPos.x, mousePos.x / 4f, Time.deltaTime * 8f))
                .WithY(Mathf.Lerp(initialPos.y, mousePos.y / 3f - 0.25f, Time.deltaTime * 8f));
        }
    }
}