
using System;
using UnityEngine;

public class Window: MonoBehaviour
{
	public enum WindowSize
	{
		Small,
		Medium,
		Large
	};
	
	public enum WindowState
	{
		Static,
		Moving
	}
	
	public enum WindowStatus
	{
		Normal,
		Ok,
		Error,
		Alert,
		Highlighted,
		Selected
	}
	
	public const int NumApps = 1;	
	public static Texture2D[] apps = new Texture2D[NumApps];
									   
	//public static Texture2D BlankPlate = Resources.Load<Texture2D>("Blank");
															
	public const float MoveDurDefault = 1.0f;															   	   									   	   
	private static Color StatusNormalColour = Color.blue;
	private static Color StatusAlertColour = Color.yellow;
	private static Color StatusOkColour = new Color(0.5f, 1, 0.5f);
	private static Color StatusErrorColour = Color.red;
	private static Color StatusHighlightedColour = new Color(0.5f, 1, 0.5f);
	private static Color StatusSelectedColour = new Color(0.5f, 1, 0.5f);

	public static readonly Vector3 WindowSizeSmall = new Vector3 (0.12f, 0.08f, 0.001f);
	public static readonly Vector3 WindowSizeMed = new Vector3 (0.16f, 0.12f, 0.001f);
	//public static readonly Vector3 WindowSizeLarge = new Vector3 (0.2f, 0.15f, 0.001f);
	public static readonly Vector3 WindowSizeLarge = new Vector3 (0.2f, 0.1125f, 0.001f);
	public static readonly Vector3 WindowSizeShrunken = new Vector3 (0.01f, 0.01f, 0.001f);
	
	
	private int windowId;
	private GameObject cuboid;
	private GameObject face;
	private Vector3 homePosition; // local position relative to viewer position in default personal cockpit layout
	//private Quaternion homeRot;
	private WindowSize homeSize;  // base size in home position
	private Vector3 posStart;	//for motion
	private Vector3 posEnd;
	private Quaternion rotStart;
	private Quaternion rotEnd;
	private Vector3 sizeStart;
	private Vector3 sizeEnd;
	private WindowState state = WindowState.Static;
	private WindowStatus status = WindowStatus.Normal;
	private WindowStatus postStatus = WindowStatus.Normal;
	private float moveDur;	//duration of motion in seconds
	private float moveTimer; //current time expired out of total duration
	private int windowToLeft; //index of window to left of this one in array (-1 if in leftmost column)
	private int windowAbove;	//index of window above this one in array (-1 if in top row)
	private bool pinned = false;
	private TextMesh textFace = null;
	private bool isOrtho = false;
	private Vector3 surfacePos;
	private Quaternion surfaceRot;
	private bool shrunken = false;
	
	public static void Init()
	{
		for (int j = 0; j < Window.NumApps; j++)
		{
			String imageName = "app" + (j + 1).ToString();
			Texture2D img = Resources.Load<Texture2D>(imageName);
			apps[j] = img;
		}
	}
	
	//Set appnum to 100+ for study plates instead of regular apps
	public static GameObject NewWindow(Vector3 position, Quaternion rotation, WindowSize size, Color colour, int id, bool showFace, int appNum)
	{
		GameObject newWindow = GameObject.CreatePrimitive(PrimitiveType.Cube);
		newWindow.transform.position = position;
		newWindow.transform.rotation = rotation;
		newWindow.name = "window" + id.ToString ();
		Vector3 dimensions = Vector3.zero;

		switch (size)
		{
		case WindowSize.Small:
			dimensions = Window.WindowSizeSmall;
			break;
		case WindowSize.Medium:
			dimensions = Window.WindowSizeMed;
			break;
		case WindowSize.Large:
			dimensions = Window.WindowSizeLarge;
			break;
		}
		
		newWindow.transform.localScale = dimensions;
		
		//Add this script and set id
		newWindow.AddComponent("Window");
		Window data = newWindow.GetComponent<Window>();
		data.WindowId = id;
		data.cuboid = newWindow;
		data.homeSize = size;
		
		//Add textured face to the cube
		if (showFace)
		{
			GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			data.face = quad;
			quad.transform.parent = newWindow.transform;
			quad.transform.localScale = new Vector3 (0.95f, 0.95f, 1);//new Vector2(dimensions.x, dimensions.y);
			quad.transform.localPosition = new Vector3(0, 0, -0.75f);
			quad.transform.localRotation = Quaternion.identity;
			Texture2D face;
			
			face = apps[appNum];
				
			quad.renderer.material.mainTexture = face;
			//quad.renderer.material.shader = Shader.Find("Self-Illumin/Diffuse");
			quad.renderer.material.shader = Shader.Find("Diffuse");
			//quad.renderer.material.shader = Shader.Find("Custom/IllumOnTop");
			
			//quad.renderer.material.shader = Shader.
			newWindow.renderer.material.color = StatusNormalColour;
			
		}
		else
		{	
			newWindow.renderer.material.color = colour;
		}
		
		newWindow.renderer.material.shader = Shader.Find("Diffuse");
		//newWindow.renderer.material.shader = Shader.Find("Self-Illumin/Diffuse");
		//newWindow.renderer.material.shader = Shader.Find("Custom/IllumOnTop");
		data.state = Window.WindowState.Static;
		
		//Add collider
		//newWindow.AddComponent<BoxCollider>();
		newWindow.AddComponent<BoxCollider>();
				
		
		return newWindow;
	}

	void OnDestroy()
	{
		GameObject.Destroy(cuboid);
		GameObject.Destroy(face);
	}
	
	public GameObject WindowGameObject
	{
		get {return cuboid;}
	}
	
	public int WindowId
	{
		get {return windowId;}
		set {windowId = value;}
	}
	
	public Vector3 HomePosition
	{
		get {return homePosition;}
		set {homePosition = value;}
	}
	
//	public Quaternion HomeRotation
//	{
//		get {return homeRot;}
//		set {homeRot = value;}
//	}
	
	public Vector3 HomeDimensions
	{
		get 
		{
			Vector3 size = Vector3.zero;
			
			switch(homeSize)
			{
			case WindowSize.Small:
			{
				size = WindowSizeSmall;
				break;
			}
			case WindowSize.Medium:
			{
				size = WindowSizeMed;
				break;
			}
			case WindowSize.Large:
			{
				size = WindowSizeLarge;
				break;
			}
			}
		
			return size;
		}
	}
	
	public WindowSize HomeSize
	{
		get {return homeSize;}
		set {homeSize = value;}
	}
	
	
	public WindowState State
	{
		get {return state;}
	}
	
	public WindowStatus Status
	{
		get {return status;}
		set
		{
			status = value;
			
			switch (value)
			{
			case WindowStatus.Alert:
				renderer.material.color = StatusAlertColour;
				break;
			case WindowStatus.Ok:
				renderer.material.color = StatusOkColour;
				break;
			case WindowStatus.Error:
				renderer.material.color = StatusErrorColour;
				break;
			case WindowStatus.Normal:
				renderer.material.color = StatusNormalColour;
				break;
			case WindowStatus.Highlighted:
				renderer.material.color = StatusHighlightedColour;
				break;
			case WindowStatus.Selected:
				renderer.material.color = StatusSelectedColour;
				break;
			}
		}
	}
	
	public bool Shrunken
	{
		get {return shrunken;}
		set {shrunken = value;}
	}
	
	public bool Pinned
	{
		get {return pinned;}
		set {pinned = value;}
	}
	
	public bool Ortho
	{
		get {return isOrtho;}
		set {isOrtho = value;}
	}
	
	public Vector3 SurfacePos
	{
		get {return surfacePos;}
		set {surfacePos = value;}
	}
	
	
	public Quaternion SurfaceRot
	{
		get {return surfaceRot;}
		set {surfaceRot = value;}
	}
	
	public string Text
	{
		get 
		{
			string text = "";
			
			if (textFace != null)
			{
				return textFace.text;
			}
			
			return text;
		}
		
		set	
		{
			if (textFace != null)
			{
				textFace.text = value;
			}
		}
	}
	public void MoveTo (Vector3 newPos, Quaternion newRot, Vector3 newSize, bool animate, WindowStatus postStatus)
	{ 
		//if (!pinned)
		//{
			if (animate)
			{
				posStart = transform.position;
				posEnd = newPos;
				rotStart = transform.rotation;
				rotEnd = newRot;
				sizeStart = transform.localScale;
				sizeEnd = newSize;
				moveDur = MoveDurDefault;
				moveTimer = 0; 
				this.postStatus = postStatus;
				state = WindowState.Moving;
			}
			else
			{
				transform.position = newPos;
				transform.rotation = newRot;
				transform.localScale = newSize;
				Status = postStatus;
			}
		//}
	}
	
	public static Quaternion GetRotationFacing(GameObject window, Vector3 pos, Vector3 faceNormal, Vector3 viewerPos)
	{
		Vector3 toViewer = viewerPos - pos;
		float angleDif = Vector3.Angle(new Vector3(faceNormal.x, 0, faceNormal.z), new Vector3(toViewer.x, 0, toViewer.z));
		
		if(faceNormal.y > 0.7)
		{
			faceNormal = new Vector3(toViewer.x, 10, toViewer.z).normalized;
		}
		
		Quaternion rotToNormal = Quaternion.FromToRotation(Vector3.back, faceNormal);
		Quaternion windowRot = Quaternion.Euler(rotToNormal.eulerAngles.x, rotToNormal.eulerAngles.y, 0);
	
		
		return windowRot;
	}
	
	public static void SetPositionFacing(GameObject window, Vector3 pos, Vector3 faceNormal, Vector3 viewerPos)
	{
		Vector3 toViewer = viewerPos - pos;
		float angleDif = Vector3.Angle(new Vector3(faceNormal.x, 0, faceNormal.z), new Vector3(toViewer.x, 0, toViewer.z));
		
		if(faceNormal.y > 0.7)
		{
			faceNormal = new Vector3(toViewer.x, 10, toViewer.z).normalized;
			//window.renderer.material.color = Color.red;
		}
//		else
//		{
//			window.renderer.material.color = Color.blue;
//		}
		
		window.transform.position = pos;
		Quaternion rotToNormal = Quaternion.FromToRotation(Vector3.back, faceNormal);
	 	Quaternion windowRot = Quaternion.Euler(rotToNormal.eulerAngles.x, rotToNormal.eulerAngles.y, 0);
	 	
//		if (angleDif > 90)
//		{ 
//			windowRot = Quaternion.Euler(rotToNormal.eulerAngles.x, (rotToNormal.eulerAngles.y + 180) % 360, 0);
//	    }
	     
		window.transform.rotation = windowRot;
	}
	
	// Update is called once per frame
	void Update()
	{	
		if (state == WindowState.Moving)
		{
			float inc = Time.deltaTime;
			moveTimer += inc;
			transform.rotation = Quaternion.Slerp(rotStart, rotEnd, moveTimer);
			transform.position = Vector3.Lerp(posStart, posEnd, moveTimer);
			transform.localScale = Vector3.Lerp(sizeStart, sizeEnd, moveTimer);
			
			if (moveTimer >= moveDur)
			{
				state = WindowState.Static;
				Status = postStatus;
			}
		}
		else
		{
			transform.Rotate(new Vector3(0, 45, 0) * Time.deltaTime, Space.World);
		}
	}
	
	public Vector3 GetScreenCoordsPosition(float x, float y)
	{
		Vector3 pos = face.transform.position;
		Vector3 normal = -face.transform.forward;
		Quaternion rot = face.transform.rotation;
	
		//Find window corners - when facing window 'left' is the cube's right (+x) side
		//Vector3 windowSize = face.renderer.bounds.size; 
		Vector3 windowSize = new Vector3 (HomeDimensions.x * face.transform.localScale.x, HomeDimensions.y * face.transform.localScale.y, 0);
		Vector3 topLeftOffset = new Vector3(-windowSize.x / 2.0f, windowSize.y / 2.0f, 0);
		Vector3 screenOffset = new Vector3(x * windowSize.x, -y * windowSize.y, 0);
		Vector3 screenPos = pos + (rot * (topLeftOffset + screenOffset));	//top left
		
		return screenPos;
	}
}


