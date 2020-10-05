﻿using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    /// <summary> Base Class to cause damage to the Animals</summary>
    public class MDamager : MonoBehaviour, IMDamager
    {
        #region Public Variables
        /// <summary>ID of the Damager. This is called on the Animator to wakes up the Damager</summary>
        [SerializeField, Tooltip("ID of the Damager")]
        protected int index = 1;

        /// <summary>Enable/Disable the Damager</summary>
        [SerializeField, Tooltip("Enable/Disable the Damager")]
        protected BoolReference m_Active = new BoolReference(true);

        /// <summary>Hit Layer to interact with Objects in case RayCast is used</summary>
        [SerializeField,Tooltip("Hit Layer to interact with Objects"),ContextMenuItem("Get Layer from Root", "GetLayerFromRoot")]
        protected LayerReference m_hitLayer = new LayerReference(-1);

        /// <summary>What to do with Triggers</summary>
        [SerializeField, Tooltip("What to do with Triggers")]
        protected QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        /// <summary>Owner. usually the Character Owns the Damager</summary>
        [SerializeField, Tooltip("Owner. usually the Character Owns the Damager")]
        protected Transform m_Owner;
       
        /// <summary>Owner. usually the Character Owns the Damager</summary>
        [Tooltip("Dont Hit any objects on the Owner's hierarchy")]
        public BoolReference dontHitOwner = new BoolReference( true);

        /// <summary>Damager can activate interactables</summary>
        [Tooltip("Damager can activate interactables")]
        public BoolReference interact = new BoolReference(true);

        /// <summary>Damager allows the Damagee to apply a reaction</summary>
        [Tooltip("Damager allows the Damagee to apply an animal reaction")]
        public BoolReference react = new BoolReference(true);

        /// <summary>Ignores Damagee Multiplier</summary>
        [Tooltip("If true the Damage Receiver will not apply its Multiplier")]
        public BoolReference ignoreDamageeM = new BoolReference(false);

        /// <summary>Stat to modify on the Damagee</summary>
        [Tooltip("Stat to modify on the Damagee")]
        public StatModifier statModifier = new StatModifier();

        /// <summary>Critical Change (0 - 1)</summary>
        [SerializeField, Tooltip("Critical Change (0 - 1)\n1 means it will be always critical")]
        protected FloatReference m_cChance = new FloatReference(0);

        /// <summary>If the Damage is critical, the Stat modifier value will be multiplied by the Critical Multiplier</summary>
        [SerializeField, Tooltip("If the Damage is critical, the Stat modifier value will be multiplied by the Critical Multiplier")]
        protected FloatReference cMultiplier = new FloatReference(2);

        [SerializeField, Tooltip("Force to Apply to RigidBodies when the Damager hit them")]
        protected FloatReference m_Force = new FloatReference(50f);

        [Tooltip("Force mode to apply to the Object that the Damager Hits")]
        public ForceMode forceMode = ForceMode.Force;
        /// <summary>Direction to Apply the Force to the Object the Damager Hits</summary>
        [Tooltip("Direction to apply the Force to the Object the Damager Hits")]
        protected Vector3 Direction = Vector3.forward;

        public TransformEvent OnHit = new TransformEvent();

        #endregion

        #region Properties
        /// <summary>Owner of the Damager</summary>
        public virtual Transform Owner { get => m_Owner; set => m_Owner = value; }

        /// <summary>Force of the Damager</summary>
        public float Force { get => m_Force; set => m_Force = value; }

      
        public LayerMask Layer { get => m_hitLayer.Value; set => m_hitLayer.Value = value; }
        public QueryTriggerInteraction TriggerInteraction  { get => triggerInteraction; set => triggerInteraction = value; }
        //{
        //    get => triggerInteraction;
        //    set
        //    {
        //        triggerInteraction = value;
        //        Debug.Log("triggerInteraction" + triggerInteraction);
        //    }
        //}


        protected void GetLayerFromRoot()
        {
            var HitMaskOwner = m_Owner.GetComponentInParent<IMLayer>();

            if (HitMaskOwner != null)
            {
                Layer = HitMaskOwner.Layer;
                Debug.Log($"{name} Layer set by its Root: {transform.root}");
                // TriggerInteraction = HitMaskOwner.TriggerInteraction;
            }
        }

        /// <summary>Does the hit was Critical</summary>
        public bool IsCritical { get; set; }


        /// <summary>If the Damage is critical, the Stat modifier value will be multiplied by the Critical Multiplier</summary>
        public float CriticalMultiplier { get => cMultiplier.Value; set => cMultiplier.Value = value; }

        /// <summary>>Critical Change (0 - 1)</summary>
        public float CriticalChance { get => m_cChance.Value; set => m_cChance.Value = value; }

        /// <summary>>Index of the Damager</summary>
        public virtual int ID => index;

        /// <summary>  Set/Get the Damager Active  </summary>
        public virtual bool Active 
        { 
            get => m_Active.Value;
            set => m_Active.Value = enabled = value; 
        }


        #endregion

        /// <summary>The Damagee does not have all the conditions to apply the Damage  </summary>
        public virtual bool IsInvalid(GameObject damagee)
        {
            if (dontHitOwner && damagee.transform.IsChildOf(Owner)) return true;   //Dont hit yourself!
            if (!MTools.Layer_in_LayerMask(damagee.layer, Layer)) return true;            //Just hit what is on the HitMask Layer
            return false;
        }

        /// <summary>  The Damagee does not have all the conditions to apply the Damage  </summary>
        public virtual bool IsInvalid(Collider damagee)
        {
            if (IsInvalid(damagee.gameObject)) return true; 
            if (damagee.isTrigger && TriggerInteraction == QueryTriggerInteraction.Ignore) return true;    //just collapse when is a collider what we are hitting
            return false;
        }

      
        /// <summary>  Applies the Damage to the Game object  </summary>
        /// <returns>is False if the other gameobject didn't had a IMDamage component attached</returns>
        protected virtual bool TryDamage(IMDamage damagee, StatModifier stat)
        {
            if (damagee != null)
            {
                var criticalStat = IsCriticalStatModifier(stat);

                damagee.ReceiveDamage(Direction, Owner, criticalStat, IsCritical, react, ignoreDamageeM);
                OnHit.Invoke(damagee.Damagee);
                return true;
            }
            return false;
        }


        protected virtual bool TryDamage(GameObject other, StatModifier stat) => TryDamage(other.FindComponent<IMDamage>(), stat);


        /// <summary>Activates the Damager in case the Damager uses a Trigger</summary>
        public virtual void DoDamage(bool value)  { }
      

        /// <summary>Damager can Activate Interactables </summary>
        protected bool TryInteract(GameObject damagee)
        {
            if (interact)
            {
                var interactable = damagee.FindComponent<IInteractable>();
                if (interactable != null && !interactable.HasInteracted)
                {
                    interactable.Interact(ID);
                    return true;
                }
            }
            return false;
        }


        /// <summary>Apply Physics to the Damageee </summary>
        protected virtual bool TryPhysics(Rigidbody rb, Collider col, Vector3 Direction, float force)
        {
            if (rb && force > 0)
            {
                if (col)
                {
                    var HitPoint = col.ClosestPoint(transform.position);
                    rb.AddForceAtPosition(Direction * force, HitPoint, forceMode);
                }
                else
                    rb.AddForce(Direction * force, forceMode);

                return true;
            }

            return false;
        }

        public void SetOwner(Transform owner) => Owner = owner;

        /// <summary>  Prepare the modifier value to change it if is critical  </summary>
        protected StatModifier IsCriticalStatModifier(StatModifier mod)
        {
            IsCritical = m_cChance > Random.value;  //Calculate if is critical

            var modifier = new StatModifier(mod);

            if (IsCritical && CriticalChance > 0)
            {
                modifier.Value = mod.Value * CriticalMultiplier;        //apply the Critical Damage
            }

            return modifier;
        }

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            statModifier = new StatModifier()
            {
                ID = MTools.GetInstance<StatID>("Health"),
                modify = StatOption.SubstractValue,
                Value = new FloatReference(10)
            };

            m_hitLayer.Variable = MTools.GetInstance<LayerVar>("Hit Layer");
            m_hitLayer.UseConstant = false;

            m_Owner = transform;
#endif
        }
    }




    ///--------------------------------INSPECTOR-------------------
    ///
#if UNITY_EDITOR
    [CustomEditor(typeof(MDamager)),CanEditMultipleObjects]
    public class MDamagerEd : Editor
    {
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        protected MonoScript script;
        protected SerializedProperty Force, forceMode, index, statModifier, onhit, dontHitOwner, owner, m_Active,
            hitLayer, triggerInteraction, m_cChance, cMultiplier, ignoreDamageeM, react, interact;

        private void OnEnable()
        {
            FindBaseProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDescription("Damager Logic");
            DrawScript();
            DrawGeneral();
            DrawPhysics();
            DrawCriticalDamage();
            DrawStatModifier();
            DrawMisc();
            DrawEvents();
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.PropertyField(onhit);
                DrawCustomEvents();
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void DrawCustomEvents()  { }
      
        protected virtual void FindBaseProperties()
        {
            script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

            index = serializedObject.FindProperty("index");
            m_Active = serializedObject.FindProperty("m_Active");
            hitLayer = serializedObject.FindProperty("m_hitLayer");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");
            dontHitOwner = serializedObject.FindProperty("dontHitOwner");
            owner = serializedObject.FindProperty("m_Owner");


            react = serializedObject.FindProperty("react");
            interact = serializedObject.FindProperty("interact");
            ignoreDamageeM = serializedObject.FindProperty("ignoreDamageeM");

            m_cChance = serializedObject.FindProperty("m_cChance");
            cMultiplier = serializedObject.FindProperty("cMultiplier");

            Force = serializedObject.FindProperty("m_Force");
            forceMode = serializedObject.FindProperty("forceMode");

            statModifier = serializedObject.FindProperty("statModifier");

            onhit = serializedObject.FindProperty("OnHit");
        }


        protected virtual void DrawMisc(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Misc", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(react);
            EditorGUILayout.PropertyField(interact);
            EditorGUILayout.PropertyField(ignoreDamageeM, new GUIContent("Ignore Damagee Mult"));
            if (drawbox) EditorGUILayout.EndVertical();
        }

        protected virtual void DrawGeneral(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(m_Active);
            EditorGUILayout.PropertyField(index);
            EditorGUILayout.PropertyField(hitLayer);
            EditorGUILayout.PropertyField(triggerInteraction);
            EditorGUILayout.PropertyField(dontHitOwner, new GUIContent("Don't hit Owner"));
          
            if (dontHitOwner.FindPropertyRelative("ConstantValue").boolValue)
            {
                EditorGUILayout.PropertyField(owner);
            }

            if (drawbox) EditorGUILayout.EndVertical();
        }

        protected virtual void DrawPhysics(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Physics", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(Force);
            EditorGUILayout.PropertyField(forceMode, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            if (drawbox) EditorGUILayout.EndVertical();
        }


        protected virtual void DrawCriticalDamage(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Critical Damage", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_cChance, new GUIContent("Chance [0-1]"), GUILayout.MinWidth(50));
            EditorGUIUtility.labelWidth = 47;
            EditorGUILayout.PropertyField(cMultiplier, new GUIContent("Mult"), GUILayout.MinWidth(50));
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();
            if (drawbox) EditorGUILayout.EndVertical();
        }


        protected virtual void DrawStatModifier(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(statModifier, new GUIContent("Stat Modifier","Which Stat will be affected on the Object to hit after Impact"), true);
            if (drawbox) EditorGUILayout.EndVertical();
        }


        protected void DrawScript()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
        }

        protected void DrawDescription(string desc )
        {
            EditorGUILayout.BeginVertical(StyleBlue);
            EditorGUILayout.HelpBox(desc, MessageType.None);
            EditorGUILayout.EndVertical();
        }
    }
#endif
}