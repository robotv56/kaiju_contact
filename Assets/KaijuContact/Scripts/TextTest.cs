using UnityEngine;
using TMPro;
using System.Collections;

public class TextTest : MonoBehaviour
{
    public bool running = true;
    TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        StartCoroutine("coroutine");
    }

    int lines = 0;
    string buffer = "";

    IEnumerator coroutine()
    {
        while(running)
        {
            
            for(int i = 0; i != 39; i++)
            {
                buffer += Random.Range(0,10);
                text.SetText(buffer);
                yield return new WaitForSeconds(0.001f);
            }
            buffer += "\n";
            if (lines > 26)
            {
                buffer = buffer.Substring(40);
            }
            Debug.Log("line done");
            lines++;
            text.SetText(buffer);
            //yield return new WaitForSeconds(0.001f);
        }

        yield return null;
    }

}
