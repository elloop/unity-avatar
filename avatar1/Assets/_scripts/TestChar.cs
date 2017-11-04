using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestChar : MonoBehaviour {

    public Transform src;
    public Transform target;

    public static TestChar instance_;

    public Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> meshData
        = new Dictionary<string, Dictionary<string, SkinnedMeshRenderer>>();

    /// <summary>
    /// version 1.0
    /// </summary>
    // Use this for initialization
    /*
    void Start() {
        readMeshData();

        foreach (KeyValuePair<string, Dictionary<string, SkinnedMeshRenderer>> item in meshData) {
            string part = item.Key;
            //Debug.Log("part name: " + part);

            foreach (KeyValuePair<string, SkinnedMeshRenderer> me in item.Value) {
                string parti = me.Key;
                //Debug.Log("part small: " + parti);
                //Debug.Log("smr: " + me.Value);
            }
        }

        rebuildTarget();

        target.GetComponent<Animation>().Play("walk");
    }
    */

    private Dictionary<string, SkinnedMeshRenderer> tSmr_ = 
        new Dictionary<string, SkinnedMeshRenderer>();

    private Dictionary<string, Dictionary<string, Transform>> objs_ = 
        new Dictionary<string, Dictionary<string, Transform>>();

    private Transform[] hips_;
    /// <summary>
    /// 
    /// version 2.0 add game object
    /// </summary>
    void Start() {
        readMeshObjs();
        setDefaultSkin();
        target.GetComponent<Animation>().Play("walk");


        instance_ = this;

        for (int i = 0; i < btnNames_.Length; i++) {
            string name = btnNames_[i];
            GameObject.Find(name).GetComponent<Button>().onClick.AddListener(actions_[i]);
        }
    }

    string[] btnNames_ = { "hair", "face", "shoes", "pants" };
    UnityAction[] actions_ = { changeHair, changeFace, changeShoes, changePants };

    static void changeHair() {
        Debug.Log("changeHair ");
        instance_.changePart("hair", "2");
    }

    static void changeFace() {
        Debug.Log("changeFace");
        instance_.changePart("face", "2");
    }

    static void changeShoes() {
        Debug.Log("changeShoes");
        instance_.changePart("shoes", "2");
    }

    static void changePants() {
        Debug.Log("changePants");
        instance_.changePart("pants", "2");
    }

    /// <summary>
    /// read src skin mesh into game obj.
    /// </summary>
    void readMeshObjs() {
        SkinnedMeshRenderer[] smrs = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in smrs) {
            string[] partsName = smr.name.Split('-');
            if (!objs_.ContainsKey(partsName[0])) {
                objs_.Add(partsName[0], new Dictionary<string, Transform>());
                GameObject partObj = new GameObject();
                partObj.name = partsName[0];
                partObj.transform.parent = target;
                tSmr_.Add(partsName[0], partObj.AddComponent<SkinnedMeshRenderer>());
            }
            objs_[partsName[0]].Add(partsName[1], smr.transform);
        }
        hips_ = target.GetComponentsInChildren<Transform>();
    }

    void setDefaultSkin() {
        foreach (KeyValuePair<string, Dictionary<string, Transform>> obj in objs_) {
            changePart(obj.Key, "1");
        }
    }

    void changePart(string part, string i) {
        SkinnedMeshRenderer smr = objs_[part][i].GetComponent<SkinnedMeshRenderer>();

        List<Transform> bonesUnderSmr = new List<Transform>();

        foreach (Transform bone in smr.bones) {
            foreach (Transform tb in hips_) {
                if (tb.name == bone.name) {
                    bonesUnderSmr.Add(tb);
                }
            }
        }

        tSmr_[part].sharedMesh = smr.sharedMesh;
        tSmr_[part].bones = bonesUnderSmr.ToArray();
        tSmr_[part].materials = smr.materials;
    }

    /// <summary>
    /// read skin mesh 
    /// </summary>
    void readMeshData() {
        SkinnedMeshRenderer[] smrs = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in smrs) {
            string[] partsName = smr.name.Split('-');
            if (!meshData.ContainsKey(partsName[0])) {
                meshData.Add(partsName[0], new Dictionary<string, SkinnedMeshRenderer>());
            }
            meshData[partsName[0]].Add(partsName[1], smr);
        }
    }


    void rebuildTarget() {
        SkinnedMeshRenderer targetSmr = target.gameObject.AddComponent<SkinnedMeshRenderer>();
        List<Material> targetMaterials = new List<Material>();
        List<Transform> targetBones = new List<Transform>();
        List<CombineInstance> targetCombineInstances = new List<CombineInstance>();
        Transform[] hips = target.GetComponentsInChildren<Transform>();

        foreach (KeyValuePair<string, Dictionary<string, SkinnedMeshRenderer>> ssmr in meshData) {
            SkinnedMeshRenderer smr = new SkinnedMeshRenderer();
            switch (ssmr.Key) {
                case "eyes":
                    smr = ssmr.Value["1"];
                    break;
                default:
                    smr = ssmr.Value["2"];
                    break;
            }

            targetMaterials.AddRange(smr.materials);

            CombineInstance ci = new CombineInstance();
            ci.mesh = smr.sharedMesh;
            targetCombineInstances.Add(ci);

            foreach (Transform sbone in smr.bones) {
                foreach (Transform tbone in hips) {
                    if (tbone.name == sbone.name) {
                        targetBones.Add(tbone);
                        break;
                    }
                }
            }
        }

        targetSmr.sharedMesh = new Mesh();
        targetSmr.sharedMesh.CombineMeshes(targetCombineInstances.ToArray(), false, false);

        targetSmr.bones = targetBones.ToArray();

        targetSmr.materials = targetMaterials.ToArray();
    }

    void onGUI() {
        float scaleW = 1;
        float scaleH = 1;

        GUI.skin.button.fontSize = (int)(25 * scaleW);

        if (GUI.Button(new Rect(70 * scaleW, 50 * scaleH, 90 * scaleW, 40 * scaleH), "face")) {
            changePart("face", "2");
        }
        if (GUI.Button(new Rect(70 * scaleW, 110 * scaleH, 90 * scaleW, 40 * scaleH), "hair")) {
            changePart("hair", "2");
        }
        if (GUI.Button(new Rect(70 * scaleW, 170 * scaleH, 220 * scaleW, 40 * scaleH), "pant")) {
            changePart("pant", "2");
        }
        if (GUI.Button(new Rect(70 * scaleW, 230 * scaleH, 220 * scaleW, 40 * scaleH), "shoes")) {
            changePart("shoes", "2");
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
