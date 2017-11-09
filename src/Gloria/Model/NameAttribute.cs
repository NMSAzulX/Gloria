using System;

namespace Gloria
{
    public class NameAttribute :Attribute
    {
        public string Name;
        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
