using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSound : MonoBehaviour
{
    public AudioClip stepSound;
    public AudioClip attackSound;
    public AudioClip jumpSound;
    public AudioClip hitSound;

    private AudioSource audioSource;

    private void Awake() {
        audioSource = this.GetComponent<AudioSource>();
    }

    private void stepPlay() {
        if (stepSound == null) {
            return;
        }
        audioSource.PlayOneShot(stepSound);
    }

    private void attackPlay() {
        if (attackSound == null) {
            return;
        }
        audioSource.PlayOneShot(attackSound);
    }

    public void jumpPlay() {
        if (jumpSound == null) {
            return;
        }
        audioSource.PlayOneShot(jumpSound);
    }

    public void hitPlay() {
        if (hitSound == null) {
            return;
        }

        audioSource.PlayOneShot(hitSound);
    }
}
