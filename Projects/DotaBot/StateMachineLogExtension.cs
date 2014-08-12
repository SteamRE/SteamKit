// File Name:      StateMachineLogExtension.cs
// Project:           DotaBot
// Copyright (c) christian stewart 2014
// 
// All rights reserved.

using System;
using System.Globalization;
using System.Reflection;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Extensions;
using Appccelerate.StateMachine.Machine;
using log4net;

namespace Appccelerate.SourceTemplates.Log4Net
{
    public class StateMachineLogExtension<TState, TEvent> : ExtensionBase<TState, TEvent>
        where TState : struct, IComparable
        where TEvent : struct, IComparable
    {
        private readonly ILog log;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StateMachineLogExtension{TState,TEvent}" /> class.
        /// </summary>
        public StateMachineLogExtension()
        {
            log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StateMachineLogExtension{TState,TEvent}" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public StateMachineLogExtension(string logger)
        {
            log = LogManager.GetLogger(logger);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StateMachineLogExtension{TState,TEvent}" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public StateMachineLogExtension(ILog logger)
        {
            log = logger;
        }

        /// <summary>
        ///     Called after the state machine switched states.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="oldState">The old state.</param>
        /// <param name="newState">The new state.</param>
        public override void SwitchedState(IStateMachineInformation<TState, TEvent> stateMachine,
            IState<TState, TEvent> oldState, IState<TState, TEvent> newState)
        {
            log.InfoFormat(
                "State machine {0} switched from state {1} to state {2}.",
                stateMachine,
                oldState,
                newState);
        }

        /// <summary>
        ///     Called when the state machine is initializing.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="initialState">The initial state. Can be replaced by the extension.</param>
        public override void InitializingStateMachine(IStateMachineInformation<TState, TEvent> stateMachine,
            ref TState initialState)
        {
            log.InfoFormat("State machine {0} initializes to state {1}.", stateMachine, initialState);
        }

        /// <summary>
        ///     Called when the state machine was initialized.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="initialState">The initial state.</param>
        public override void InitializedStateMachine(IStateMachineInformation<TState, TEvent> stateMachine,
            TState initialState)
        {
            log.InfoFormat("State machine {0} initialized to state {1}", stateMachine, initialState);
        }

        /// <summary>
        ///     Called when the state machine enters the initial state.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="state">The state.</param>
        public override void EnteringInitialState(IStateMachineInformation<TState, TEvent> stateMachine, TState state)
        {
            log.InfoFormat("State machine {0} enters initialstate {1}.", stateMachine, state);
        }

        public override void EnteredInitialState(IStateMachineInformation<TState, TEvent> stateMachine, TState state,
            ITransitionContext<TState, TEvent> context)
        {
            Ensure.ArgumentNotNull(context, "context");

            log.DebugFormat("State machine {0} performed {1}.", stateMachine, context.GetRecords());
        }

        /// <summary>
        ///     Called when an event is firing on the state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="eventId">The event id. Can be replaced by the extension.</param>
        /// <param name="eventArgument">The event argument. Can be replaced by the extension.</param>
        public override void FiringEvent(IStateMachineInformation<TState, TEvent> stateMachine, ref TEvent eventId,
            ref object eventArgument)
        {
            Ensure.ArgumentNotNull(stateMachine, "stateMachine");

            if (log.IsInfoEnabled)
            {
                log.InfoFormat(
                    CultureInfo.InvariantCulture,
                    "Fire event {0} on state machine {1} with current state {2} and event argument {3}.",
                    eventId,
                    stateMachine.Name,
                    stateMachine.CurrentStateId,
                    eventArgument);
            }
        }

        /// <summary>
        ///     Called when an event was fired on the state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="context">The transition context.</param>
        public override void FiredEvent(IStateMachineInformation<TState, TEvent> stateMachine,
            ITransitionContext<TState, TEvent> context)
        {
            Ensure.ArgumentNotNull(stateMachine, "stateMachine");
            Ensure.ArgumentNotNull(context, "context");

            if (log.IsDebugEnabled)
            {
                log.DebugFormat("State machine {0} performed {1}.", stateMachine.Name, context.GetRecords());
            }
        }

        public override void HandlingEntryActionException(IStateMachineInformation<TState, TEvent> stateMachine,
            IState<TState, TEvent> state, ITransitionContext<TState, TEvent> context, ref Exception exception)
        {
            Ensure.ArgumentNotNull(stateMachine, "stateMachine");
            Ensure.ArgumentNotNull(state, "state");

            log.ErrorFormat("Exception in entry action of state {0} of state machine {1}: {2}", state.Id,
                stateMachine.Name, exception);
        }

        public override void HandlingExitActionException(IStateMachineInformation<TState, TEvent> stateMachine,
            IState<TState, TEvent> state, ITransitionContext<TState, TEvent> context, ref Exception exception)
        {
            Ensure.ArgumentNotNull(stateMachine, "stateMachine");
            Ensure.ArgumentNotNull(state, "state");

            log.ErrorFormat("Exception in exit action of state {0} of state machine {1}: {2}", state.Id,
                stateMachine.Name, exception);
        }

        /// <summary>
        ///     Called before a guard exception is handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="transition">The transition.</param>
        /// <param name="transitionContext">The transition context.</param>
        /// <param name="exception">The exception. Can be replaced by the extension.</param>
        public override void HandlingGuardException(IStateMachineInformation<TState, TEvent> stateMachine,
            ITransition<TState, TEvent> transition, ITransitionContext<TState, TEvent> transitionContext,
            ref Exception exception)
        {
            Ensure.ArgumentNotNull(stateMachine, "stateMachine");

            log.ErrorFormat("Exception in guard of transition {0} of state machine {1}: {2}", transition,
                stateMachine.Name, exception);
        }

        /// <summary>
        ///     Called before a transition exception is handled.
        /// </summary>
        /// <param name="stateMachine">The state machine.</param>
        /// <param name="transition">The transition.</param>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception. Can be replaced by the extension.</param>
        public override void HandlingTransitionException(IStateMachineInformation<TState, TEvent> stateMachine,
            ITransition<TState, TEvent> transition, ITransitionContext<TState, TEvent> context, ref Exception exception)
        {
            Ensure.ArgumentNotNull(stateMachine, "stateMachine");

            log.ErrorFormat("Exception in action of transition {0} of state machine {1}: {2}", transition,
                stateMachine.Name, exception);
        }
    }
}