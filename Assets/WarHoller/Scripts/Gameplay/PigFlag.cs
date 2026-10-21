using UnityEngine;
using WarHoller.Art;
using WarHoller.Core;

namespace WarHoller.Gameplay
{
    /// <summary>
    /// CTF "pig" flag: sits at a team base until an enemy picks it up and carries it overhead.
    /// </summary>
    public class PigFlag : MonoBehaviour
    {
        public FactionIds OwnerTeam;
        public Vector3 HomePosition;
        public Health Carrier;
        public float PickupRadius = 2.2f;

        Transform _visual;
        float _dropReturnTimer = -1f;
        const float DropReturnSeconds = 12f;

        public bool IsHome => Carrier == null && (transform.position - HomePosition).sqrMagnitude < 0.25f;
        public bool IsCarried => Carrier != null;

        public static PigFlag Create(Transform parent, FactionIds owner, Vector3 home)
        {
            var go = new GameObject(owner == FactionIds.Hatfield ? "HatfieldPig" : "McCoyPig");
            go.transform.SetParent(parent, false);
            go.transform.position = home;
            var pig = go.AddComponent<PigFlag>();
            pig.OwnerTeam = owner;
            pig.HomePosition = home;
            pig.BuildVisual();
            return pig;
        }

        void BuildVisual()
        {
            var pink = MaterialUtil.CreateColor(new Color(0.92f, 0.55f, 0.62f));
            var snout = MaterialUtil.CreateColor(new Color(0.85f, 0.45f, 0.5f));
            _visual = new GameObject("PigVisual").transform;
            _visual.SetParent(transform, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(_visual, false);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localScale = new Vector3(0.9f, 0.7f, 1.2f);
            body.GetComponent<Renderer>().material = pink;
            Object.Destroy(body.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(_visual, false);
            head.transform.localPosition = new Vector3(0f, 0.55f, 0.55f);
            head.transform.localScale = new Vector3(0.55f, 0.5f, 0.55f);
            head.GetComponent<Renderer>().material = pink;
            Object.Destroy(head.GetComponent<Collider>());

            var nose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nose.name = "Snout";
            nose.transform.SetParent(_visual, false);
            nose.transform.localPosition = new Vector3(0f, 0.5f, 0.85f);
            nose.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            nose.transform.localScale = new Vector3(0.25f, 0.12f, 0.25f);
            nose.GetComponent<Renderer>().material = snout;
            Object.Destroy(nose.GetComponent<Collider>());

            var earL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            earL.name = "EarL";
            earL.transform.SetParent(_visual, false);
            earL.transform.localPosition = new Vector3(-0.22f, 0.78f, 0.45f);
            earL.transform.localScale = new Vector3(0.12f, 0.18f, 0.08f);
            earL.GetComponent<Renderer>().material = pink;
            Object.Destroy(earL.GetComponent<Collider>());

            var earR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            earR.name = "EarR";
            earR.transform.SetParent(_visual, false);
            earR.transform.localPosition = new Vector3(0.22f, 0.78f, 0.45f);
            earR.transform.localScale = new Vector3(0.12f, 0.18f, 0.08f);
            earR.GetComponent<Renderer>().material = pink;
            Object.Destroy(earR.GetComponent<Collider>());

            var leg = MaterialUtil.CreateColor(new Color(0.75f, 0.4f, 0.45f));
            foreach (var offset in new[]
                     {
                         new Vector3(-0.25f, 0.15f, 0.3f), new Vector3(0.25f, 0.15f, 0.3f),
                         new Vector3(-0.25f, 0.15f, -0.3f), new Vector3(0.25f, 0.15f, -0.3f)
                     })
            {
                var l = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                l.name = "Leg";
                l.transform.SetParent(_visual, false);
                l.transform.localPosition = offset;
                l.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);
                l.GetComponent<Renderer>().material = leg;
                Object.Destroy(l.GetComponent<Collider>());
            }

            // Subtle collar so teams read at distance
            var collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            collar.name = "Collar";
            collar.transform.SetParent(_visual, false);
            collar.transform.localPosition = new Vector3(0f, 0.55f, 0.35f);
            collar.transform.localScale = new Vector3(0.5f, 0.04f, 0.5f);
            collar.GetComponent<Renderer>().material = OwnerTeam == FactionIds.Hatfield
                ? MaterialUtil.CreateColor(new Color(0.35f, 0.45f, 0.75f))
                : MaterialUtil.CreateColor(new Color(0.75f, 0.25f, 0.2f));
            Object.Destroy(collar.GetComponent<Collider>());
        }

        public void TryPickup(Health candidate, FactionIds candidateTeam)
        {
            if (Carrier != null || candidate == null || candidate.IsDead) return;
            if (candidateTeam == OwnerTeam) return; // can only steal enemy pig
            if (Vector3.Distance(candidate.transform.position, transform.position) > PickupRadius) return;
            AttachTo(candidate);
        }

        public void AttachTo(Health carrier)
        {
            Carrier = carrier;
            _dropReturnTimer = -1f;
            transform.SetParent(carrier.transform, false);
            transform.localPosition = new Vector3(0f, 2.05f, 0f); // over head
            transform.localRotation = Quaternion.identity;
        }

        public void DropAt(Vector3 worldPos)
        {
            Carrier = null;
            transform.SetParent(null, true);
            transform.position = new Vector3(worldPos.x, 0.05f, worldPos.z);
            transform.rotation = Quaternion.identity;
            _dropReturnTimer = DropReturnSeconds;
        }

        public void ReturnHome()
        {
            Carrier = null;
            transform.SetParent(null, true);
            transform.position = HomePosition;
            transform.rotation = Quaternion.identity;
            _dropReturnTimer = -1f;
        }

        void Update()
        {
            if (Carrier != null)
            {
                if (Carrier.IsDead)
                {
                    var pos = Carrier.transform.position;
                    DropAt(pos);
                }
                return;
            }

            if (_dropReturnTimer > 0f)
            {
                _dropReturnTimer -= Time.deltaTime;
                if (_dropReturnTimer <= 0f)
                    ReturnHome();
            }
        }
    }
}
