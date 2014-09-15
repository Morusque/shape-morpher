using UnityEngine;
using System.Collections.Generic;

public class shapeTester : MonoBehaviour {
	
	public IRageSpline rageSpline;
	public GameObject currentCam;
	public float morphPhase = 0.0f;
	public float speed = 1.0f;
	int iterations = 0;
	int frames=0;

	void Start () {
		for (int i=0;i<2;i++) GetComponent<dualShapeModel>().generateEllipse(i,new Vector2(1,1));
		toNextPolygon();
	}
	
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Vector2 worldPos = currentCam.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
			if (GetComponent<dualShapeModel>().collidesWith(new Vector2(worldPos.x,worldPos.y))) toNextPolygon();
		}
		/*
		if ((frames+100)%200==0) {
			Vector2 worldPos = currentCam.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
			toNextPolygon();
		}
		*/
		GetComponent<dualShapeModel>().setMorphPhase(getMorphShapeA(morphPhase));
		setRageSpline(GetComponent<dualShapeModel>().finalSplinePoints);
		if (morphPhase<1) morphPhase += Mathf.Min(Time.deltaTime*speed,1.0f-morphPhase);
		frames++;
	}

	float getMorphShapeA(float input) {
		if (input>=1.0f) return input;
		return Mathf.Sin(Mathf.Pow(input,2)*Mathf.PI)/50.0f+(-Mathf.Sin(Mathf.Pow(input,4.0f)*5.0f*Mathf.PI)*Mathf.Pow(1-input,0.75f)+input);
	}

	void toNextPolygon() {
		GetComponent<dualShapeModel>().setArbitraryPolygon(0,GetComponent<dualShapeModel>().finalSplinePoints);
		float type = Random.value;
		if (type<0.1) {
			GetComponent<dualShapeModel>().generateEllipse(1,new Vector2(Random.Range(1.0f,3.0f),Random.Range(1.0f,4.0f)));
		}
		if (type>=0.1&&type<0.6) {
			float size = Random.Range(2.0f,3.0f);
			GetComponent<dualShapeModel>().generatePolygon(1,Random.Range(3,10),Random.value,new Vector2(size,size));
		}
		if (type>=0.6&&type<0.8) {
			float size = Random.Range(2.0f,3.0f);
			GetComponent<dualShapeModel>().generateStar(1,Random.Range(3,7)*2,Random.value,new Vector2(size,size));
		}
		if (type>=0.8&&type<0.9) {
			GetComponent<dualShapeModel>().generateRect(1,new Vector2(Random.Range(1.0f,3.0f),Random.Range(1.0f,4.0f)));
		}
		if (type>=0.9&&type<1) {
			GetComponent<dualShapeModel>().generateRhombus(1,new Vector2(Random.Range(1.0f,3.0f),Random.Range(1.0f,4.0f)));
		}
		// if (iterations%10==0) GetComponent<dualShapeModel>().generateEllipse(1,new Vector2(1,1));
		morphPhase=0.0f;
		iterations++;
	}

	void setRageSpline(Vector2[] splinePoints) {
		rageSpline = GetComponent<RageSpline>();
		rageSpline.ClearPoints();
		for (int i=0;i<splinePoints.Length;i++) {
			rageSpline.AddPoint(0, new Vector3(splinePoints[i].x,splinePoints[i].y,0),new Vector3(0,0,0));
		}
		rageSpline.RefreshMesh();
	}
	
}

