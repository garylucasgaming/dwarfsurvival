using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Showcase_GunItem : MonoBehaviour
{
    public enum ammoType { Tag = 0, Name = 1 }
    [System.Serializable]
    public class animations
    {
        public AnimationClip fireAnim, reloadAnim;
    }
    [System.Serializable]
    public class ammoClass
    {
        public string ammoName;
        public ammoType type;
    }
    public int ammo, maxAmmo;
    public float damage, rps, range;
    public bool fullAuto = false;
    public AudioClip fireSound;
    public float fireSoundVolume, reloadSoundVolume;
    [Tooltip("The item tags or names that this gun can accept as ammo")]
    public List<ammoClass> acceptedAmmo;
    public animations animationSet;
}
