using System;

namespace RedisMq.Web
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class SubscribeAttribute : Attribute
    {
        public SubscribeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}