using System;

namespace TestModelLib
{
    public class Person
    {
        private int _id;
        private string _name;
        private byte _age;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public byte Age
        {
            get => _age;
            set => _age = value;
        }

        public Person()
        {
            
        }

        public Person(int id, string name, byte age)
        {
            _id = id;
            _name = name;
            _age = age;
        }
    }
}
