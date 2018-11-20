using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VRArmIK;


[Serializable]
public class MomoInputData
{
	public Vector3 lhand;
	public Vector3 rhand;
	public Vector3 head;
	public Vector3 headUp;
	public Vector3 lHandUp;
	public Vector3 rHandUp;

	public Vector3 removeXZPosition(Vector3 pos) => new Vector3(0f, pos.y, 0f);
	public Vector3 relativeXZPosition(Vector3 pos, Vector3 reference) => new Vector3(pos.x - reference.x, pos.y, pos.z - reference.z);
	public Vector3 relativeVector(Vector3 vector, Transform reference) => reference.InverseTransformVector(vector);

	public void set(Transform lHandT, Transform rHandT, Transform headT, float height)
	{
		//float height = PoseManager.Instance.playerHeightHmd;
		lhand = relativeXZPosition(lHandT.position, headT.position) / height;
		rhand = relativeXZPosition(rHandT.position, headT.position) / height;
		head = removeXZPosition(headT.position) / height;

		var angle = Mathf.Atan2(headT.forward.x, headT.forward.z);
		var rot = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0);

		lhand = rot * lhand;
		rhand = rot * rhand;
		head = rot * head;

		headUp = headT.up.normalized;
		lHandUp = relativeVector(lHandT.up, headT).normalized;
		rHandUp = relativeVector(rHandT.up, headT).normalized;

		Debug.DrawRay(headT.position, headUp);
		Debug.DrawRay(lHandT.position, lHandUp);
		Debug.DrawRay(rHandT.position, rHandUp);
	}
}

[Serializable]
public class MomoOutputData
{
	public Vector3 lelbow;
	public Vector3 relbow;
	public Vector3 lshoulder;
	public Vector3 rshoulder;
	public Vector3 hals;

	public Vector3 absoluteXZPosition(Vector3 pos, Vector3 reference) => new Vector3(pos.x + reference.x, pos.y, pos.z + reference.z);
	//public Vector3 absoluteVector(Vector3 vector, Transform reference) => reference.TransformVector(vector);

	public void get(Transform headT, float height)
	{
		//float height = PoseManager.Instance.playerHeightHmd;

		var angle = Mathf.Atan2(headT.forward.x, headT.forward.z);
		var rot = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);

		lelbow = absoluteXZPosition(rot * lelbow * height, headT.position);
		relbow = absoluteXZPosition(rot * relbow * height, headT.position);
		lshoulder = absoluteXZPosition(rot * lshoulder * height, headT.position);
		rshoulder = absoluteXZPosition(rot * rshoulder * height, headT.position);
		hals = absoluteXZPosition(rot * hals * height, headT.position);
	}
}

public class MomoIK : MonoBehaviour
{
	public AvatarVRTrackingReferences vrInput;
	public MomoAvatar avatar;
	RequestSocket client;
	public MomoInputData input;
	public MomoOutputData output;

	public float inputHeight = 0.5f;
	public float outputHeight = 0.5f;
	public float heightOffset = 0f;

	void Start()
	{

		AsyncIO.ForceDotNet.Force();

		client = new RequestSocket("tcp://192.168.0.45:8888");

	}

	void Update()
	{
		input = new MomoInputData();
		input.set(vrInput.leftHand.transform, vrInput.rightHand.transform, vrInput.head.transform, inputHeight);


		JObject jObject = new JObject();
		JArray jarray = new JArray();
		var jjarray = new JArray();

		var floats = new float[]
		{
			input.rhand.x,
			input.rhand.y,
			input.rhand.z,

			input.lhand.x,
			input.lhand.y,
			input.lhand.z,

			input.head.x,
			input.head.y,
			input.head.z,

			input.headUp.x,
			input.headUp.y,
			input.headUp.z,

			input.rHandUp.x,
			input.rHandUp.y,
			input.rHandUp.z,

			input.lHandUp.x,
			input.lHandUp.y,
			input.lHandUp.z,
		};


		//floats = new float[]
		//{0.4098174273967743f, 1.2729549407958984f,
		//	-0.10741797834634781f, -0.4676345884799957f,
		//	1.4525625705718994f, 0.4844164550304413f, 0.0f,
		//	2.5220344066619873f, 0.0f,
		//	0.07423189282417297f, 0.995918869972229f,
		//	-0.05133424699306488f, 0.8549186587333679f,
		//	0.501842200756073f, -0.13140957057476044f,
		//	-0.8807568550109863f, 0.42515695095062256f,
		//	-0.20858803391456604f};

		foreach (var myfloat in floats)
		{
			jarray.Add(myfloat);
		}

		jjarray.Add(jarray);
		jObject["inputs"] = jjarray;

		this.client.SendFrame(jObject.ToString(), more: false);
		string result;
		this.client.TryReceiveFrameString(TimeSpan.FromSeconds(1), out result);

		var outputObj = new { outputs = new float[][] { } };
		var yaoo = JsonConvert.DeserializeAnonymousType(result, outputObj);
		var outs = yaoo.outputs[0];

		output = new MomoOutputData();
		var i = 0;
		output.lelbow = new Vector3(outs[i++], outs[i++], outs[i++]);
		output.relbow = new Vector3(outs[i++], outs[i++], outs[i++]);
		output.lshoulder = new Vector3(outs[i++], outs[i++], outs[i++]);
		output.rshoulder = new Vector3(outs[i++], outs[i++], outs[i++]);
		output.hals = new Vector3(outs[i++], outs[i++], outs[i++]);
		output.get(vrInput.head.transform, outputHeight);

		avatar.updatePositions(vrInput);
		avatar.updatePositions(output);


	}

	void OnDestroy()
	{
		print("destructing");
		this.client.Close();
		NetMQConfig.ContextTerminate();
	}
}
