using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Security.Cryptography;
public class MapGen : MonoBehaviour
{

	public int xLength;
	public int yLength;

	public string seed;

	public GameObject wall;
	public GameObject floor;
	public GameObject mapObject;
	public GameObject spawnPoint;
	public GameObject finishLine;
	public GameObject player1;
	//public Transform spawnTransform;
	public Camera camera;
	public Text widthText;
	public Text heightText;
	public Text fillText;
	public Slider wSlider;
	public Slider hSlider;
	public Slider fSlider;
	public Button playButton;
	public Button genButton;
	public Toggle playerToggle;

	[Range(0, 100)]
	public int randFill;


	int[,] canvas;

	private void Update()
	{
		//Scrolling in and out controls
		if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
		{

				camera.orthographicSize += 2;

		}
		else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
		{
			if (camera.orthographicSize > 5)
			{
				camera.orthographicSize -= 2;
			}
		}
	}


	//Empties canvas so my pc doesnt melt, recreates a new canvas with 
	public void GenerateMap()
	{
		EmptyMap();
		GenMap();
		CreateTiles();
	}


	void GenMap()
	{
		//Creates a new 2d array of dimensions xLenth x yLength
		canvas = new int[xLength, yLength];
		FillArray(); //Fills the array
	}


	void FillArray()
	{
		//Generates a random seed based on current time and hash it to create rand Object
		seed = Time.time.ToString();
		System.Random rand = new System.Random(seed.GetHashCode());

		//iterates through each x
		for (int x = 0; x < xLength; x++)
		{
			//iterates through each y
			for (int y = 0; y < yLength; y++)
			{
				//Checks if we are at the border of the array
				if (x == 0 || x == xLength - 1 || y == 0 || y == yLength - 1)
				{
					//Forces there to be a border by setting the value equal to 1 (solid block)
					canvas[x, y] = 1;
				}
				else
				{
					//otherwise it chooses a random number and compares it to randFill value. If less, the spot is a wall
					if (rand.Next(0, 100) < randFill)
					{
						canvas[x, y] = 1;
					}
					else //otherwise the spot is empty
						canvas[x, y] = 0;
				}
			}
		}
	}

	public void SmoothIterate()
	{
		//Iterates through each element in the array
		for (int x = 0; x < xLength; x++)
		{
			for (int y = 0; y < yLength; y++)
			{
				//Gets the surroundin
				int surroundingNum = AroundCount(x, y);

				//if we aren't around 4 or more blocks
				if (surroundingNum > 4)
					canvas[x, y] = 1;
				else if (surroundingNum < 4)
					canvas[x, y] = 0;

			}
		}

		EmptyMap();
		CreateTiles();
	}

	int AroundCount(int x, int y)
	{
		//intitialize count to 0
		int count = 0;
		//iterate through each of the 8 blocks around the current one
		for (int aroundX = x - 1; aroundX <= x + 1; aroundX++)
		{
			for (int aroundY = y - 1; aroundY <= y + 1; aroundY++)
			{
				//Check that we arent looking at a border block
				if (aroundX >= 0 && aroundX < xLength && aroundY >= 0 && aroundY < yLength)
				{
					//Make sure we arent looking at ourselves
					if (aroundX != x || aroundY != y)
					{
						//if we arent, add the value of the block to wallcount
						//since walls are worth 1 and blank spaces are 0,
						//this will only count wall blocks around the center block
						count += canvas[aroundX, aroundY];
					}
				}
				else //if we are looking at a border block, just add 1. no need to do any checks since those are guaranteed wall blocks
				{
					count++;
				}
			}
		}

		return count; //return the number of wall blocks around ourselves
	}


	void CreateTiles()
	{
		//Check that the canvas isnt empty
		if (canvas != null)
		{
			//iterate through each block in the array
			for (int x = 0; x < xLength; x++)
			{
				for (int y = 0; y < yLength; y++)
				{
					//gameobject representing the block we are about to spawn
					GameObject whichCube;

					//At this point, the array is filled with 0's and 1's. We will iterate through the array and if we see a 1, spawn a solid wall block
					if (canvas[x, y] == 1)
						whichCube = wall;
					else //Ohterwise we will spawn an empty tile
						whichCube = floor;
					Vector3 pos = new Vector3(-xLength / 2 + x + .5f, 0, -yLength / 2 + y + .5f); //finds the position of the current value of the array in world space
					Instantiate(whichCube, pos, new Quaternion(90,0,0,0), mapObject.transform); //Instantiates the gameobject
				}
			}


			//Now we will choose a block to turn into 
			bool createSpawn = false;
			int attempts = 0;

			while (createSpawn == false)
			{
				int randX = UnityEngine.Random.Range(0, xLength);
				int randY = UnityEngine.Random.Range(0, yLength);

				//Finds a blank spot, checks if the block below is solid, and makes the spot the spawn point
				if (canvas[randX, randY] == 0 && canvas[randX, randY + 1] == 1)
				{
					Vector3 pos = new Vector3(-xLength / 2 + randX + .5f, 0, -yLength / 2 + randY + .5f);
					//spawnTransform.position = pos;
					Instantiate(spawnPoint, pos, new Quaternion(90, 0, 0, 0), mapObject.transform);
					createSpawn = true;
				}

				//If after 100 tries we dont find a good place, just spawn it at the center
				if (attempts > 100)
				{
					Vector3 pos = new Vector3(0, 0, 0);
					//spawnTransform.position = pos;
					Instantiate(finishLine, pos, new Quaternion(90, 0, 0, 0), mapObject.transform);
					createSpawn = true;
				}

				attempts++;

			}
			
			//Same thing with finish line
			bool createFinish = false;
			attempts = 0;

			while (createFinish == false)
			{
				int randX = UnityEngine.Random.Range(0, xLength);
				int randY = UnityEngine.Random.Range(0, yLength);

				//Finds a blank spot, checks if the block below is solid, and makes the spot the finish point
				if (canvas[randX, randY] == 0 && canvas[randX, randY + 1] == 1)
				{
					Vector3 pos = new Vector3(-xLength / 2 + randX + .5f, 0, -yLength / 2 + randY + .5f);
					Instantiate(finishLine, pos, new Quaternion(90, 0, 0, 0), mapObject.transform);
					createFinish = true;
				}

				if (attempts > 100)
				{
					Vector3 pos = new Vector3(0, yLength, 0);
					Instantiate(finishLine, pos, new Quaternion(90, 0, 0, 0), mapObject.transform);
					createFinish = true;
				}

				attempts++;
			}
		}
		mapObject.transform.Rotate(mapObject.transform.rotation.x + 90, 0, 0);

		/*if(yLength / 2 < xLength / 2)
			camera.orthographicSize = yLength / 2;
		else
			camera.orthographicSize = xLength / 2;*/

	}


	//Destroys every child gameobject in maptile which is where everything is generated under
	public void EmptyMap()
	{
		foreach (Transform mapTile in mapObject.transform)
		{
			Destroy(mapTile.gameObject);
		}
	}


	public void ChangeWidth()
	{
		widthText.text = "Width: " + wSlider.value;
		xLength = (int)wSlider.value;
	}

	public void ChangeHeight()
	{
		heightText.text = "Height: " + hSlider.value;
		yLength = (int)hSlider.value;
	}

	public void ChangeFill()
	{
		fillText.text = "Fill %: " + fSlider.value;
		randFill = (int)fSlider.value;
	}

	public void SpawnPlayers()
	{

		if (!playerToggle.isOn)
		{
			Instantiate(player1, spawnPoint.transform.position, new Quaternion(0,0,0,0));
		}

	}
}
