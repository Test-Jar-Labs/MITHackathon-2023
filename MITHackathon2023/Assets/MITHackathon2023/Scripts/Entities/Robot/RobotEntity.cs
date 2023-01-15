using System;
using MITHack.Robot.Game;
using MITHack.Robot.Utils;
using UnityEngine;

namespace MITHack.Robot.Entities
{
    public class RobotEntity : GenericSingleton<RobotEntity>
    {
        #region defines
        
        public enum RobotEntityState
        {
            StateAlive,
            StateDead
        }

        public struct RobotEntityStateChangeContext
        {
            public RobotEntityState prev;
            public RobotEntityState next;
        }
        
        public delegate void RobotEntityGenericDelegate<in TContext>(TContext context);

        #endregion

        public RobotEntityGenericDelegate<RobotEntityStateChangeContext> StateChangedEvent;


        private RobotEntityState _robotEntityState = RobotEntityState.StateAlive;

        public RobotEntityState EntityState => _robotEntityState;
        
        
        private void SetState(RobotEntityState entityState)
        {
            if (_robotEntityState != entityState)
            {
                StateChangedEvent?.Invoke(new RobotEntityStateChangeContext
                {
                    prev = _robotEntityState,
                    next = entityState
                });
                _robotEntityState = entityState;
            }
        }
        
        public void Kill()
        {
            if (EntityState != RobotEntityState.StateAlive)
            {
                return;
            }
            SetState(RobotEntityState.StateDead);
            // TODO: Animation
            Revive();
        }

        public void Revive()
        {
            SetState(RobotEntityState.StateAlive);
        }

        private void OnDrawGizmos()
        {
            var gameManager = GameManager.Get();
            if (gameManager)
            {
                var currentLives = gameManager.CurrentLives;
                switch (currentLives)
                {
                    case 0:
                    {
                        Gizmos.color = Color.red;
                        break;
                    }
                    case 1:
                    {
                        Gizmos.color = new Color(1.0f,115.0f / 255.0f,0);
                        break;
                    }
                    case 2:
                    {
                        Gizmos.color = new Color(1.0f, 213.0f / 255.0f, 0.0f);
                        break;
                    }
                    case 3:
                    {
                        Gizmos.color = new Color(0.0f, 1.0f, 38.0f / 255.0f);
                        break;
                    }
                }
                Gizmos.DrawSphere(transform.position, 0.2f);
            }
        }
    }
}