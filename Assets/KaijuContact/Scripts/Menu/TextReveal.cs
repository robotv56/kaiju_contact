using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextReveal : MonoBehaviour
{
    public float delay = 0.1f;

    private TextMeshProUGUI text;

    private int count;


    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        count = text.text.Length;
        text.maxVisibleCharacters = 0;
        StartCoroutine("Coroutine");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Coroutine()
    {
        for(int i = 0; i != count; i++)
        {
            text.maxVisibleCharacters += 1;
            if(text.text[i] != '\n' && text.text[i] != ' ')yield return new WaitForSeconds(delay);
        }

        yield return null;
    }
}
