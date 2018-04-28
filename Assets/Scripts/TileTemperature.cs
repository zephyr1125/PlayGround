using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileTemperature : MonoBehaviour
{
	public Vector2Int Pos;

	private TileTemperatureManager _tileManager;
	
	public Image Image, WestWholeWall, NorthWholeWall, WestHalfWall, NorthHalfWall;
	public TMP_Text Text;

	public float CurrentTemperature, PreTemperature;

	private bool _isContinue;

	public bool HasWestWholeWall, HasNorthWholeWall,HasWestHalfWall, HasNorthHalfWall;

	public void Init(Vector2Int pos)
	{
		Pos = pos;
		_tileManager = TileTemperatureManager.Instance;
		CurrentTemperature = _tileManager.DefaultTemperature;
		PreTemperature = _tileManager.DefaultTemperature;
	}

	public void OnClick()
	{
		switch (_tileManager.ClickState)
		{
			case TileTemperatureManager.ClickStateEnum.ClickSetSource:
				SetSource();
				break;
			case TileTemperatureManager.ClickStateEnum.ClickSetHalfWall:
				SetHalfWall();
				break;
			case TileTemperatureManager.ClickStateEnum.ClickSetWholeWall:
				SetWholeWall();
				break;
		}
	}
	
	public void SetSource()
	{
		SetTemperature(_tileManager.SourceTemperature);
		_isContinue = _tileManager.IsCenterContinue;
	}

	public void SetWholeWall()
	{
		if (HasNorthWholeWall && HasWestWholeWall)
		{
			HasWestWholeWall = false;
			HasNorthWholeWall = false;
		}else if (HasNorthWholeWall)
		{
			HasWestWholeWall = true;
		}else if (HasWestWholeWall)
		{
			HasWestWholeWall = false;
			HasNorthWholeWall = true;
		}
		else
		{
			HasWestWholeWall = true;
		}
		
		NorthWholeWall.gameObject.SetActive(HasNorthWholeWall);
		WestWholeWall.gameObject.SetActive(HasWestWholeWall);
	}
	
	public void SetHalfWall()
	{
		if (HasNorthHalfWall && HasWestHalfWall)
		{
			HasWestHalfWall = false;
			HasNorthHalfWall = false;
		}else if (HasNorthHalfWall)
		{
			HasWestHalfWall = true;
		}else if (HasWestHalfWall)
		{
			HasWestHalfWall = false;
			HasNorthHalfWall = true;
		}
		else
		{
			HasWestHalfWall = true;
		}
		
		NorthHalfWall.gameObject.SetActive(HasNorthHalfWall);
		WestHalfWall.gameObject.SetActive(HasWestHalfWall);
	}

	private void SetTemperature(float temperature)
	{
		CurrentTemperature = temperature;
		Text.text = temperature.ToString();
		var color = new Color();
		if (temperature > 0)
		{
			color = new Color(1, 1 - temperature / 30, 1 - temperature / 30);
		}
		else
		{
			color = new Color(1 - temperature / -30, 1 - temperature / -30, 1);
		}
		Image.color = color;
	}

	/// <summary>
	/// 算法1: 使自己直接等于周围温度的平均值,有0.1/Step的气温自恢复, 完全墙视为对面与自己温度一样\
	/// 不完全隔热墙使用2/3平均温度
	/// </summary>
	public void Step()
	{
		//持续标签表示为热源,不改变自身温度
		if (_isContinue)
		{
			return;
		}
		
		var neighbours = _tileManager.GetNeighbours(Pos);
		float average = 0;
		foreach (var neighbour in neighbours)
		{
			float wallRate = 0;
			//如果邻居是墙对面,则使用我自己的温度
			//半墙的话则使用2/3平均温度
			if (neighbour.Pos.x == Pos.x - 1)
			{
				if (HasWestWholeWall)
				{
					wallRate = 1;
				}else if(HasWestHalfWall)
				{
					wallRate = _tileManager.HalfWallRate;
				}
			}
			
			if (neighbour.Pos.y == Pos.y + 1)
			{
				if (HasNorthWholeWall)
				{
					wallRate = 1;
				}else if(HasNorthHalfWall)
				{
					wallRate = _tileManager.HalfWallRate;
				}
			}
			
			if (neighbour.Pos.x == Pos.x + 1)
			{
				if (neighbour.HasWestWholeWall)
				{
					wallRate = 1;
				}else if(neighbour.HasWestHalfWall)
				{
					wallRate = _tileManager.HalfWallRate;
				}
			}
			
			if (neighbour.Pos.y == Pos.y - 1)
			{
				if (neighbour.HasNorthWholeWall)
				{
					wallRate = 1;
				}else if(neighbour.HasNorthHalfWall)
				{
					wallRate = _tileManager.HalfWallRate;
				}
			}
			
			average += neighbour.PreTemperature + (PreTemperature - neighbour.PreTemperature) * wallRate;
		}
		
		//平均值里要加入自己
		average += PreTemperature;
		
		average /= neighbours.Length + 1;

		float damp = 0;
		
		//0.1度的气温自恢复
		if (average >= _tileManager.DefaultTemperature+0.1f)
		{
			damp = -0.1f;
		}else if (average <= _tileManager.DefaultTemperature - 0.1f)
		{
			damp = 0.1f;
		}

		SetTemperature(average + damp);
	}

	/// <summary>
	/// 所有tile要统一进行PreTemperature的保存
	/// </summary>
	public void SavePreTemperature()
	{
		PreTemperature = CurrentTemperature;
	}

	public void Reset()
	{
		_isContinue = false;
		SetTemperature(_tileManager.DefaultTemperature);
		PreTemperature = _tileManager.DefaultTemperature;
	}
}
