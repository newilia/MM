
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class Ship_Move2 : MonoBehaviour
{
	public Transform 	Wind_Transform; //Ссылка на объект отвечащий за направление ветра
	
	public Transform 	Ship_Transform; //Ссылка на корабль, для его поворота
	public Transform 	Ship_Kren_Transform;		//Ссылка на корабль, чтобы наклонять его в зависимости от ветра и парусов.
	public GameObject 	Parus;			//Ссылка на объект с анимацией парусов
	public Transform 	Parus_Forward;	//ссылка на передний парус для его разворота
	public Transform 	Parus_Back;		//ссылка на задний парус для его разворота
	public float 		Speed; 			//скорость
	public float 		Open_Parus; 	//Степень раскрытия паруса [0,1]
	public float 		Turn_Limiter_Parus;	//Ограничение поворота паруса
	public float 		Parus_Power;	//определяем степень влияния ветра. Чем больше угол между ветром и парусом, тем сильнее влияние
	public float 		Kren_Ship;		//Крен корабля от ветра в парусах [0, 8135]

	public GameObject 	Ship_Player;
	public float 		torque; 		//скорость разворота
	public GameObject 	Rudder; 		//руль корабля прямо
	public GameObject 	RudderLeft; 	//руль корабля налево
	public GameObject 	RudderRight;	//руль корабля направо
	
	private Rigidbody 	m_rb;
	private Vector3 	m_dir;
	public float 		Wind_Power;		//Сила с которой дует ветер
	
	public float 		angle_Ship_Parus; //Угол между кораблём и парусом
	public float 		angle_Wind_Parus; //Угол между направлением ветра и парусом
	public float 		angle_Wind_Ship;  //Угол между направлением ветра и кораблём
	
	public float		Current_Kren;

    void Start()
    {
		Turn_Limiter_Parus = 90;
		m_rb = GetComponent<Rigidbody>();
    }

	void Update()
    {
		InputValue();
	}
	
    void FixedUpdate()
    {
		RBMove();
    }
	
	private void InputValue()
	{
		RBRotate();
		Parus_Power = (Mathf.Abs(Mathf.Abs(angle_Wind_Parus) - 90f)) * Open_Parus; //определяем степень влияния ветра. Чем больше угол между ветром и парусом, тем сильнее влияние
		Kren_Ship = (90f - (Mathf.Abs(Mathf.Abs(angle_Wind_Ship) - 90f))) * Parus_Power * Wind_Power; // [0, 8135] Степень отклонения корабля, зависит от ветра, парусов и положения корабля отностительно ветра. 
		m_dir = new Vector3(0,0, Parus_Power);
		m_dir = Ship_Transform.TransformDirection(m_dir); //перемещать игрока относительно положения камеры/
		
		//Определяем угол между двумя объектами по оси Y
		angle_Ship_Parus = Vector3.SignedAngle(Ship_Transform.forward, Parus_Back.forward, Vector3.up);
		angle_Wind_Parus = Vector3.SignedAngle(Wind_Transform.forward, Parus_Back.forward, Vector3.up);
		angle_Wind_Ship = Vector3.SignedAngle(Ship_Transform.forward, Wind_Transform.forward, Vector3.up);
		
		///////Изменяем раскрытие парусов
		if (Input.GetKey(KeyCode.W)) //Поднять парус
		{
			if(Open_Parus < 1) 
				Open_Parus = Open_Parus + 0.01f;
		}
		if (Input.GetKey(KeyCode.S)) //Спустить парус
		{
			if(Open_Parus > 0) 
				Open_Parus = Open_Parus - 0.01f;
		}
		if(Open_Parus < 0)
			Open_Parus = 0f;
		
		///////Изменяем натяжение парусов
		if (Input.GetKey(KeyCode.Q)) //Ослабить натяжение
		{
			if(Turn_Limiter_Parus < 90) 
				Turn_Limiter_Parus += 1f;
		}
		if (Input.GetKey(KeyCode.E)) //Натянуть
		{
			if(Turn_Limiter_Parus > 2) 
				Turn_Limiter_Parus -= 1f;
		}
	}

	
	private void RBMove()
	{
		//Вычисление скорости
		m_rb.velocity = m_dir * Speed * Wind_Power;
	}
	
	private void RBRotate()
	{
		float turn = Input.GetAxis("Horizontal"); 				//Направление поворота
		Ship_Player.transform.Rotate(0f, torque * turn, 0f); 	//Поворот корабля
		
		Parus.GetComponent<Animator>().SetFloat("Blend", Open_Parus); //Упрравление анимацией паруса
		
		//Поворот парусов в направлении ветра
		if (angle_Wind_Parus < -91f || angle_Wind_Parus > -89f) //Если парус ещё не стоит по ветру
		{
			if ((Mathf.Abs(angle_Wind_Parus) + 90f) > 180f) 		//Определяем в какую сторону поворачивать паруса
				if (angle_Ship_Parus < (88f + Turn_Limiter_Parus)) 	//Ограничение на максимальный поворот паруса от ветра
				{
					Parus_Forward.transform.Rotate(0f, torque, 0f);
					Parus_Back.transform.Rotate(0f, torque, 0f);
				}
			if ((Mathf.Abs(angle_Wind_Parus) + 90f) < 180f) 		//Определяем в какую сторону поворачивать паруса
				if (angle_Ship_Parus > (92f - Turn_Limiter_Parus)) 	//Ограничение на максимальный поворот паруса от ветра
				{
					Parus_Forward.transform.Rotate(0f, torque * -1f, 0f);
					Parus_Back.transform.Rotate(0f, torque * -1f, 0f);
				}
		}
		
		//Поворот паруса если изменяется натяжение, то есть изменяется ограничение на поворот
		if (angle_Ship_Parus < (90f - Turn_Limiter_Parus))
		{
			Parus_Forward.transform.Rotate(0f, torque, 0f);
			Parus_Back.transform.Rotate(0f, torque, 0f);
		}
		if (angle_Ship_Parus > (90f + Turn_Limiter_Parus))
		{
			Parus_Forward.transform.Rotate(0f, torque * -1f, 0f);
			Parus_Back.transform.Rotate(0f, torque * -1f, 0f);
		}
		

		if (Ship_Kren_Transform.eulerAngles.x > 180) //определяем текущий крен корабля в градусах. Например, если указан 350, то превращаем в -10
			Current_Kren = Ship_Kren_Transform.eulerAngles.x - 360f;
		else 
			Current_Kren = Ship_Kren_Transform.eulerAngles.x;
		
		//Крен корабля в зависимости от ветра
			if (angle_Wind_Ship < 0f) 												//Определяем в какую сторону наклонять корабль
				if (Current_Kren > ((Kren_Ship / 500f)*-1f)) 	//Ограничение на максимальный крен: Угол корабля по X > нужный крен корабля в зависимости от условий
					Ship_Kren_Transform.transform.Rotate(-0.1f, 0f, 0f);
			if (angle_Wind_Ship > 0f) 												//Определяем в какую сторону наклонять корабль
				if (Current_Kren < (Kren_Ship / 500f)) 		//Ограничение на максимальный крен: Угол корабля по X > нужный крен корабля в зависимости от условий
					Ship_Kren_Transform.transform.Rotate(0.1f, 0f, 0f);
					
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
	}
}
