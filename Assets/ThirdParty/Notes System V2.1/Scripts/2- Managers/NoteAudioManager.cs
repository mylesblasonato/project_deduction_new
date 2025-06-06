using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace NoteSystem
{
    public class NoteAudioManager : MonoBehaviour
    {
        [Header("List of Sound Effect SO's")]
        [SerializeField] private Sound[] sounds = null;

        [Header("Sound Mixer Group")]
        [SerializeField] private AudioMixerGroup mixerGroup = null;

        [Header("Should persist?")]
        [SerializeField] private bool persistAcrossScenes = true;

        public static NoteAudioManager instance;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                if (persistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }

            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.loop = s.loop;

                s.source.outputAudioMixerGroup = mixerGroup;
            }
        }

        public void Play(Sound sound)
        {
            Sound s = sounds.FirstOrDefault(item => item == sound);

            if (s == null)
            {
                Debug.LogWarning("Sound: " + sound + " not found!");
                return;
            }

            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

            s.source.Play();
        }

        public void StopPlaying(Sound sound)
        {
            Sound s = sounds.FirstOrDefault(item => item == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.Stop();
        }

        public void PausePlaying(Sound sound)
        {
            Sound s = sounds.FirstOrDefault(item => item == sound);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            s.source.volume = s.volume * (1f + Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
            s.source.pitch = s.pitch * (1f + Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));
            s.source.Pause();
        }
    }
}
