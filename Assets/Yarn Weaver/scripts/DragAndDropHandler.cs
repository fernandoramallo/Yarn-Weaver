using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Json;

// Handles dragging and dropping a file in the browser for a WebGL build
public class DragAndDropHandler : MonoBehaviour {

  struct DraggedFileData {
    public string filename;
    public string fileContents;
  }

  // Called from javascript when user drops a file over the window
  // Takes data encoded as a json
  public void OnDrop(string draggedData) {
    var data = JsonUtility.FromJson<DraggedFileData> (draggedData);

    if (string.IsNullOrEmpty (data.filename) || string.IsNullOrEmpty (data.fileContents)) {
      Debug.Log ("Couldn't parse dropped file!");
      return;
    }

    var weaver = FindObjectOfType<YarnWeaverMain> ();
    
    if (data.filename.EndsWith(".json")) {
      Debug.Log ("Loading JSON yarn dialogue for file " + data.filename);
      weaver.OnFileDropped (data.filename, data.fileContents);
    }
  }


}
