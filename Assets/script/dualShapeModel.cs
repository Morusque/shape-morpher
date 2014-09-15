using UnityEngine;
using System.Collections.Generic;

public class dualShapeModel : MonoBehaviour {

	public Vector2[] finalSplinePoints;
	PolygonModel[] polys = new PolygonModel[2];

	int nbVertex = 128;
	float morphPhase=0;
	float simplifyThreshold = 0.005f;

	void Start () {
	}
	
	void Update () {
	}

	public void generatePolygon(int which, int nbEdges, float phase, Vector2 dimensions) {
		if (which<0||which>=polys.Length) return;
		polys[which] = new PolygonModel(nbEdges,phase,dimensions,nbVertex,1);
		updateSimpleSplines(which);
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public void generateStar(int which, int nbEdges, float phase, Vector2 dimensions) {
		if (which<0||which>=polys.Length) return;
		polys[which] = new PolygonModel(nbEdges,0,dimensions,nbVertex,2);
		updateSimpleSplines(which);
		polys[which].compositePoly=polys[which].simplePoly;
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public void generateEllipse(int which, Vector2 dimensions) {
		if (which<0||which>=polys.Length) return;
		polys[which] = new PolygonModel(0,0,dimensions,nbVertex,0);
		updateSimpleSplines(which);
		polys[which].compositePoly=polys[which].simplePoly;
		polys[which].compositeGenerated = true;
		updateFinalSpline();
	}

	public void generateRect(int which, Vector2 dimensions) {
		if (which<0||which>=polys.Length) return;
		polys[which] = new PolygonModel(4,0.5f,dimensions,nbVertex,1);
		updateSimpleSplines(which);
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public void generateRhombus(int which, Vector2 dimensions) {
		if (which<0||which>=polys.Length) return;
		polys[which] = new PolygonModel(4,0.0f,dimensions,nbVertex,1);
		updateSimpleSplines(which);
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public void setArbitraryPolygon(int which, Vector2[] arbitraryPolygon) {
		if (which<0||which>=polys.Length) return;
		Vector2 bottomLeft = arbitraryPolygon[0];
		Vector2 topRight = arbitraryPolygon[0];
		for (int i=0;i<arbitraryPolygon.Length;i++) {
			bottomLeft.x = Mathf.Min(arbitraryPolygon[i].x,bottomLeft.x);
			bottomLeft.y = Mathf.Min(arbitraryPolygon[i].y,bottomLeft.y);
			topRight.x = Mathf.Max(arbitraryPolygon[i].x,topRight.x);
			topRight.y = Mathf.Max(arbitraryPolygon[i].y,topRight.y);
		}
		polys[which] = new PolygonModel(arbitraryPolygon.Length,0,new Vector2(topRight.x-bottomLeft.x,topRight.y-bottomLeft.y), nbVertex, 1);
		polys[which].arbitrary = true;
		polys[which].simplePoly = arbitraryPolygon;
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	void updateSimpleSplines(int which) {
		if (which<0||which>=polys.Length) return;
		if (polys[which].type==0) polys[which].simplePoly = GetComponent<shapeShifter>().generateReconstructedEllipse(polys[which].dimension,nbVertex);
		else if (polys[which].type==1) polys[which].simplePoly = GetComponent<shapeShifter>().generatePolygon(polys[which].nbEdges,polys[which].phase,polys[which].dimension);
		else if (polys[which].type==2) polys[which].simplePoly = GetComponent<shapeShifter>().generateStar(polys[which].nbEdges,polys[which].phase,polys[which].dimension);
	}

	void updateCompositeSplines(int which) {
		if (which<0||which>=polys.Length) return;
		polys[which].compositePoly = GetComponent<shapeShifter>().generateReconstructedPolygon(polys[which].simplePoly,nbVertex);
		polys[which].compositeGenerated = true;
	}

	void updateFinalSpline() {
		if (!(polys[0].compositeGenerated&&polys[1].compositeGenerated)) return;
		if (morphPhase==0) {
			finalSplinePoints = polys[0].simplePoly;
		} else if (morphPhase==1) {
			finalSplinePoints = polys[1].simplePoly;
		} else {
			finalSplinePoints = GetComponent<shapeShifter>().getPolygonMerge(polys[0].compositePoly,polys[1].compositePoly,morphPhase,polys[0].displayPhase,polys[1].displayPhase);
			// finalSplinePoints = GetComponent<shapeShifter>().simplifyShape(finalSplinePoints,simplifyThreshold);
		}
	}

	public void setDisplayPhase(int which, float displayPhase) {
		if (which<0||which>=polys.Length) return;
		polys[which].displayPhase=displayPhase;
	}

	public void setVertexCount(int number) {
		nbVertex = number;
		for (int i=0;i<polys.Length;i++) updateCompositeSplines(i);
		updateFinalSpline();
	}

	public void setSimplifyThreshold(float t) {
		simplifyThreshold = t;
		updateFinalSpline();
	}

	public void setMorphPhase(float phase) {
		morphPhase=phase;
		updateFinalSpline();
	}

	public void setEdgesCount(int which, int number) {
		if (which<0||which>=polys.Length) return;
		polys[which].nbEdges=number;
		updateSimpleSplines(which);
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public void setPhase(int which, int phase) {
		if (which<0||which>=polys.Length) return;
		polys[which].phase=phase;
		updateSimpleSplines(which);
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public void setPhase(int which, float phase) {
		if (which<0||which>=polys.Length) return;
		polys[which].phase=phase;
		updateSimpleSplines(which);
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public void setPhase(int which, Vector2 dimension) {
		if (which<0||which>=polys.Length) return;
		polys[which].dimension=dimension;
		updateSimpleSplines(which);
		updateCompositeSplines(which);
		updateFinalSpline();
	}

	public bool collidesWith(Vector2 p) {
		return containsPoint(finalSplinePoints,new Vector2(p.x-transform.position.x,p.y-transform.position.y));
	}

	static bool containsPoint (Vector2[] polyPoints, Vector2 p) {
		if (polyPoints==null) return false;
		var j = polyPoints.Length-1; 
		var inside = false; 
		for (int i = 0; i < polyPoints.Length; j = i++) { 
			if ( ((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) && 
			    (p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x)) 
				inside = !inside; 
		} 
		return inside; 
	}

	public bool hasNumberOfEdges(int[] e) {
		// returns true if one of the displayed shape has one of the possible requested number of edges
		for (int i=0;i<e.Length;i++) {
			if (morphPhase<=1) if (polys[0].nbEdges==e[i]) return true;
			if (morphPhase>0) if (polys[1].nbEdges==e[i]) return true;
		}
		return false;
	}
	
	public int[] currentNumberOfEdges() {
		// lists the number of edges of the current showed polygons
		List<int> edges = new List<int>();
		if (morphPhase<=1) edges.Add(polys[0].nbEdges);
		if (morphPhase>0) edges.Add(polys[1].nbEdges);
		return edges.ToArray();
	}

}

struct PolygonModel  {
	public bool arbitrary;
	public int nbEdges;
	public float phase;
	public float displayPhase;
	public Vector2 dimension;
	public Vector2[] simplePoly;
	public Vector2[] compositePoly;
	public bool compositeGenerated;
	public int type;
	public PolygonModel(int nbEdges, float phase, Vector2 dimension, int nbVertex, int type) {
		this.nbEdges=nbEdges;
		this.phase=phase;
		this.dimension=dimension;
		this.arbitrary=false;
		this.displayPhase=0.0f;
		this.simplePoly=new Vector2[nbEdges];
		this.compositePoly=new Vector2[nbVertex];
		this.compositeGenerated=false;
		this.type=type;// ellipse/ polygon, star
	}
}
