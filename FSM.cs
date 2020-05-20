    /// <summary>
    /// 状态机接口
    /// </summary>
    public interface IStateMachine {
        /// <summary>
        /// 当前状态
        /// </summary>
        /// <value>状态机的当前状态</value>
        IState CurrentState { get; }

        /// <summary>
        /// 默认状态
        /// </summary>
        /// <value>状态机的默认状态</value>
        IState DefaultState { set; get; }

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="state">要添加的状态</param>
        void AddState (IState state);

        /// <summary>
        /// 删除状态
        /// </summary>
        /// <param name="state">要删除的状态</param>
        void RemoveState (IState state);
    }

    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IState {
        /// <summary>
        /// 状态名
        /// </summary>
        /// <value></value>
        string Name { get; }

        /// <summary>
        /// 当前状态的状态机
        /// </summary>
        /// <value></value>
        IStateMachine StateMachine { get; set; }

        /// <summary>
        /// 该状态从开始到现在的时间
        /// </summary>
        /// <value></value>
        float Timer { get; }

        /// <summary>
        /// 状态过渡
        /// </summary>
        /// <value>当前状态的所有过渡</value>
        List<ITransition> Transitions { get; }
        /// <summary>
        /// 进入状态时的回调
        /// </summary>
        /// <param name="prev">上一个状态</param>
        void EnterCallback (IState prev);

        /// <summary>
        /// 退出状态时的回调
        /// </summary>
        /// <param name="next">下一个状态</param>
        void ExitCallback (IState next);

        /// <summary>
        /// Update的回调
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        void UpdateCallback (float deltaTime);

        /// <summary>
        /// LateUpdate的回调
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        void LateUpdateCallback (float deltaTime);

        /// <summary>
        /// FixedUpdate的回调
        /// </summary>
        void FixedUpdateCallback ();

        /// <summary>
        /// 添加过渡
        /// </summary>
        /// <param name="transition">状态过渡</param>
        void AddTransition (ITransition transition);
    }

    /// <summary>
    /// 过渡接口
    /// </summary>
    public interface ITransition {
        /// <summary>
        /// The State which transition translate from
        /// </summary>
        /// <value></value>
        IState From { get; set; }

        /// <summary>
        /// The State which transition translate to
        /// </summary>
        /// <value></value>
        IState To { get; set; }

        /// <summary>
        /// Chenck If Transition Complete
        /// </summary>
        /// <returns>是否过渡结束</returns>
        bool TransitionCallback ();

        /// <summary>
        /// 能否开始过渡
        /// </summary>
        /// <returns>if true,transition begin.if false dont</returns>
        bool ShouldBegin ();
    }

    ///有限状态机
    public class FSMMachine : State, IStateMachine {

        /// <summary>
        /// CurrentState
        /// </summary>
        /// <value>CurrentState</value>
        public IState CurrentState {
            get {
                return currentState;
            }
        }

        /// <summary>
        /// DefaultState
        /// </summary>
        /// <value>DefaultState</value>
        public IState DefaultState {
            get {
                return defaultState;
            }
            set {
                AddState (value);
                defaultState = value;
            }
        }

        private IState defaultState;
        private IState currentState;
        private List<IState> states;
        private bool isTranslating = false;

        private ITransition currentTransition; //当前正在执行的ITransition
        /// <summary>
        /// FSMMachine
        /// </summary>
        /// <param name="name"></param>
        public FSMMachine (string name, IState defaultState) : base (name) {
            states = new List<IState> ();
            this.defaultState = defaultState;
        }
        /// <summary>
        /// Add State
        /// </summary>
        /// <param name="state">state</param>
        public void AddState (IState state) {
            if (states == null || states.Contains (state))
                return;
            states.Add (state);
            state.StateMachine = this;
            if (defaultState == null)
                defaultState = state;
        }

        /// <summary>
        /// Remove State
        /// </summary>
        /// <param name="state">state</param>
        public void RemoveState (IState state) {
            if (currentState == state)
                return;
            if (states == null || !states.Contains (state))
                return;
            states.Remove (state);
            state.StateMachine = null;
            if (defaultState == state) {
                defaultState = states.Count >= 1 ? states[0] : null;
            }
        }

        public override void UpdateCallback (float deltaTime) {
            if (isTranslating) {
                if (currentTransition.TransitionCallback ()) {
                    DoTransition (currentTransition);
                    isTranslating = false;
                }
                return;
            }
            base.UpdateCallback (deltaTime);
            if (currentState == null)
                currentState = defaultState;
            foreach (var transition in currentState.Transitions) {
                if (transition.ShouldBegin ()) {
                    currentTransition = transition;
                    isTranslating = true;
                    return;
                }
            }
            currentState.UpdateCallback (deltaTime);
        }

        public override void LateUpdateCallback (float deltaTime) {
            if (isTranslating)
                return;
            base.LateUpdateCallback (deltaTime);
            currentState.LateUpdateCallback (deltaTime);
        }

        public override void FixedUpdateCallback () {
            if (isTranslating)
                return;
            base.FixedUpdateCallback ();
            currentState.FixedUpdateCallback ();
        }

        //开始进行过渡
        private void DoTransition (ITransition transition) {
            currentState.ExitCallback (transition.To);
            currentState = transition.To;
            currentState.EnterCallback (transition.From);
        }
    }

    public class State : IState {
        public string Name { get; private set; }

        /// <summary>
        /// 当前状态的状态机
        /// </summary>
        /// <value></value>
        public IStateMachine StateMachine {
            get { return stateMachine; }
            set { stateMachine = value; }
        }

        public float Timer { get; private set; }

        public List<ITransition> Transitions {
            get {
                return transitions;
            }
        }

        private IStateMachine stateMachine;
        private List<ITransition> transitions;
        /// <summary>
        /// 当进入状态时调用的事件
        /// </summary>
        public event Action<IState> OnEnter;

        /// <summary>
        /// 当离开状态时调用的事件
        /// </summary>
        public event Action<IState> OnExit;

        /// <summary>
        /// 当Update时调用的事件
        /// </summary>
        public event Action<float> OnUpdate;

        /// <summary>
        /// 当LateUpdate时调用的事件
        /// </summary>
        public event Action<float> OnLateUpdate;

        /// <summary>
        /// 当FixedUpdate时调用的事件
        /// </summary>
        public event Action OnFixedUpdate;
        public State (string name) {
            Name = name;
            transitions = new List<ITransition> ();
        }

        /// <summary>
        /// 添加过渡
        /// </summary>
        /// <param name="transition">状态过渡</param>
        public void AddTransition (ITransition transition) {
            if (transitions == null || transitions.Contains (transition))
                return;

            transitions.Add (transition);
        }

        /// <summary>
        /// 进入状态时的回调
        /// </summary>
        /// <param name="prev">上一个状态</param>
        public virtual void EnterCallback (IState prev) {
            OnEnter?.Invoke (prev);
            //重置计时器
            Timer = 0f;

        }

        /// <summary>
        /// 退出状态时的回调
        /// </summary>
        /// <param name="next">下一个状态</param>
        public virtual void ExitCallback (IState next) {
            OnExit?.Invoke (next);
            //重置计时器
            Timer = 0f;
        }

        /// <summary>
        /// Update的回调
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        public virtual void UpdateCallback (float deltaTime) {
            OnUpdate?.Invoke (deltaTime);
            //累计计时器
            Timer += deltaTime;
        }

        /// <summary>
        /// LateUpdate的回调
        /// </summary>
        /// <param name="deltaTime">Time.deltaTime</param>
        public virtual void LateUpdateCallback (float deltaTime) {
            OnLateUpdate?.Invoke (deltaTime);
        }

        /// <summary>
        /// FixedUpdate的回调
        /// </summary>
        public virtual void FixedUpdateCallback () {
            OnFixedUpdate?.Invoke ();
        }
    }

    public class Transition : ITransition {
        public event Func<bool> OnTransition;
        public event Func<bool> OnCheck;

        /// <summary>
        /// The State which transition translate from
        /// </summary>
        /// <value></value>
        public IState From { get; set; }

        /// <summary>
        /// The State which transition translate to
        /// </summary>
        /// <value></value>
        public IState To { get; set; }

        public Transition (IState from, IState to) {
            this.From = from;
            this.To = to;
        }

        public bool TransitionCallback () {
            if (OnTransition != null)
                return OnTransition ();

            return true;
        }

        public bool ShouldBegin () {
            if (OnCheck != null)
                return OnCheck ();
            return false;
        }
    }
