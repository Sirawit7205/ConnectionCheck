using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class GraphModule : MonoBehaviour {
	// Use this for initialization
	public TextMesh fuckyou;
	public KMSelectable[] LEDS;
	public KMSelectable Check;
	public GameObject[] R,L,LEDG,LEDR;
	int[]   WhichGraphDigit=new int[]{6,3,6,1,1,3,7,0,4,5}
		// 	                          0 1 2 3 4 5 6 7 8 9
	,WhichGraphLetter=new int[]{4,3,4,6,4,7,6,0,2,0,4,2,2,5,3,0,5,3,2,6,7,5,7,1,1,1};
	//							A B C D E F G H I J K L M N O P Q R S T U V W X Y Z
	Vector2[][] Neighbors=new Vector2[][]
	{
		new Vector2[]{new Vector2(1,2),new Vector2(2,3),new Vector2(1,3),new Vector2(4,6),new Vector2(5,6),new Vector2(5,7),new Vector2(4,7)}//0
		,new Vector2[]{new Vector2(1,4),new Vector2(1,2),new Vector2(1,6),new Vector2(2,3),new Vector2(2,4),new Vector2(4,7),new Vector2(6,7),new Vector2(7,8),new Vector2(6,8),new Vector2(5,6)}//1
		,new Vector2[]{new Vector2(1,2),new Vector2(1,3),new Vector2(1,6),new Vector2(2,6),new Vector2(3,6),new Vector2(3,4),new Vector2(4,6),new Vector2(5,6),new Vector2(4,5),new Vector2(4,8),new Vector2(4,7),new Vector2(5,7),new Vector2(7,8),}//2
		,new Vector2[]{new Vector2(1,7),new Vector2(1,2),new Vector2(2,7),new Vector2(6,7),new Vector2(5,6),new Vector2(5,7),new Vector2(4,8),new Vector2(3,8),new Vector2(3,4)}//3
		,new Vector2[]{new Vector2(1,3),new Vector2(2,4),new Vector2(3,6),new Vector2(1,2),new Vector2(3,4),new Vector2(3,7),new Vector2(4,7),new Vector2(5,7),new Vector2(7,8),new Vector2(4,5),new Vector2(3,8),new Vector2(5,8),new Vector2(1,8),new Vector2(1,6),new Vector2(2,6),new Vector2(4,6)}//4
		,new Vector2[]{new Vector2(1,2),new Vector2(2,3),new Vector2(3,4),new Vector2(4,5),new Vector2(5,6),new Vector2(6,7),new Vector2(7,8),new Vector2(1,8),new Vector2(2,7),new Vector2(3,6),new Vector2(2,6),new Vector2(3,7),new Vector2(1,4),new Vector2(5,8)}//5
		,new Vector2[]{new Vector2(1,3),new Vector2(3,5),new Vector2(5,7),new Vector2(2,7),new Vector2(2,4),new Vector2(4,6),new Vector2(6,8),new Vector2(3,7),new Vector2(4,7),new Vector2(4,8),new Vector2(1,2),new Vector2(5,6)}//6
		,new Vector2[]{new Vector2(2,3),new Vector2(3,8),new Vector2(3,5),new Vector2(3,6),new Vector2(4,5),new Vector2(4,7),new Vector2(4,8),new Vector2(5,7),new Vector2(5,6),new Vector2(7,8),new Vector2(2,8),new Vector2(6,7),new Vector2(1,6),new Vector2(1,7),new Vector2(1,2),new Vector2(2,7)}//7
	};
	Vector2[] Queries;
	int[] On,digitCount;
	int graphID=-1;
	bool isActive=false;
	Dictionary <Vector2,bool> dict=new Dictionary<Vector2, bool>();
	void Start ()
	{
		MyInit ();

		GetComponent<KMBombModule>().OnActivate += ActivateModule;
	}

	void ActivateModule()
	{
		// ACCESS SERIAL AND FIND THE KEY
		string serial;
		char dgt;
		int batteryCount = 0;
		List<string> responses = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_BATTERIES, null);
		foreach (string response in responses)
		{
			Dictionary<string, int> responseDict = JsonConvert.DeserializeObject<Dictionary<string, int>>(response);
			batteryCount += responseDict["numbatteries"];
		}
		//fuckyou.text = batteryCount +"";
		// ONLY UNITY EDITOR RANDOM GENERATOR
		if (Application.platform == RuntimePlatform.WindowsEditor)
			dgt = (char)Random.Range ('A','Z'+1);
		else
		{
			List<string> queryResponse = GetComponent<KMBombInfo>().QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
			Dictionary<string, string> responseDict = JsonConvert.DeserializeObject<Dictionary<string, string> >(queryResponse[0]);
			serial=responseDict["serial"];
			bool tem;
			// FIRST CONDITION ALL DIFFERENT
			tem=true;
			for (int i = 1; i <= 8; i++)
				if (digitCount [i] != 1)
				{
					tem = false;
					break;
				}
			if (tem)
			{
				dgt=serial[5];
			}
			else if (digitCount[1]>1)//SECOND CONDITION more than one 1
			{
				dgt=serial[0];
			}
			else if (digitCount[7]>1)//THIRD CONDITION more than one 7
			{
				dgt=serial[5];
			}
			else if (digitCount[2]>2)//4th CONDITION at least three 2
			{
				dgt=serial[1];
			}
			else if (digitCount[5]==0) //5th CONDITION no 5
			{
				dgt=serial[4];
			}
			else if (digitCount[8]==2) //6th CONDITION exact two 8
			{
				dgt=serial[2];
			}
			else if (batteryCount==0 || batteryCount>6) //Battery out of bound
			{
				dgt=serial[5];
			}
			else //Battery for digit choosing
			{
				dgt=serial[batteryCount-1];
			}
			//fuckyou.text = dgt +"";
		}
		//tem='1'; /////////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		Debug.Log ("Dgt : " + dgt);

		if('A'<=dgt && dgt<='Z')
			graphID=WhichGraphLetter[dgt-'A'];
		else
			graphID=WhichGraphDigit[dgt-'0'];
		if (graphID != -1)
		{
			for (int i = 0; i < Neighbors [graphID].Length; i++)
				dict.Add (Neighbors [graphID] [i], true);
		}
		else
		{
			Debug.Log ("THIS IS BAD");
		}
		Debug.Log ("ID : " + graphID);
		isActive = true;
		for (int i = 0; i < 4; i++)
			if(On[i]>0)
				LEDG [i].SetActive (true);
			else
				LEDR [i].SetActive (true);
	}
	void OnPressLED(int which)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (!isActive)
			return;
		Debug.Log (which);
		On [which] = 1-On [which];
		LEDG [which].SetActive (On[which]==1);
		LEDR [which].SetActive (On[which]!=1);
	}
	void OnPressCheck()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (!isActive)
			return;
		bool win = true;
		for (int i = 0; i < 4; i++)
		{
			bool tempo;
			bool lineExists=dict.TryGetValue(Queries[i],out tempo);
			if (lineExists != (On [i]==1))
			{
			 	win = false;
				break;
			}
		}
		if (win)
			GetComponent<KMBombModule> ().HandlePass ();
		else
			GetComponent<KMBombModule> ().HandleStrike ();
	}
	void MyInit()
	{
		// Decide the source graph based on the first digit of serial

		On=new int[4];
		//Setup Pressing Functions
		Check.OnInteract += delegate () {OnPressCheck (); return false; };
		for (int i = 0; i < 4; i++)
		{
			int j = i;
			LEDS[i].OnInteract += delegate () {OnPressLED (j); return false; };
		}
		// Setup Keys
		Queries=new Vector2[4];
		digitCount = new int[9];
			for (int i = 1; i <= 8; i++)
				digitCount[i]=0;
		List<Vector2> noRepeat=new List<Vector2>();
		for (int i = 1; i <= 8; i++)
			for (int j = i + 1; j <= 8; j++)
				noRepeat.Add (new Vector2 (i, j));
		for (int i = 0; i < 4; i++)
		{
			// Random booleans
			On[i]=Random.Range ((int)0, (int)2);
			//CHOOSE VALUES
			int pos=Random.Range(0,noRepeat.Count);
			Queries [i] = noRepeat [pos];
			noRepeat.RemoveAt(pos);
			// COUNT DIGITS FOR DECISION
			digitCount [(int)Queries [i].x]++;
			digitCount [(int)Queries [i].y]++;
			//SOMETIMES SWAP GRAPHICS
			float smol=Queries[i].x,big=Queries[i].y;
			if (Random.Range (0, 2) == 1)
			{
				float temp=smol;
				smol=big;
				big=temp;
			}
			//Debug.Log (smol + ":" + big);
			L[i].GetComponentInChildren<TextMesh>().text=smol+"";
			R[i].GetComponentInChildren<TextMesh>().text=big+"";
		}
	}
}
