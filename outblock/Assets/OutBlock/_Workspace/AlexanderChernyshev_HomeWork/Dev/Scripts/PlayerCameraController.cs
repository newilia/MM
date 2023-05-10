using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
	[SerializeField, Header("Start")]
	public GameObject CameraStart;
	public GameObject ShipMove;
	public GameObject ShipStatic;
	public GameObject Gloria_Default;
	public GameObject Gloria_Ship;
	public GameObject WhiteBox;
	public GameObject TestTrigger;
	
	private IEnumerator coroutine;
	
	[SerializeField, Header("Teleport")]
	public GameObject CameraTeleportStart;
	public GameObject Tower;
	public GameObject DomeMain;
	public GameObject DomelFar;
	public GameObject Teleport;
	public GameObject TeleportTrigger;
	public GameObject StopTrigger;
	
	public float Time1_Tower;
	public float Time2_Dome;
	public float Time3_Teleport;
	public float Time4_Dome;
	public float Time5_Tower;
	public float Time6_Camera;
	
	public GameObject TeleportFinish;
	public GameObject VictoryTrigger;
	
	public GameObject WaterCollider;
	public GameObject WaterWithoutCollider;
	
    void Start()
    {
        
    }

    void Update()
    {
        
    }
	private void OnTriggerEnter(Collider other)
    {
        if (other.name == "TestTrigger")
        {
			TestTrigger.SetActive(false);
			//other.GetComponent<Animator>().SetBool("TakeCoin", true);
			//Debug.Log("Сработал тригер");
			CameraStart.SetActive(true);

            coroutine = ShipStart();
            StartCoroutine(coroutine);
        }
		if (other.name == "TeleportTrigger")
        {
			StopTrigger.SetActive(true);
			TeleportTrigger.SetActive(false);
			CameraTeleportStart.SetActive(true);

            coroutine = UpTower();
            StartCoroutine(coroutine);
			
			coroutine = OpenDome();
			StartCoroutine(coroutine);
			
			coroutine = TeleportStart();
			StartCoroutine(coroutine);
			
			coroutine = CloseDome();
			StartCoroutine(coroutine);
			
			coroutine = DownTower();
			StartCoroutine(coroutine);
			
			coroutine = CameraDisable();
			StartCoroutine(coroutine);
        }
		if (other.name == "TowerUpTrigger")
        {
			Tower.GetComponent<Animator>().SetBool("Up2_Tower", true);
        }
		if (other.name == "TowerRotateTrigger")
        {
			Tower.GetComponent<Animator>().SetBool("Rotate_Tower", true);
        }
		if (other.name == "OpenDomeTrigger")
        {
			DomeMain.GetComponent<Animator>().SetBool("Open2_Tower", true);
			//DomelFar.GetComponent<Animator>().SetBool("Open2_Tower", true);
        }
		if (other.name == "FinishTrigger")
        {
			coroutine = TeleportFinish_Play();
			StartCoroutine(coroutine);
        }
    }
	private IEnumerator ShipStart()
    {
        yield return new WaitForSeconds(2f);
		ShipMove.SetActive(true);
		
		coroutine = EndStarScene();
        StartCoroutine(coroutine);
		//Debug.Log("1");
		coroutine = UI_WhiteBox_Start();
        StartCoroutine(coroutine);
		//Debug.Log("2");
    }
	
	private IEnumerator EndStarScene()
    {
        yield return new WaitForSeconds(10f);
		//Debug.Log("4");
        ShipMove.SetActive(false);
		ShipStatic.SetActive(true);
		Gloria_Default.SetActive(true);
		Gloria_Ship.SetActive(false);
		CameraStart.SetActive(false);
		
		WaterCollider.SetActive(false);
		WaterWithoutCollider.SetActive(true);
		
    }
	private IEnumerator UI_WhiteBox_Start()
    {
        yield return new WaitForSeconds(8f);
		//Debug.Log("3");
		WhiteBox.GetComponent<Animator>().SetBool("Start", true);
    }
	
	//------------------------------------------------------------------------------------------------------
	private IEnumerator UpTower()
    {
        yield return new WaitForSeconds(Time1_Tower);
		Tower.GetComponent<Animator>().SetBool("Up_Tower", true);
    }
	private IEnumerator OpenDome()
    {
        yield return new WaitForSeconds(Time2_Dome);
		DomeMain.GetComponent<Animator>().SetBool("Open_Tower", true);
		DomelFar.GetComponent<Animator>().SetBool("Open_Tower", true);
    }
	private IEnumerator TeleportStart()
    {
        yield return new WaitForSeconds(Time3_Teleport);
		Teleport.GetComponent<Animator>().SetBool("Start_Teleport", true);
    }
	private IEnumerator CloseDome()
    {
        yield return new WaitForSeconds(Time4_Dome);
		DomeMain.GetComponent<Animator>().SetBool("Close_Tower", true);
		DomelFar.GetComponent<Animator>().SetBool("Close_Tower", true);
    }
	private IEnumerator DownTower()
    {
        yield return new WaitForSeconds(Time5_Tower);
		Tower.GetComponent<Animator>().SetBool("Down_Tower", true);
    }
	private IEnumerator CameraDisable()
    {
        yield return new WaitForSeconds(Time6_Camera);
		CameraTeleportStart.SetActive(false);
		StopTrigger.SetActive(false);
    }
	
	private IEnumerator TeleportFinish_Play()
    {
        yield return new WaitForSeconds(2f);
		TeleportFinish.GetComponent<Animator>().SetBool("Start_Teleport", true);
		
		coroutine = Victory();
		StartCoroutine(coroutine);
    }
	private IEnumerator Victory()
    {
        yield return new WaitForSeconds(9f);
		VictoryTrigger.SetActive(true);
    }
}


/*
	private IEnumerator UpTower()
    {
        yield return new WaitForSeconds(2f);
		Tower.GetComponent<Animator>().SetBool("Up_Tower", true);
		
		coroutine = OpenDome();
        StartCoroutine(coroutine);
    }
	private IEnumerator OpenDome()
    {
        yield return new WaitForSeconds(5f);
		DomeMain.GetComponent<Animator>().SetBool("Open_Tower", true);
		DomelFar.GetComponent<Animator>().SetBool("Open_Tower", true);
		
		coroutine = TeleportStart();
        StartCoroutine(coroutine);
    }
	private IEnumerator TeleportStart()
    {
        yield return new WaitForSeconds(7f);
		Teleport.GetComponent<Animator>().SetBool("Start_Teleport", true);
		
		coroutine = CloseDome();
        StartCoroutine(coroutine);
    }
	private IEnumerator CloseDome()
    {
        yield return new WaitForSeconds(4f);
		DomeMain.GetComponent<Animator>().SetBool("Close_Tower", true);
		DomelFar.GetComponent<Animator>().SetBool("Close_Tower", true);
		
		coroutine = DownTower();
        StartCoroutine(coroutine);
    }
	private IEnumerator DownTower()
    {
        yield return new WaitForSeconds(4f);
		Tower.GetComponent<Animator>().SetBool("Down_Tower", true);
		
		coroutine = CameraDisable();
        StartCoroutine(coroutine);
    }
	private IEnumerator CameraDisable()
    {
        yield return new WaitForSeconds(7f);
		CameraTeleportStart.SetActive(false);
    }
*/
