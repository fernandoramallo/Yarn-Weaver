﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System;
using System.IO;
using System.Linq;

using Yarn.Unity;
using SFB;

public class YarnWeaverMain : MonoBehaviour {

	public Button playButton;
	public Text filenameLabel;
	public Dropdown recentFilesList;
	public TextAsset sampleYarn;

	public GameObject sidebar;
	public GameObject[] workArea;
  public GameObject loadButton;
  public GameObject refreshButton;
  public GameObject exitButton;
  public Text dragHint;

	DialogueRunner dialogueRunner;

	static string currentFilePath;
	List<string> previousFilePaths = new List<string>();

	static bool tutorialMode = false;

	const string prefsKey = "YarnWeaver_File";

	// Use this for initialization
	void Start () {
    #if UNITY_WEBGL
    dragHint.gameObject.SetActive(true);
    loadButton.SetActive(false);
    refreshButton.SetActive(false);
    exitButton.SetActive(false);
    recentFilesList.gameObject.SetActive(false);
    #else
    dragHint.gameObject.SetActive(false);

		// build recent file list
		previousFilePaths.Clear(); // just to be safe
		while ( PlayerPrefs.HasKey( prefsKey + previousFilePaths.Count.ToString() )) {
			previousFilePaths.Add( PlayerPrefs.GetString( prefsKey + previousFilePaths.Count.ToString() ) );
		}

		if (previousFilePaths.Count > 0) {
			// convert that into dropdown population
			// we also want to remove the "file:///" if it's there
			recentFilesList.AddOptions( previousFilePaths.Select( x => (Path.GetFileNameWithoutExtension( x ) + "\n<size=10>" + ( x.StartsWith("file:///") ? x.Replace("file:///", "") : x) +"</size>").Replace("%20", " ") ).ToList() );
		} else {
			recentFilesList.gameObject.SetActive( false );
		}
    #endif

		dialogueRunner = FindObjectOfType<DialogueRunner>();
		filenameLabel.text = "";

		if (tutorialMode == true) {
			OnClickLoadSample();
		} else if (currentFilePath != null && currentFilePath.Length > 0) {
			StartCoroutine( OutputRoutine( currentFilePath ) );
		}
	}
	
	// Update is called once per frame
	void Update () {
		bool isFileOpen = tutorialMode || (currentFilePath != null && currentFilePath.Length > 0);
		if (isFileOpen) {
			filenameLabel.text = tutorialMode ? "SAMPLE / TUTORIAL" : Path.GetFileNameWithoutExtension( currentFilePath.Replace( "%20", " " ) );
			filenameLabel.text += " > " + (dialogueRunner.isDialogueRunning && dialogueRunner.currentNodeName != null && dialogueRunner.currentNodeName.Length > 0 ? dialogueRunner.currentNodeName : "(STOPPED)" );
		}
		foreach (var go in workArea) {
      #if UNITY_WEBGL
      if (go == refreshButton)
        continue;
      #endif
			go.SetActive( isFileOpen );
		}
		sidebar.SetActive( !isFileOpen );

	}

	public void OnClickRecentFile( int filepathIndex ) {
		// 0 is "open recent file" and doesn't count
		if (filepathIndex > 0) {
			StartCoroutine( OutputRoutine( previousFilePaths[filepathIndex - 1] ) );
		}
	}

	public void OnClickLoadSample() {
		tutorialMode = true;
		dialogueRunner.AddScript( sampleYarn );
		StartCurrentFile();
	}

	public void OnResetButtonClick () {
		SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
	}

	public void OnCloseButtonClick () {
		currentFilePath = "";
		tutorialMode = false;
		SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
	}

	public void OnOpenButtonClick () {
		var startPath = System.Environment.GetFolderPath( System.Environment.SpecialFolder.Desktop );
		if (currentFilePath != null && currentFilePath.Length > 0) {
			startPath = Path.GetDirectoryName( currentFilePath );
		}

		var paths = StandaloneFileBrowser.OpenFilePanel("select Yarn JSON file...", startPath, "json", false);
		if (paths.Length > 0) {
			StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
		}
	}

	public void OnClickQuitButton () {
		Application.Quit();
	}

	IEnumerator OutputRoutine(string url) {
		dialogueRunner.Stop();
		while (dialogueRunner.isDialogueRunning) {
			yield return 0;
		}
		dialogueRunner.Clear();
		var loader = new WWW(url);
		yield return loader;
		currentFilePath = url;
		previousFilePaths.Add( url );
		dialogueRunner.AddScript( loader.text );

		// save to playerprefs
		if (previousFilePaths.Contains( currentFilePath )) {
			Debug.Log( "detecting duplicate filepaths in file history..." );
			previousFilePaths.RemoveAll( x => x == currentFilePath );
		}
		previousFilePaths.Insert( 0, currentFilePath );

		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		for (int i = 0; i < previousFilePaths.Count; i++) {
			PlayerPrefs.SetString( prefsKey + i.ToString(), previousFilePaths[i] );
		}
		PlayerPrefs.Save();

		yield return 0;

		StartCurrentFile();
	}

  #if UNITY_WEBGL
  // We load a pre-loaded dialog for WebGL since we can't load from the file system
  IEnumerator OutputRoutineFromLoadedJson(string url, string fileContents) {
    dialogueRunner.Stop();
    while (dialogueRunner.isDialogueRunning) {
      yield return 0;
    }
    dialogueRunner.Clear();

    currentFilePath = url;
    dialogueRunner.AddScript( fileContents );

    yield return 0;

    StartCurrentFile();
  }
  #endif

	string GetStartNode () {
		// search for a node that starts with "Start" or with the filename
		string filename = Path.GetFileNameWithoutExtension( currentFilePath );
		var startSearch = dialogueRunner.dialogue.allNodes.Where( x => x.ToLower().StartsWith("start") || x.ToLower().StartsWith(filename.ToLower()) ).ToArray();
		if (startSearch != null && startSearch.Length > 0) {
			return startSearch[0];
		} else { // otherwise, just go for the first node we find
			return dialogueRunner.dialogue.allNodes.ToArray()[0];
		}
	}

	public void StartCurrentFile () {
		if (dialogueRunner.isDialogueRunning) {
			dialogueRunner.Stop();
		} else {
			dialogueRunner.startNode = GetStartNode();
			dialogueRunner.StartDialogue();
		}
	}

  public void OnFileDropped(string filePath, string fileContents) {
    StartCoroutine (OutputRoutineFromLoadedJson (filePath, fileContents));
  }
}
