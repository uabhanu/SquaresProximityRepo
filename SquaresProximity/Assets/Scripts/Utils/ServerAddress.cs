namespace Utils
{
    using System;
    
    public class ServerAddress : IEquatable<ServerAddress>
    {
        private int _port;
        private string _ip;

        public string IP => _ip;
        public int Port => _port;

        public ServerAddress(string ip , int port)
        {
            _ip = ip;
            _port = port;
        }

        public override string ToString()
        {
            return $"{_ip}:{_port}";
        }

        public bool Equals(ServerAddress other)
        {
            if(ReferenceEquals(null , other)) return false;
            if(ReferenceEquals(this , other)) return true;
            return _ip == other._ip && _port == other._port;
        }

        #pragma warning disable CS0659
            public override bool Equals(object obj)
        #pragma warning restore CS0659
        {
            if(ReferenceEquals(null , obj)) return false;
            if(ReferenceEquals(this , obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((ServerAddress)obj);
        }
    }
}