using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YARG.Core.Audio;

namespace YARG.Deluxe
{
    public static class CustomSFX
    {
        public static void PlaySoundEffect(string sfxName)
        {
            StemMixer mixer = GlobalAudioHandler.LoadCustomFile(Path.Combine(Application.streamingAssetsPath, "sfx", sfxName), 1f, 0.25, SongStem.Sfx);
            mixer?.Play(true);
            mixer.SongEnd += delegate
            {
                mixer.Dispose();
            };
        }
    }
}
