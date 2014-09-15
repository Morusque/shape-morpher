using UnityEngine;
using System.Collections.Generic;

public class shapeShifter : MonoBehaviour {

	void Start () {
	}
	
	void Update () {	
	}
	
	public Vector2[] generatePolygon(int nbPolygonPoints, float polygonPhase, Vector2 polygonDimensions) {
		Vector2[] points = new Vector2[nbPolygonPoints];
		while (polygonPhase<0||polygonPhase>=(Mathf.PI*2)) polygonPhase = (polygonPhase+Mathf.PI*2)%(Mathf.PI*2);
		for (int j=0;j<nbPolygonPoints;j++) {
			float phase=(float)(j+polygonPhase)*Mathf.PI*2.0f/nbPolygonPoints;
			phase%=(Mathf.PI*2);
			points[j]=new Vector2(Mathf.Cos(phase)*polygonDimensions.x,Mathf.Sin(phase)*polygonDimensions.y);
		}
		return points;
	}

	public Vector2[] generateStar(int nbPolygonPoints, float polygonPhase, Vector2 polygonDimensions) {
		Vector2[] points = new Vector2[nbPolygonPoints];
		while (polygonPhase<0||polygonPhase>=(Mathf.PI*2)) polygonPhase = (polygonPhase+Mathf.PI*2)%(Mathf.PI*2);
		for (int j=0;j<nbPolygonPoints;j++) {
			float phase=((float)j+polygonPhase)*Mathf.PI*2.0f/nbPolygonPoints;
			phase%=(Mathf.PI*2);
			points[j]=new Vector2(Mathf.Cos(phase)*polygonDimensions.x*((j%2.0f)*0.5f+0.5f),Mathf.Sin(phase)*polygonDimensions.y*((j%2.0f)*0.5f+0.5f));
		}
		return points;
	}

	public Vector2[] generateReconstructedEllipse(Vector2 dimension, int nbVertex) {
		Vector2[] splinePoints = new Vector2[nbVertex];
		for (int i=0;i<nbVertex;i++) {
			float phase=(float)i*Mathf.PI*2.0f/nbVertex;
			splinePoints[i] = new Vector2(Mathf.Cos(phase)*dimension.x,Mathf.Sin(phase)*dimension.y);
		}
		return splinePoints;
	}

	public Vector2[] generateReconstructedPolygon(Vector2[] points, int nbVertex) {
		Vector2[] polarPoints = new Vector2[points.Length];
		for (int j=0;j<points.Length;j++) {
			polarPoints[j] = new Vector2((Mathf.Atan2(points[j].y,points[j].x)+(Mathf.PI*2))%(Mathf.PI*2),points[j].magnitude);
		}
		Vector2[] splinePoints = new Vector2[nbVertex];
		for (int i=0;i<nbVertex;i++) {
			float phase=(float)i*Mathf.PI*2.0f/nbVertex;
			int lastPoint=-1;
			int nextPoint=-1;
			for (int j=0;j<points.Length;j++) {
				float phaseDifference = vrMax(phase,polarPoints[j].x,Mathf.PI*2);
				if (phaseDifference<=0) {
					if (lastPoint==-1) {
						lastPoint=j;
					}else if (Mathf.Abs(phaseDifference)<=Mathf.Abs(vrMax(phase,polarPoints[lastPoint].x,Mathf.PI*2))) {
						lastPoint=j;
					}
				}
				if (phaseDifference>=0) {
					if (nextPoint==-1) {
						nextPoint=j;
					}else if (Mathf.Abs(phaseDifference)<=Mathf.Abs(vrMax(phase,polarPoints[nextPoint].x,Mathf.PI*2))) {
						nextPoint=j;
					}
				}
			}
			float positionOnLine = 0.0f;
			if (lastPoint!=nextPoint) {
				float totalDist = Vector2.Distance(points[lastPoint],points[nextPoint]);
				float vertexAngle = vrMax((Mathf.Atan2(-points[lastPoint].y,-points[lastPoint].x)+(Mathf.PI*2))%(Mathf.PI*2),(Mathf.Atan2(points[nextPoint].y-points[lastPoint].y,points[nextPoint].x-points[lastPoint].x)+(Mathf.PI*2))%(Mathf.PI*2),Mathf.PI*2.0f);
				float subPhaseAngle = vrMax(phase,polarPoints[lastPoint].x,Mathf.PI*2);
				float shiftedAngle = Mathf.PI-(subPhaseAngle+Mathf.PI/2.0f+vertexAngle);
				float projection = polarPoints[lastPoint].y*Mathf.Sin(subPhaseAngle);
				float partialDist = Mathf.Abs(Mathf.Tan(shiftedAngle)*(projection/Mathf.Sin(shiftedAngle)));
				positionOnLine = partialDist/totalDist;
			}
			float xS=points[lastPoint].x*(1.0f-positionOnLine)+points[nextPoint].x*positionOnLine;
			float yS=points[lastPoint].y*(1.0f-positionOnLine)+points[nextPoint].y*positionOnLine;
			splinePoints[i] = new Vector2(xS,yS);
		}
		return splinePoints;
	}

	public Vector2[] simplifyShape(Vector2[] shape, float threshold) {
		if (threshold==0) return shape;
		List<Vector2> result = new List<Vector2>();
		List<float> angles = new List<float>();
		for (int i=0;i<shape.Length;i++) {
			result.Add(shape[i]);
			float anglePrevious = (Mathf.Atan2(shape[i].y-shape[(i-1+shape.Length)%shape.Length].y,shape[i].x-shape[(i-1+shape.Length)%shape.Length].x)+Mathf.PI*2.0f)%(Mathf.PI*2.0f);
			float angleNext = (Mathf.Atan2(shape[(i+1)%shape.Length].y-shape[i].y,shape[(i+1)%shape.Length].x-shape[i].x)+Mathf.PI*2.0f)%(Mathf.PI*2.0f);
			angles.Add(vrMax(anglePrevious,angleNext,Mathf.PI*2.0f));
		}
		bool change = true;
		while (change && angles.Count>3) {
			change=false;
			int smallest=0;
			for (int i=1;i<angles.Count;i++) {
				if (Mathf.Abs(angles[i])<Mathf.Abs(angles[smallest])) smallest=i;
			}
			if (Mathf.Abs(angles[smallest])<threshold) {
				angles.Remove(angles[smallest]);
				result.Remove(result[smallest]);
				for (int j = 0;j < 1;j++) {
					int i=(smallest-1+j+angles.Count)%angles.Count;
					float anglePrevious = (Mathf.Atan2(result[i].y-result[(i-1+result.Count)%result.Count].y,result[i].x-result[(i-1+result.Count)%result.Count].x)+Mathf.PI*2.0f)%(Mathf.PI*2.0f);
					float angleNext = (Mathf.Atan2(result[(i+1)%result.Count].y-result[i].y,result[(i+1)%result.Count].x-result[i].x)+Mathf.PI*2.0f)%(Mathf.PI*2.0f);
					angles[i] = (vrMax(anglePrevious,angleNext,Mathf.PI*2.0f));
				}
				change=true;
			}
		}
		Vector2[] resultArray=result.ToArray();
		result.Clear();
		angles.Clear();
		return resultArray;
	}

	public Vector2[] getPolygonMerge(Vector2[] p1, Vector2[] p2, float amount) {
		Vector2[] result = new Vector2[Mathf.Min(p1.Length,p2.Length)];
		for (int i=0;i<result.Length;i++) {
			result[i] = new Vector2((p2[i].x*amount+p1[i].x*(1-amount)),(p2[i].y*amount+p1[i].y*(1-amount)));
		}
		return result;
	}

	public Vector2[] getPolygonMerge(Vector2[] p1, Vector2[] p2, float amount, float phaseA, float phaseB) {
		Vector2[] result = new Vector2[Mathf.Min(p1.Length,p2.Length)];
		for (int i=0;i<result.Length;i++) {
			int iA=Mathf.FloorToInt(i+phaseA*result.Length);
			int iB=Mathf.FloorToInt(i+phaseB*result.Length);
			while (iA<0||iA>=result.Length) iA = (iA+result.Length)%result.Length;
			while (iB<0||iB>=result.Length) iB = (iB+result.Length)%result.Length;
			result[i] = new Vector2((p2[iB].x*amount+p1[iA].x*(1.0f-amount)),(p2[iB].y*amount+p1[iA].y*(1.0f-amount)));
		}
		return result;
	}

	public float vrMax(float a, float b, float m) {
		float d1=b-a;
		if (d1>m/2) {
			d1=d1-m;
		}
		if (d1<-m/2) {
			d1=d1+m;
		}
		return d1;
	}

}
