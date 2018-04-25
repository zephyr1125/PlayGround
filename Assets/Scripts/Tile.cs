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
	
	public Image Image;
	public TMP_Text Text;

	public float CurrentTemperature, PreTemperature;

	private bool _isContinue;

	public void Init(Vector2Int pos)
	{
		Pos = pos;
		_tileManager = TileManager.Instance;
		CurrentTemperature = _tileManager.DefaultTemperature;
		PreTemperature = _tileManager.DefaultTemperature;
	}

	public void SetSource()
	{
		SetTemperature(_tileManager.SourceTemperature);
		_isContinue = _tileManager.IsCenterContinue;
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
	/// 算法1: 使自己直接等于周围温度的平均值,0.1的阻尼
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
