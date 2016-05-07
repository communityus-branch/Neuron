using Static_Interface.API.PlayerFramework;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.UnityExtensions
{
    public abstract class MonoBehaviour : UnityEngine.MonoBehaviour
    {
        protected internal virtual bool ForceSafeDestroy => false;
        internal bool BlockOnDestroy;
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// <para/>
        /// Awake is used to initialize any variables or game state before the game starts.Awake is called only once during the lifetime of the script instance. Awake is called after all objects are initialized so you can safely speak to other objects or query them using eg. GameObject.FindWithTag. Each GameObject's Awake is called in a random order between objects. Because of this, you should use Awake to set up references between scripts, and use Start to pass any information back and forth. Awake is always called before any Start functions. This allows you to order initialization of scripts. Awake can not act as a coroutine.
        /// <para/>
        /// Note for C# and Boo users: use Awake instead of the constructor for initialization, as the serialized state of the component is undefined at construction time. Awake is called once, just like the constructor.
        /// </summary>
        [UnityMessage]
        protected virtual void Awake()
        {

        }

        /// <summary>
        /// This function is called every fixed framerate frame, if the <see cref="UnityEngine.MonoBehaviour"/>is enabled.
        /// <para/>
        /// FixedUpdate should be used instead of Update when dealing with <see cref="Rigidbody"/>. For example when adding a force to a rigidbody, you have to apply the force every fixed frame inside FixedUpdate instead of every frame inside Update.
        /// <para/>
        /// In order to get the elapsed time since last call to FixedUpdate, use Time.<see cref="Time.deltaTime"/>. This function is only called if the <see cref="Behaviour"/> is enabled. Override this function in order to provide your component's functionality.
        /// </summary>
        [UnityMessage]
        protected virtual void FixedUpdate()
        {

        }

        /// <summary>
        /// Callback for setting up animation IK (inverse kinematics).
        /// <para/>
        /// OnAnimatorIK() is called by the Animator Component immediately before it updates its internal IK system.This callback can be used to set the positions of the IK goals and their respective weights.
        /// <para/>
        /// See Also: Animator.<see cref="Animator.SetIKPosition"/>, Animator.<see cref="Animator.SetIKPositionWeight"/>, Animator.<see cref="Animator.SetIKRotation"/>, Animator.<see cref="Animator.SetIKRotationWeight"/>.
        /// </summary>
        /// <param name="layerIndex">The index of the layer on which the IK solver is called.</param>
        [UnityMessage]
        protected virtual void OnAnimatorIK(int layerIndex)
        {

        }

        /// <summary>
        /// Callback for processing animation movements for modifying root motion.
        /// <para/>
        /// This callback will be invoked at each frame after the state machines and the animations have been evaluated, but before OnAnimatorIK.
        /// <para/>
        /// See Also: <see href="http://docs.unity3d.com/Manual/ScriptingRootMotion.html">Root motion</see>.
        /// </summary>
        [UnityMessage]
        protected virtual void OnAnimatorMove()
        {

        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
        /// <para/>
        /// Like the Awake function, Start is called exactly once in the lifetime of the script.However, Awake is called when the script object is initialised, regardless of whether or not the script is enabled.Start may not be called on the same frame as Awake if the script is not enabled at initialisation time.
        /// <para/>
        /// The Awake function is called on all objects in the scene before any object's Start function is called. This fact is useful in cases where object A's initialisation code needs to rely on object B's already being initialised; B's initialisation should be done in Awake while A's should be done in Start.
        /// <para/>
        /// Where objects are instantiated during gameplay, their Awake function will naturally be called after the Start functions of scene objects have already completed.
        /// </summary>
        [UnityMessage]
        protected virtual void Start()
        {
            
        }

        /// <summary>
        /// This function is called after a new level was loaded.
        /// <para/>
        /// OnLevelWasLoaded can be a co-routine, simply use the yield statement in the function.
        /// </summary>
        /// <param name="level">level is the index of the level that was loaded.Use the menu item File->Build Settings... to see what scene the index refers to. See Also: Application.LoadLevel.</param>
        [UnityMessage]
        protected virtual void OnLevelWasLoaded(int level)
        {
            
        }

        /// <summary>
        /// Sent to all game objects before the application is quit.
        /// <para/>
        /// In the editor this is called when the user stops playmode. In the web player it is called when the web view is closed.
        /// <para/>
        /// Note that iOS applications are usually suspended and do not quit. You should tick "Exit on Suspend" in Player settings for iOS builds to cause the game to quit and not suspend, otherwise you may not see this call. If "Exit on Suspend" is not ticked then you will see calls to <see cref="OnApplicationPause"/> instead.
        /// <para/>
        /// Note: On Windows Store Apps and Windows Phone 8.1 there's no application quit event, consider using <see cref="OnApplicationFocus"/> event when focusStatus equals false.
        /// </summary>
        [UnityMessage]
        protected virtual void OnApplicationQuit()
        {
            
        }

        /// <summary>
        /// Sent to all game objects when the player pauses.
        /// <para/>
        /// OnApplicationPause can be a co-routine, simply use the yield statement in the function. If it is implemented as a coroutine, it will be evaluated twice during the initial frame: first as an early notification and second time during the normal coroutine update step.
        /// </summary>
        [UnityMessage]
        protected virtual void OnApplicationPause(bool pauseStatus)
        {
            
        }

        /// <summary>
        /// Sent to all game objects when the player gets or loses focus.
        /// <para/>
        /// OnApplicationFocus can be a co-routine, simply use the yield statement in the function.If it is implemented as a coroutine, it will be evaluated twice during the initial frame: first as an early notification and second time during the normal coroutine update step.
        /// </summary>
        /// <param name="focusStatus"></param>
        [UnityMessage]
        protected virtual void OnApplicationFocus(bool focusStatus)
        {
            
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// <para/>
        /// OnEnable cannot be a co-routine.
        /// </summary>
        [UnityMessage]
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// <para/>
        /// This is also called when the object is destroyed and can be used for any cleanup code.When scripts are reloaded after compilation has finished, OnDisable will be called, followed by an OnEnable after the script has been loaded.     
        /// OnDisable cannot be a co-routine.
        /// </summary>
        [UnityMessage]
        protected virtual void OnDisable()
        {

        }

        /// <summary>
        /// This function is called when the <see cref="UnityEngine.MonoBehaviour"/> will be destroyed.
        /// <para/>
        /// OnDestroy will only be called on game objects that have previously been active.
        /// <para/>
        /// OnDestroy cannot be a co-routine.
        /// </summary>
        [UnityMessage]
        protected internal virtual void OnDestroy()
        {
            if (ForceSafeDestroy || !BlockOnDestroy)
            {
                OnDestroySafe();
            }
        }

        /// <summary>
        /// Extensions may only use this method instead of <see cref="OnDestroy"/>.
        /// </summary>
        protected virtual void OnDestroySafe()
        {
            
        }

        /// <summary>
        /// LateUpdate is called every frame, if the <see cref="Behaviour"/> is enabled.
        ///  <para />
        ///  LateUpdate is called after all Update functions have been called.This is useful to order script execution.For example a follow camera should always be implemented in LateUpdate because it tracks objects that might have moved inside Update.
        /// <para/>
        /// In order to get the elapsed time since last call to LateUpdate, use Time.<see cref="Time.deltaTime"/>. This function is only called if the <see cref="Behaviour"/> is enabled. Override this function in order to provide your component's functionality.
        /// </summary>
        [UnityMessage]
        protected virtual void LateUpdate()
        {

        }
        /// <summary>
        /// OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider.
        /// <para/>
        /// In contrast to <see cref="OnTriggerEnter"/>, OnCollisionEnter is passed the <see cref="Collision "/> class and not a <see cref="Collider"/>. The<see cref="Collision"/> class contains information about contact points, impact velocity etc.If you don't use collisionInfo in the function, leave out the collisionInfo parameter as this avoids unneccessary calculations. Notes: Collision events are only sent if one of the colliders also has a non-kinematic rigidbody attached. Collision events will be sent to disabled <see cref="UnityEngine.MonoBehaviour"/>s, to allow enabling Behaviours in response to collisions.
        /// </summary>
        /// <para/>
        /// OnCollisionEnter can be a co-routine, simply use the yield statement in the function.
        [UnityMessage]
        protected virtual void OnCollisionEnter(Collision collision)
        {
            
        }

        /// <summary>
        /// OnCollisionExit is called when this collider/rigidbody has stopped touching another rigidbody/collider.
        /// <para/>
        /// In contrast to <see cref="OnTriggerExit"/>, OnCollisionExit is passed the <see cref="Collision "/> class and not a <see cref="Collider"/>. The<see cref="Collision"/> class contains information about contact points, impact velocity etc.If you don't use collisionInfo in the function, leave out the collisionInfo parameter as this avoids unneccessary calculations. Notes: Collision events are only sent if one of the colliders also has a non-kinematic rigidbody attached. Collision events will be sent to disabled <see cref="UnityEngine.MonoBehaviour"/>s, to allow enabling Behaviours in response to collisions.
        /// </summary>
        /// <para/>
        /// OnCollisionExit can be a co-routine, simply use the yield statement in the function.
        [UnityMessage]
        protected virtual void OnCollisionExit(Collision collision)
        {

        }

        /// <summary>
        /// OnCollisionStay is called once per frame for every collider/rigidbody that is touching rigidbody/collider.
        /// <para/>
        /// In contrast to <see cref="OnTriggerStay"/>, OnCollisionStay is passed the <see cref="Collision "/> class and not a <see cref="Collider"/>. The<see cref="Collision"/> class contains information about contact points, impact velocity etc.If you don't use collisionInfo in the function, leave out the collisionInfo parameter as this avoids unneccessary calculations. Notes: Collision events are only sent if one of the colliders also has a non-kinematic rigidbody attached. Collision events will be sent to disabled <see cref="UnityEngine.MonoBehaviour"/>s, to allow enabling Behaviours in response to collisions. Collision stay events are not sent for sleeping Rigidbodies.
        /// <para/>
        /// OnCollisionStay can be a co-routine, simply use the yield statement in the function.
        /// </summary>
        [UnityMessage]
        protected virtual void OnCollisionStay(Collision collision)
        {

        }

        /// <summary>
        /// OnGUI is called for rendering and handling GUI events.
        /// <para/>
        /// This means that your OnGUI implementation might be called several times per frame(one call per event). For more information on GUI events see the <see cref="Event"/> reference. If the <see cref="UnityEngine.MonoBehaviour"/>'s enabled property is set to false, OnGUI() will not be called. 
        /// <para/>
        /// For more information, see the <see href="http://docs.unity3d.com/Manual/GUIScriptingGuide.html">GUI Scripting Guide</see>.
        /// </summary>
        [UnityMessage]
        protected virtual void OnGUI()
        {
            
        }

        /// <summary>
        /// OnTriggerEnter is called when the <see cref="Collider"/> other enters the trigger.
        /// <para/>
        /// This message is sent to the trigger collider and the rigidbody(or the collider if there is no rigidbody) that touches the trigger.Notes: Trigger events are only sent if one of the colliders also has a rigidbody attached.Trigger events will be sent to disabled <see cref="UnityEngine.MonoBehaviour"/>s, to allow enabling Behaviours in response to collisions.
        /// <para/>
        /// OnTriggerEnter can be a co-routine, simply use the yield statement in the function.
        /// </summary>
        [UnityMessage]
        protected virtual void OnTriggerEnter()
        {

        }

        /// <summary>
        /// OnTriggerExit is called when the <see cref="Collider"/> other has stopped touching the trigger.
        /// <para/>
        /// This message is sent to the trigger and the collider that touches the trigger.Notes: Trigger events are only sent if one of the colliders also has a rigidbody attached.Trigger events will be sent to disabled <see cref="UnityEngine.MonoBehaviour"/>s, to allow enabling Behaviours in response to collisions.
        /// <para/>
        /// OnTriggerExit can be a co-routine, simply use the yield statement in the function.
        /// </summary>
        [UnityMessage]
        protected virtual void OnTriggerExit()
        {

        }

        /// <summary>
        /// OnTriggerStay is called once per frame for every <see cref="Collider"/> other that is touching the trigger.
        /// <para/>
        /// This message is sent to the trigger and the collider that touches the trigger.Notes: Trigger events are only sent if one of the colliders also has a rigidbody attached.Trigger events will be sent to disabled <see cref="UnityEngine.MonoBehaviour"/>s, to allow enabling Behaviours in response to collisions.
        /// <para/>
        /// OnTriggerStay can be a co-routine, simply use the yield statement in the function.
        /// </summary>
        [UnityMessage]
        protected virtual void OnTriggerStay()
        {

        }

        /// <summary>
        /// Reset to default values.
        /// <para/>
        /// Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time. This function is only called in editor mode. Reset is most commonly used to give good default values in the inspector.
        /// </summary>
        [UnityMessage]
        protected virtual void Reset()
        {
            
        }

        [UnityMessage]
        protected virtual void OnPlayerLoaded()
        {
            
        }

        /// <summary>
        /// Update is called every frame, if the <see cref="UnityEngine.MonoBehaviour"/> is enabled.
        /// <para/>
        /// Update is the most commonly used function to implement any kind of game behaviour.
        /// <para/>
        /// In order to get the elapsed time since last call to Update, use Time.deltaTime. This function is only called if the Behaviour is enabled. Override this function in order to provide your component's functionality.    
        /// </summary>
        [UnityMessage]
        protected virtual void Update()
        {
            if (Connection.IsServer())
            {
                UpdateServer();
            }
            if (Connection.IsClient())
            {
                UpdateClient();
            }
        }

        protected virtual void UpdateServer()
        {
            
        }

        protected virtual void UpdateClient()
        {
            
        }


        protected virtual void OnResolutionChanged(Vector2 newRes)
        {
            
        }
    }
}