using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
	public Vector2Int Pos;

	private TileManager _tileManager;
	
	public Image Image, WestWall, NorthWall;
	public TMP_Text Text;

	public float CurrentTemperature, PreTemperature;

	private bool _isContinue;

	public bool HasWestWall, HasNorthWall;

	public void Init(Vector2Int pos)
	{
		Pos = pos;
		_tileManager = TileManager.Instance;
		CurrentTemperature = _tileManager.DefaultTemperature;
		PreTemperature = _tileManager.DefaultTemperature;
	}

	public void OnClick()
	{
		switch (_tileManager.ClickState)
		{
			case TileManager.ClickStateEnum.ClickSetSource:
				SetSource();
				break;
			case TileManager.ClickStateEnum.ClickSetWall:
				SetWall();
				break;
		}
	}
	
	public void SetSource()
	{
		SetTemperature(_tileManager.SourceTemperature);
		_isContinue = _tileManager.IsCenterContinue;
	}

	public void SetWall()
	{
		if (HasNorthWall && HasWestWall)
		{
			HasWestWall = false;
			HasNorthWall = false;
		}else if (HasNorthWall)
		{
			HasWestWall = true;
		}else if (HasWestWall)
		{
			HasWestWall = false;
			HasNorthWall = true;
		}
		else
		{
			HasWestWall = true;
		}
		
		NorthWall.gameObject.SetActive(HasNorthWall);
		WestWall.gameObject.SetActive(HasWestWall);
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
	/// 算法1: 使自己直接等于周围温度的平均值,0.1的阻尼, 墙视为对面与自己温度一样
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
			//如果邻居是墙对面,则使用我自己的温度
			if (HasWestWall && neighbour.Pos.x == Pos.x - 1)
			{
				average += PreTemperature;
				continue;
			}
			
			if (HasNorthWall && neighbour.Pos.y == Pos.y + 1)
			{
				average += PreTemperature;
				continue;
			}
			
			if (neighbour.HasWestWall && neighbour.Pos.x == Pos.x + 1)
			{
				average += PreTemperature;
				continue;
			}
			
			if (neighbour.HasNorthWall && neighbour.Pos.y == Pos.y - 1)
			{
				average += PreTemperature;
				continue;
			}
			
			average += neighbour.PreTemperature;
		}

		average /= neighbours.Length;

		//0.1度的阻尼
		if (average >= CurrentTemperature+0.1f)
		{
			SetTemperature(average-0.1f);
		}else if (average <= CurrentTemperature - 0.1f)
		{
			SetTemperature(average+0.1f);
		}
	}

	/// <summary>
	/// 所有tile要统一进行PreTemperature的保存
	/// </summary>
	public void SavePreTemperature()
	{
		PreTemperature = CurrentTemperature;
	}
}
