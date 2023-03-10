using UnityEngine.Audio;
using UnityEngine;
using System;

public class audioManager : MonoBehaviour{
    public Sound[] sounds;
    void Awake() {
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    public void Play(string name) {
        Sound sound = Array.Find(sounds, s => s.name == name);
        sound.source.Play();
    }
}
