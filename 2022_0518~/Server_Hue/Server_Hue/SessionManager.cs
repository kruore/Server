using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Server_Hue
{
    internal class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager instance { get { return _session; } }
        Dictionary<int, GameSession> _sessions = new Dictionary<int, GameSession>();
        object _lock = new object();

        public GameSession Generate()
        {
            lock (_lock)
            {
                GameSession session = new GameSession();
                if(((IPEndPoint)Program.ipPoint).Address.ToString()=="127.0.0.1")
                {
                    session.SessionID = 0;
                }
                else
                {
                    session.SessionID = 1;
                }
                if (_sessions.ContainsKey(session.SessionID))
                {
                    
                    _sessions.Remove(session.SessionID);
                    _sessions.Add(session.SessionID, session);
                }
                else
                {
                    _sessions.Add(session.SessionID, session);
                }
                return session;
            }
        }

        public GameSession Find(int id)
        {
            lock (_lock)
            {
                GameSession session = null;
                // _sessions.TryGetValue(id, out session);
                Console.WriteLine($"Session egist{id}");
                return session;
            }
        }

        public void Remove(GameSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session.SessionID);
            }
        }
    }
}
