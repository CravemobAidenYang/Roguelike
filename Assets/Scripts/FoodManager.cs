using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FoodListSaveData
{
    public int count;
    public FoodSaveData[] foodSaveDatas;

    public FoodListSaveData(int count, FoodSaveData[] foodSaveDatas)
    {
        this.count = count;
        this.foodSaveDatas = foodSaveDatas;
    }
}

public class FoodManager : MonoBehaviour 
{
    public float _ColaProbability;
    public Food[] _FoodPrefabs;

    List<Food> _FoodList = new List<Food>();
    public static FoodManager Instance
    {
        get;
        private set;
    }

	void Awake () 
    {
	    if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
	}
	
	public Food CreateFood()
    {
        int index;
        if(Random.Range(0f, 1f) <= _ColaProbability)
        {
            index = 0;
        }
        else
        {
            index = 1;
        }
        var food = Instantiate(_FoodPrefabs[index]);

        _FoodList.Add(food);

        return food;
    }

    public void Cleanup()
    {
        for (int i = 0; i < _FoodList.Count; ++i)
        {
            Destroy(_FoodList[i].gameObject);
        }

        _FoodList.Clear();
    }

    public Food GetFoodByPos(Position pos)
    {
        foreach(var food in _FoodList)
        {
            if(food.Pos == pos)
            {
                return food;
            }
        }

        return null;
    }

    public void RemoveFood(Food food)
    {
        Destroy(food.gameObject);
        _FoodList.Remove(food);
    }

    public void SaveFoodData()
    {
        var foodDataArr = new FoodSaveData[_FoodList.Count];

        for (int i = 0; i < _FoodList.Count; ++i)
        {
            foodDataArr[i] = _FoodList[i].CreateSaveData();
        }

        var foodListData = new FoodListSaveData(_FoodList.Count, foodDataArr);

        SaveLoad.SaveData("FoodListSaveData", foodListData);
    }

    public void LoadFoodData()
    {
        Cleanup();

        var data = SaveLoad.LoadData<FoodListSaveData>("FoodListSaveData", null);

        for (int i = 0; i < data.count; ++i)
        {
            var food = Instantiate(_FoodPrefabs[data.foodSaveDatas[i].foodIndex]);
            food.ApplySaveData(data.foodSaveDatas[i]);
            _FoodList.Add(food);
        }
    }
}
