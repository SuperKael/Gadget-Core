using System;
using System.Linq;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom Object. Make sure to call Register on it to register your Object.
    /// </summary>
    public class ObjectInfo : RegistryEntry<ObjectInfo, ObjectType>
    {
        /// <summary>
        /// The ObjectType of this Object.
        /// </summary>
        public readonly ObjectType Type;

        /// <summary>
        /// The the Item to drop when harvested.
        /// </summary>
        public readonly Item ItemDrop;
        /// <summary>
        /// Up to this many extra items may be dropped for each "hit" of the resource.
        /// </summary>
        public readonly int RandomDropBonus;
        /// <summary>
        /// The offset of this Object's harvest collider.
        /// </summary>
        public readonly Vector2 ColliderOffset;

        /// <summary>
        /// The Texture associated with this Object. May be null.
        /// </summary>
        public virtual Texture Tex { get; protected set; }
        /// <summary>
        /// The Texture associated with this Object's Fly's head. May be null.
        /// </summary>
        public virtual Texture FlyHeadTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this Object's Fly's wing. May be null.
        /// </summary>
        public virtual Texture FlyWingTex { get; protected set; }

        /// <summary>
        /// The Material associated with this Object. May be null.
        /// </summary>
        public virtual Material Mat { get; protected set; }
        /// <summary>
        /// The Material associated with this Object's Fly's head. May be null.
        /// </summary>
        public virtual Material FlyHeadMat { get; protected set; }
        /// <summary>
        /// The Material associated with this Object's Fly's wing. May be null.
        /// </summary>
        public virtual Material FlyWingMat { get; protected set; }

        /// <summary>
        /// The GameObject representing this Object. This is registered as a prefab.
        /// </summary>
        public virtual GameObject Object { get; protected set; }
        /// <summary>
        /// The string usable in <see cref="Resources.Load(string)"/> to load this object.
        /// </summary>
        public virtual string ResourcePath { get; protected set; }

        /// <summary>
        /// Use to create a new ObjectInfo. Make sure to call Register on it to register your Object.
        /// </summary>
        public ObjectInfo(ObjectType Type, Item ItemDrop, int RandomDropBonus, Texture Tex, Texture FlyHeadTex = null, Texture FlyWingTex = null, Vector2 ColliderOffset = default)
        {
            this.Type = Type;
            this.ItemDrop = ItemDrop;
            this.RandomDropBonus = RandomDropBonus;
            this.Tex = Tex;
            this.FlyHeadTex = FlyHeadTex;
            this.FlyWingTex = FlyWingTex;
            this.ColliderOffset = ColliderOffset;
        }

        /// <summary>
        /// Use to create a new ObjectInfo. Make sure to call Register on it to register your Object.
        /// </summary>
        public ObjectInfo(ObjectType Type, Item ItemDrop, int RandomDropBonus, Material Mat, Material FlyHeadMat = null, Material FlyWingMat = null, Vector2 ColliderOffset = default)
        {
            this.Type = Type;
            this.ItemDrop = ItemDrop;
            this.RandomDropBonus = RandomDropBonus;
            this.Mat = Mat;
            this.FlyHeadMat = FlyHeadMat;
            this.FlyWingMat = FlyWingMat;
            this.ColliderOffset = ColliderOffset;
        }

        /// <summary>
        /// Prompts this resource node to drop one hit's worth of items.
        /// </summary>
        public virtual void DropItem(Vector3 pos)
        {
            GameObject drop = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), pos, Quaternion.identity);
            int[] st = GadgetCoreAPI.ConstructIntArrayFromItem(ItemDrop);
            st[1] += UnityEngine.Random.Range(0, RandomDropBonus + 1);
            if (GameScript.challengeLevel > 0)
            {
                if (UnityEngine.Random.Range(0, 200) < GameScript.challengeLevel * 2)
                {
                    Camera.main.SendMessage("AUDSPEC2", SendMessageOptions.DontRequireReceiver);
                    int[] array = new int[11];
                    array[0] = UnityEngine.Random.Range(201, 221);
                    array[1] = 1;
                    int[] value = array;
                    GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), pos, Quaternion.identity);
                    gameObject.SendMessage("InitL", value);
                }
                if (UnityEngine.Random.Range(0, 200) < GameScript.challengeLevel)
                {
                    Camera.main.SendMessage("AUDSPEC3", SendMessageOptions.DontRequireReceiver);
                    int[] array2 = new int[11];
                    array2[0] = UnityEngine.Random.Range(86, 89);
                    array2[1] = 1;
                    int[] value2 = array2;
                    GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), pos, Quaternion.identity);
                    gameObject2.SendMessage("InitL", value2);
                }
            }
            int randomBonus = UnityEngine.Random.Range(0, 90);
            if (Type == ObjectType.ORE)
            {
                randomBonus += (int)(GameScript.MODS[19] * 1.5f);
            }
            else if (Type == ObjectType.TREE || Type == ObjectType.PLANT)
            {
                randomBonus += (int)(GameScript.MODS[20] * 1.5f);
            }
            else if (Type == ObjectType.BUGSPOT)
            {
                randomBonus += (int)(GameScript.MODS[22] * 1.5f);
            }
            if (randomBonus > 110)
            {
                st[1] += 4;
            }
            else if (randomBonus > 100)
            {
                st[1] += 3;
            }
            else if (randomBonus > 90)
            {
                st[1] += 2;
            }
            else if (randomBonus > 80)
            {
                st[1] += 1;
            }
            drop.SendMessage("InitL", st);
        }

        /// <summary>
        /// Registers this ObjectInfo to the ObjectRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual ObjectInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as ObjectInfo;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            if (Mat == null)
            {
                Mat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                {
                    mainTexture = Tex
                };
                Mat.SetFloat("_Cutoff", 0.5f);
            }
            else
            {
                Tex = Mat.mainTexture;
            }
            if (FlyHeadTex != null || FlyHeadMat != null)
            {
                if (FlyHeadMat == null)
                {
                    FlyHeadMat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                    {
                        mainTexture = FlyHeadTex
                    };
                    FlyHeadMat.SetFloat("_Cutoff", 0.5f);
                }
                else
                {
                    FlyHeadTex = FlyHeadMat.mainTexture;
                }
            }
            if (FlyWingTex != null || FlyWingMat != null)
            {
                if (FlyWingMat == null)
                {
                    FlyWingMat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                    {
                        mainTexture = FlyWingTex
                    };
                    FlyWingMat.SetFloat("_Cutoff", 0.5f);
                }
                else
                {
                    FlyWingTex = FlyWingMat.mainTexture;
                }
            }

            string name;
            switch (Type)
            {
                case ObjectType.ORE:
                    name = "ore";
                    break;
                case ObjectType.TREE:
                    name = "tree";
                    break;
                case ObjectType.PLANT:
                    name = "plant";
                    break;
                case ObjectType.BUGSPOT:
                    name = "bugspot";
                    break;
                default:
                    name = "object";
                    break;
            }

            Object = UnityEngine.Object.Instantiate((GameObject)Resources.Load("obj/" + (name != "object" && FlyHeadMat == null && FlyWingMat == null ? name : "bugspot") + "0"));
            Object.name = name;

            ObjectScript script = Object.GetComponent<ObjectScript>();
            script.id = GetID();
            script.b.GetComponent<MeshRenderer>().material = Mat;
            script.GetComponent<BoxCollider>().center += (Vector3)ColliderOffset;

            if (script.b.transform.childCount > 0)
            {
                Transform fly = script.b.transform.GetChild(0).GetChild(0);
                if (fly.name == "fly")
                {
                    foreach (Transform child in fly)
                    {
                        if (child.name == "Plane")
                        {
                            if (FlyHeadMat != null) child.GetComponent<MeshRenderer>().material = FlyHeadMat;
                        }
                        else if (child.name.StartsWith("Plane"))
                        {
                            if (FlyHeadMat != null) child.GetComponent<MeshRenderer>().material = FlyWingMat;
                        }
                    }
                }
            }

            GadgetCoreAPI.AddCustomResource(ResourcePath = "obj/" + name + ID, Object);
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override ObjectType GetEntryType()
        {
            return Type;
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<ObjectInfo, ObjectType> GetRegistry()
        {
            return ObjectRegistry.Singleton;
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public override bool IsValidIDForType(int id)
        {
            return id > 0;
        }

        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID. Should skip the vanilla ID range.
        /// </summary>
        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < GetRegistry().GetIDStart() - 1) lastValidID = GetRegistry().GetIDStart() - 1;
            return ++lastValidID;
        }
    }
}
