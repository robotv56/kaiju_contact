using UnityEngine;
using TMPro;
using System.Collections;

public class DecorTextScroll : MonoBehaviour
{
    public bool running = true;
    TextMeshProUGUI text;

    public int linesIn = 26;
    public int charsIn = 39;

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
            for (int i = 0; i != charsIn; i++)
            {
                buffer += Random.Range(0, 10);
                text.SetText(buffer);
                yield return new WaitForSeconds(0.1f);
            }
            buffer += "\n";
            if (lines > linesIn)
            {
                buffer = buffer.Substring(charsIn + 1);
            }
            //Debug.Log("line done");
            lines++;
            text.SetText(buffer);
            //yield return new WaitForSeconds(0.001f);
        }

        yield return null;
    }

}
