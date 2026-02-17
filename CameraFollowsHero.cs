using UnityEngine;

public class CameraFollowsHero : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Distance & Height")]
    public float distanceBehind = 20.0f;
    public float height = 8.0f;

    [Header("Height Limits")]
    public float minHeight = 5.0f;
    public float maxHeight = 30.0f;

    [Header("Side Angle")]
    public float sideAngle = 0.0f;          // úhel ze strany (stupně)
    public float sideAngleSpeed = 60.0f;    // rychlost změny úhlu

    [Header("Camera Controls")]
    public float heightChangeSpeed = 20.0f;
	public Transform mainHero;
	[Header("Management hero")]
	public GameObject heroObj;
	public Vector3 DestinationPoint;
	public float speed = 25.0f;
	public Animator animator;
	public void Start()
	{
		//heroObj = GameObject.Find("JennyFinal_lowpoly_z_erased");
    if (heroObj != null)
        mainHero = heroObj.transform;
	}
    void Update()
    {
		if(target!=null)heroObj = target.gameObject;
		if(mainHero!=null)
			target = mainHero;
		if(heroObj!=null)
			animator = heroObj.transform.GetComponent<Animator>();
        if (target == null)
            return;

        HandleInput();
        FollowTarget();
	//	TurnUsingControl();
    }

    // =========================
    // INPUT
    // =========================
	public void TurnUsingControl()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if(Physics.Raycast(ray, out hit, 2000) && Input.GetKeyDown(KeyCode.LeftControl))
		{
			DestinationPoint = hit.point;
		}
		
		if(DestinationPoint!=Vector3.zero)
		{
			heroObj.transform.position = Vector3.MoveTowards(heroObj.transform.position, DestinationPoint, speed*Time.deltaTime);
		}
	}
    void HandleInput()
    {
        // ZMĚNA VÝŠKY (+ / -)
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus))
        {
            height += heightChangeSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            height -= heightChangeSpeed * Time.deltaTime;
        }

        height = Mathf.Clamp(height, minHeight, maxHeight);

        // NATÁČENÍ KAMERY ZE STRANY (Q / E)
        if (Input.GetKey(KeyCode.Q))
        {
            sideAngle -= sideAngleSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            sideAngle += sideAngleSpeed * Time.deltaTime;
        }
    }

    // =========================
    // CAMERA LOGIC
    // =========================

    void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(0f, target.eulerAngles.y + sideAngle, 0f);

        Vector3 offset =
            rotation * Vector3.back * distanceBehind +
            Vector3.up * height;

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * 10.0f);
		
		if(target.name.Contains("btr"))
		{
			distanceBehind = 22.3f;
			sideAngle = -90.0f;
			height = 18.42f;
		}
		else if(target.name.Contains("Mia"))
		{
			distanceBehind = 24.4f;
			sideAngle = 0.0f;
			height = 16.52f;
		}
    }
}
