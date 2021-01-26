using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILogging : MonoBehaviour {

	public Text loggingWindow;
	static UILogging instance = null;

	public bool loggingEnabled = true;

    public Queue<string> textQueue;
    private static int queueLength = 10;


	public UILogging() {
        if (instance == null)
        {
            instance = this;
            textQueue = new Queue<string>(queueLength);
        }
        else
            Debug.LogError("There are multiple logging instances, and only the first one will be functional");
	}

    // TODO clean up the logging text

	public static void Error(string format, params object[] p) {
		if (Application.isEditor)
			Debug.LogErrorFormat(format, p);
		if(instance != null && instance.loggingEnabled)
        {
            instance.loggingWindow.text = ProcessText(string.Format("<color=red>" + format + "</Color>\n", p));
        }
			
	}

	public static void Warning(string format, params object[] p) {
		if (Application.isEditor)
			Debug.LogWarningFormat(format, p);
		if (instance != null && instance.loggingEnabled)
        {
            instance.loggingWindow.text = ProcessText(string.Format("<color=orange>" + format + "</Color>\n", p));
        }

    }

	public static void Info(string format, params object[] p) {
		if (Application.isEditor)
			Debug.LogFormat(format, p);
		if (instance != null && instance.loggingEnabled)
        {
           instance.loggingWindow.text = ProcessText(string.Format("<color=black>" + format + "</Color>\n", p));
        }
			
	}

    private static string ProcessText(string text)
    {
        if (instance.textQueue.Count == queueLength)
        {
            instance.textQueue.Dequeue();
            instance.textQueue.TrimExcess();
        }
        instance.textQueue.Enqueue(text);

        // create 1 text window from queue
        string returnText = "";
        foreach (string textLine in instance.textQueue)
            returnText += textLine;

        return returnText;
    }
}
