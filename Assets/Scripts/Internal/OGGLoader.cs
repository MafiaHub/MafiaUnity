using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MafiaUnity
{
    public static class OGGLoader
    {
        public static AudioClip ToAudioClip(string name)
        {
            var vorbis = new NVorbis.VorbisReader(GameAPI.instance.fileSystem.GetStreamFromPath(Path.Combine("sounds", name + ".ogg")), true);

            var channels = vorbis.Channels;
            var sampleRate = vorbis.SampleRate;
            var totalSamples = vorbis.TotalSamples;

            AudioClip clip = AudioClip.Create(name, (int)totalSamples, channels, sampleRate, false);

            var readBuffer = new float[channels * sampleRate / 5];
            var totalBuffer = new float[channels * totalSamples];

            int rd = 0;
            int prev = 0;
            while ((rd = vorbis.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
            {
                Buffer.BlockCopy(readBuffer, 0, totalBuffer, prev * sizeof(float), rd * sizeof(float));

                prev += rd;
            }

            clip.SetData(totalBuffer, 0);

            vorbis.Dispose();

            return clip;
        }
    }
}
