using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Btnsound : MonoBehaviour
{
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioSource audioSource;

    private HashSet<Button> registeredBtns = new HashSet<Button>();

    private void Update()
    {
        Button[] allbtn = FindObjectsByType<Button>(FindObjectsSortMode.None);
        for (int i = 0; i < allbtn.Length; i++)
        {
            if (!registeredBtns.Contains(allbtn[i]) && allbtn[i].name != "ShutDown")
            {
                allbtn[i].onClick.AddListener(() => audioSource.PlayOneShot(clickSound));
                registeredBtns.Add(allbtn[i]);
            }
        }
    }
}