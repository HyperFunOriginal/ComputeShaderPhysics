using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Cubes : MonoBehaviour
{

	public struct Particle
	{
		public float x, y, z, vx, vy, vz;
		public float temp;
		public float r, g, b;
	}
	public float tempscale;
	public GameObject Prefab;
	GameObject[] particlearr;
	public int resolution;
	public ComputeShader planet;
	[Range(0f, 1f)]
	public Particle[] part;
	[Range(0f, 2f)]
	public float timestep;
	[Range(1f, 10f)]
	public float accuracy, hyperwarp;
	// Use this for initialization

	void Start()
	{
		InitializeFluid();
		//StartCoroutine(LaterUpdate());
	}
    void LateUpdate()
    {
	//	yield return new WaitForSeconds(1f);
		//while (true)
	//	{
			OnGPUSolve();
			//yield return new WaitForEndOfFrame();
		//}
    }

	void Update()
    {
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Vector3 avg = Vector3.zero;
			for (int i = 0; i < part.Length; i++)
			{
				if (part[i].x == 0f && part[i].z == 0f)
				{
					avg += avg / (i + 1);
				}
				else
				{
					avg += new Vector3(part[i].x, part[i].y, part[i].z);
				}
			}
			avg /= part.Length;
			for (int i = 0; i < part.Length; i++)
            {
				float mag = Mathf.Sqrt((part[i].z - avg.z) * (part[i].z - avg.z) + (part[i].x - avg.x) * (part[i].x - avg.x) + 2f);
				part[i].vy = 0f;
				part[i].vx = (part[i].z - avg.z) * 27.3f / (mag * mag);
				part[i].vz = (part[i].x - avg.x) * -27.3f / (mag * mag);
			}
        }

		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			Vector3 avg = Vector3.zero;
			for (int i = 0; i < part.Length; i++)
			{
				if (part[i].x == 0f && part[i].z == 0f)
				{
					avg += avg / (i + 1);
				}
				else
				{
					avg += new Vector3(part[i].x, part[i].y, part[i].z);
				}
			}
			avg /= part.Length;
			for (int i = 0; i < part.Length; i++)
			{
				float mag = Mathf.Sqrt((part[i].z - avg.z) * (part[i].z - avg.z) + (part[i].x - avg.x) * (part[i].x - avg.x) + 2f);
				part[i].vx += (part[i].z - avg.z) * 12.32f / (mag * mag);
				part[i].vz -= (part[i].x - avg.x) * 12.32f / (mag * mag);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			Vector3 avg = Vector3.zero;
			for (int i = 0; i < part.Length; i++)
			{
				if (part[i].x == 0f && part[i].z == 0f)
				{
					avg += avg / (i + 1);
				}
				else
				{
					avg += new Vector3(part[i].x, part[i].y, part[i].z);
				}
			}
			avg /= part.Length;
			for (int i = 0; i < part.Length; i++)
			{
				part[i].vz -= (part[i].z - avg.z) * 0.02f;
				part[i].vx -= (part[i].x - avg.x) * 0.02f;
				part[i].vy -= (part[i].y - avg.y) * 0.02f;
			}
		}

		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			Vector3 avg = Vector3.zero;
			for (int i = 0; i < part.Length; i++)
			{
				if (part[i].x == 0f && part[i].z == 0f)
				{
					avg += avg / (i + 1);
				}
				else
				{
					avg += new Vector3(part[i].x, part[i].y, part[i].z);
				}
			}
			avg /= part.Length;
			for (int i = 0; i < part.Length; i++)
			{
				float dx = part[i].x - avg.x;
				float dz = part[i].z - avg.z;
				float dy = part[i].y - avg.y;

				part[i].x = avg.x + dx * 3f;
				part[i].z = avg.z + dz * 3f;
				part[i].y = avg.y + dy * 3f;
			}
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			for (int i = 0; i < part.Length; i++)
			{
				part[i].vx--;
			}
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			for (int i = 0; i < part.Length; i++)
			{
				part[i].vx++;
			}
		}


		if (Input.GetKeyDown(KeyCode.Alpha1))
        {
			for (int i = 0; i < part.Length; i++)
            {
				part[i].vy = 0f;
				part[i].vx = 0f;
				part[i].vz = 0f;
			}
			part[Random.Range(0, part.Length - 1)].y += Random.Range(-0.05f, 0.05f);
        }
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			for (int i = 0; i < part.Length; i++)
			{
				part[i].temp = 300f;
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			for (int i = 0; i < part.Length; i++)
			{
				part[i].vx /= 1.2f;
				part[i].vy /= 1.2f;
				part[i].vz /= 1.2f;
			}
		}
	}

    public void InitializeFluid()
	{
		part = new Particle[resolution];
		for (int i = 0; i < part.Length; i++)
        {

			part[i].z = (i / Mathf.Sqrt(resolution)) * 1.4f;

			part[i].vy = Random.Range(-0.0001f, 0.0001f);
			part[i].x = (i % Mathf.Sqrt(resolution)) * 1.4f;
			Instantiate(Prefab, new Vector3(part[i].x, part[i].z), Quaternion.identity);
		}
		particlearr = GameObject.FindGameObjectsWithTag("Particle");
	}

	public void OnGPUSolve()
	{
		int sizeOfFluid = sizeof(float) * 10;
		ComputeBuffer fluidbuffer = new ComputeBuffer(resolution, sizeOfFluid);
		fluidbuffer.SetData(part);
		planet.SetBuffer(0, "fluidBuffer", fluidbuffer);
		planet.SetFloat("resolution", resolution);
		planet.SetFloat("timestep", timestep);
		planet.SetFloat("accuracy", accuracy);


		for (int i = 0; i < Mathf.Floor(accuracy * hyperwarp); i++)
		{
			planet.Dispatch(0, Mathf.CeilToInt(resolution / 64f), 1, 1);
		}
		fluidbuffer.GetData(part);

		fluidbuffer.Dispose();
		
			for (int i = 0; i < particlearr.Length; i++)
			{
				if (particlearr[i] != null)
				{
					particlearr[i].transform.position = new Vector3(part[i].x, part[i].y, part[i].z);
					//particlearr[i].GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.black, Color.white, part[i].temp / (tempscale + 1f));
					particlearr[i].GetComponent<MeshRenderer>().material.color = new Color(part[i].r, part[i].g, part[i].b);
                }
			}
	}
}