
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class Ship_Move : MonoBehaviour
{
	public enum Choose
	{
		up,
		forward,
		right
	}
	public Choose ch;
	
	public GameObject 	Ship_Player;
	public Transform 	Ship_Transform;
	public GameObject 	Parus;
	public float 		Speed;
	public float 		torque;
		private float 		torque_Constant;
		public float 		torque_When_Ship_Standing;
	
	public GameObject 	Rudder; //руль корабля
	public GameObject 	RudderLeft;
	public GameObject 	RudderRight;
	
	private Rigidbody 	m_rb;
	private Vector3 	m_dir;
	private float 		forwardVector;
	public  float 		SpeedScale_ShiftKey;
	private float 		Speed_Constant;
	
    // Start is called before the first frame update
    void Start()
    {
		m_rb = GetComponent<Rigidbody>();
		Speed_Constant = Speed;
		torque_Constant = torque;
    }

	void Update()
    {
		InputValue();
		//RBRotate();
	}
	
    // Update is called once per frame
    void FixedUpdate()
    {
		RBMove();
    }
	private void InputValue()
	{
		if (Input.GetAxis("Vertical") > 0) //не даёт плыть назад
		{
			forwardVector = Input.GetAxis("Vertical");
			torque = torque_Constant;
			RBRotate();
		}
		else
		{
			forwardVector = 0;
			torque = torque_When_Ship_Standing;
			RBRotate();
		}
		m_dir = new Vector3(0,0, forwardVector);
		m_dir = Ship_Transform.TransformDirection(m_dir); //перемещать игрока относительно положения камеры/
	}

	
	private void RBMove()
	{
		if (Input.GetKeyDown(KeyCode.LeftShift)) //ускорение на Shift
		{
			Speed = Speed * SpeedScale_ShiftKey;
		}
		else
		{
			Speed = Speed_Constant;
		}
		//Debug.Log(Speed);
		//Debug.Log(Speed_Constant);
		m_rb.velocity = m_dir * Speed;
	}
	
	private void RBRotate()
	{
		float turn = Input.GetAxis("Horizontal");
		//if (Input.GetAxis("Vertical") = 0) torque = torque_When_Ship_Standing;
		//else torque = torque_Constant; 
		Ship_Player.transform.Rotate(0f, torque * turn, 0f);
		
		//Debug.Log(turn);
		if (turn<0)
		{
			Rudder.SetActive(false);
			RudderRight.SetActive(false);
			RudderLeft.SetActive(true);
		}else
		{
			Rudder.SetActive(false);
			RudderLeft.SetActive(false);
			RudderRight.SetActive(true);
		}
		
		if (turn == 0)
		{
			Rudder.SetActive(true);
			RudderLeft.SetActive(false);
			RudderRight.SetActive(false);
		}
		
		//Упрравление анимацией паруса
		
		if (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") == 0) Parus.GetComponent<Animator>().SetInteger("Turn Number", 4);
		if (Input.GetAxis("Horizontal") < 0 && Input.GetAxis("Vertical") > 0) Parus.GetComponent<Animator>().SetInteger("Turn Number", 2);
		if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") > 0) Parus.GetComponent<Animator>().SetInteger("Turn Number", 1);
		//if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) Parus.GetComponent<Animator>().SetInteger("Turn Number", 1);
		if (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") > 0) Parus.GetComponent<Animator>().SetInteger("Turn Number", 3);
		if (Input.GetAxis("Horizontal") > 0 && Input.GetAxis("Vertical") == 0) Parus.GetComponent<Animator>().SetInteger("Turn Number", 5);
	}
}
