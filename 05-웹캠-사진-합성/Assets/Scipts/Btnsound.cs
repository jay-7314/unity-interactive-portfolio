using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Btnsound : MonoBehaviour
{
    [System.Serializable]
    public class ButtonSoundOverride
    {
        public string buttonName;
        public AudioClip clip;
    }

    [SerializeField] AudioClip defaultClickSound;
    [SerializeField] AudioSource audioSource;
    [SerializeField] List<ButtonSoundOverride> soundOverrides = new List<ButtonSoundOverride>();

    private HashSet<Button> registeredBtns = new HashSet<Button>();

    private void Update()
    {
        Button[] allbtn = FindObjectsByType<Button>(FindObjectsSortMode.None);
        for (int i = 0; i < allbtn.Length; i++)
        {
            if (!registeredBtns.Contains(allbtn[i]) && allbtn[i].name != "ShutDown")
            {
                Button btn = allbtn[i];
                AudioClip clipToPlay = GetClipForButton(btn.name);
                btn.onClick.AddListener(() => audioSource.PlayOneShot(clipToPlay));
                registeredBtns.Add(btn);
            }
        }
    }

    private AudioClip GetClipForButton(string btnName)
    {
        foreach (var entry in soundOverrides)
        {
            if (entry.buttonName == btnName)
                return entry.clip;
        }
        return defaultClickSound;
    }
}